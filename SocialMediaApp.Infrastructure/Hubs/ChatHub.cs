using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Enums;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Interface.Factory;
using System.Security.Claims;

namespace SocialMediaApp.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ICurrentOnlineUserRepository _onlineUserRepository;
        private readonly IMessageRepositoryFactory _messageRepositoryFactory;
        private readonly IGroupChatMemberRepository _groupChatMemberRepository;
        private readonly ITwosomeChatRepository _twosomeChatRepository;
        private readonly IMessageStatusForChatMemberRepository _messageStatusRepository;
        private readonly IGroupChatRepository _groupChatRepository;
        private readonly IUserRepository _userRepository;
        public ChatHub(ICurrentOnlineUserRepository onlineUserRepository,
            IMessageRepositoryFactory factory,
            ITwosomeChatRepository twosomeChatRepository,
            IMessageStatusForChatMemberRepository messageStatusRepository,
            IGroupChatMemberRepository groupChatMemberRepository,
            IGroupChatRepository groupChatRepository,
            IUserRepository userRepository)
        {
            _onlineUserRepository = onlineUserRepository;
            _messageRepositoryFactory = factory;
            _twosomeChatRepository = twosomeChatRepository;
            _messageStatusRepository = messageStatusRepository;
            _groupChatMemberRepository = groupChatMemberRepository;
            _groupChatRepository = groupChatRepository;
            _userRepository = userRepository;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _onlineUserRepository.Add(Context.ConnectionId, userId);
                await _messageStatusRepository.MakeItReseavedForOnlineUser(userId);
                var twosome = _messageRepositoryFactory.CreateRepository("TWO");
                var usersId =  await twosome.ListOfReseaverIds(userId) ;
                if (usersId?.Any() == true)
                    await Clients.Users(usersId).SendAsync("BeingOnline", userId,true);
                var group= _messageRepositoryFactory.CreateRepository("GROUP");
                var groupsId = await group.ListOfReseaverIds(userId);
                foreach(var groupId in groupsId)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());

                }
                if (groupsId?.Any() == true)
                    await Clients.Groups(groupsId).SendAsync("BeingOnline", userId,true);
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _onlineUserRepository.Delete(Context.ConnectionId, userId);
                var remaining = await _onlineUserRepository.GetAllConnectionIdOfUser(userId);
                if (!remaining.Any())
                {
                    var twoRepo = _messageRepositoryFactory.CreateRepository("TWO");
                    var usersId = await twoRepo.ListOfReseaverIds(userId);
                    if (usersId?.Any() == true)
                        await Clients.Users(usersId).SendAsync("BeingOnline", userId, false);

                    var groupRepo = _messageRepositoryFactory.CreateRepository("GROUP");
                    var groupsId = await groupRepo.ListOfReseaverIds(userId);
                    if (groupsId?.Any() == true)
                        await Clients.Groups(groupsId).SendAsync("BeingOnline", userId, false);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
        public async Task sendMessage(int chatId, bool isGroup, string message, IFormFile? image = null)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            IMessageRepository messageRepository;
            if (isGroup)
            {
                messageRepository = _messageRepositoryFactory.CreateRepository("GROUP");
                var result = await messageRepository.Add(id, new Core.DTO.MessageDTO.AddMessageDTO { ChatId = chatId, Content = message, Media = image });
                if (result.Id != 0)
                {
                    var newMessage = await messageRepository.Get(id, result.Id);
                    await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", newMessage);
                }
                else
                {
                    throw new HubException(result.Message ?? "Failed to add message");
                }
            }
            else
            {
                messageRepository = _messageRepositoryFactory.CreateRepository("TWO");
                var result = await messageRepository.Add(id, new Core.DTO.MessageDTO.AddMessageDTO { ChatId = chatId, Content = message, Media = image });
                if (result.Id != 0)
                {
                    var newMessage = await messageRepository.Get(id, result.Id);
                    var twosomeChat = await _twosomeChatRepository.GetById(id, chatId);
                    if(twosomeChat is null)
                    {
                        throw new HubException("Chat not found.");
                    }
                    var reseaverId = twosomeChat.User2.UserId;
                    var connectionIdList = await _onlineUserRepository.GetAllConnectionIdOfUser(reseaverId);
                    connectionIdList = connectionIdList.Concat(await _onlineUserRepository.GetAllConnectionIdOfUser(id)).ToList();
                    if (connectionIdList.Any())
                    {
                        await Clients.Clients(connectionIdList).SendAsync("ReceiveMessage", newMessage);
                    }
                }
                else
                {
                    throw new HubException(result.Message ?? "Failed to add message");
                }
            }
        }
        public async Task markSeen(int messageId,bool isGroup)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result =await _messageStatusRepository.MakeItSeen(id, messageId);
            if (string.IsNullOrEmpty(result.Message))
            {
                IMessageRepository messageRepository;
                if (isGroup)
                {
                    messageRepository = _messageRepositoryFactory.CreateRepository("GROUP");
                    var message = await messageRepository.Get(id, messageId);
                    if(message.Status==MessageStatusEnum.seen)
                    {
                        await Clients.Group(message.ChatId.ToString()).SendAsync("MessageSeen", messageId);
                    }
                }
                else
                {
                    messageRepository = _messageRepositoryFactory.CreateRepository("TWO");
                    var message = await messageRepository.Get(id, messageId);
                    var twosomeChat = await _twosomeChatRepository.GetById(id, message.ChatId??0);
                    if (twosomeChat is null)
                    {
                        throw new HubException("Chat not found.");
                    }
                    var reseaverId = twosomeChat.User2.UserId;
                    var connectionIdList = await _onlineUserRepository.GetAllConnectionIdOfUser(reseaverId);
                    connectionIdList = connectionIdList.Concat(await _onlineUserRepository.GetAllConnectionIdOfUser(id)).ToList();
                    await Clients.Clients(connectionIdList).SendAsync("MessageSeen", messageId);

                }
            }
        }
        public async Task typingStatus(int chatId, bool isGroup, bool isTyping)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            if (isGroup)
            {
                await Clients.OthersInGroup(chatId.ToString()).SendAsync("Typing",chatId, Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "", isTyping);
            }
            else
            {
                var twosomeChat = await _twosomeChatRepository.GetById(id,chatId);
                if (twosomeChat is null)
                {
                    throw new HubException("Chat not found.");
                }
                var reseaverId = twosomeChat.User2.UserId;
                var connectionIdList = await _onlineUserRepository.GetAllConnectionIdOfUser(reseaverId);
                await Clients.Clients(connectionIdList).SendAsync("Typing", chatId, Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "", isTyping);
            }
        }
        public async Task updateGroupName(int groupId, string name)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result=await _groupChatRepository.UpdateName(id, groupId, name);
            if (!string.IsNullOrEmpty(result.Message))
                throw new HubException(result.Message);
            await Clients.Group(groupId.ToString()).SendAsync("GroupNameUpdated", groupId, name);
        }
        public async Task updateGroupPicture(int groupId, IFormFile? picture)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result = await _groupChatRepository.UpdatePicture(id, picture, groupId);
            if (!string.IsNullOrEmpty(result.Message))
                throw new HubException(result.Message);
            var group = await _groupChatRepository.Get(id, groupId);
            await Clients.Group(groupId.ToString()).SendAsync("GroupPictureUpdated", groupId, group.GroupPicture);
        }
        public async Task leaveGroup(int memberId,int groupId)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result=await _groupChatMemberRepository.Delete(id, memberId);
            if (!string.IsNullOrEmpty(result.Message))
                throw new HubException(result.Message);
            var connectionIds = await _onlineUserRepository.GetAllConnectionIdOfUser(id);
            await Clients.Group(groupId.ToString()).SendAsync("MemberLeft", groupId, Context.User?.FindFirst(ClaimTypes.Name)?.Value??"");
            foreach (var connectionId in connectionIds)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupId.ToString());
            }
        }
        public async Task addMemberToGroup(int groupId, string userId)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result = await _groupChatMemberRepository.Add(id, new AddGroupChatMemberDTO { ChatId = groupId, UserId = userId });
            if (!string.IsNullOrEmpty(result.Message))
                throw new HubException(result.Message);
            var connectionIds = await _onlineUserRepository.GetAllConnectionIdOfUser(userId);
            foreach (var connectionId in connectionIds)
            {
                await Groups.AddToGroupAsync(connectionId, groupId.ToString());
            }
            var user =await _userRepository.GetUser(userId);
            if (user is null)
                throw new HubException("user no found");
            await Clients.Group(groupId.ToString()).SendAsync("NewMemberAdded", groupId, user.Name);
        }
        public async Task removeMemberFromGroup(int groupId, int memberId,string userId) { 
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            var result = await _groupChatMemberRepository.Delete(id, memberId);
            if (!string.IsNullOrEmpty(result.Message))
                throw new HubException(result.Message);
            var user =await _userRepository.GetUser(userId);
            if (user is null)
                throw new HubException("user no found");
            await Clients.Group(groupId.ToString()).SendAsync("MemberRemoved", groupId, user.Name);
            var connectionIds = await _onlineUserRepository.GetAllConnectionIdOfUser(userId);

            foreach (var connectionId in connectionIds)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupId.ToString());
            }
        }
        public async Task DeleteMessage(int messageId, bool isGroup)
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id))
                throw new HubException("User is not authenticated.");
            IMessageRepository messageRepository;
            if (isGroup)
            {
                messageRepository = _messageRepositoryFactory.CreateRepository("GROUP");
                var message = await messageRepository.Get(id, messageId);
                if (message is null)
                    throw new HubException("Message not found.");
                var result = await messageRepository.Delete(id, messageId);
                if (string.IsNullOrEmpty(result.Message))
                {
                    await Clients.Group(message.ChatId.ToString()).SendAsync("MessageDeleted", messageId);
                }
                else
                {
                    throw new HubException(result.Message ?? "Failed to delete message");
                }
            }
            else
            {
                messageRepository = _messageRepositoryFactory.CreateRepository("TWO");
                var message = await messageRepository.Get(id, messageId);
                if (message is null)
                    throw new HubException("Message not found.");
                var result = await messageRepository.Delete(id, messageId);
                if (string.IsNullOrEmpty(result.Message))
                {
                    var twosomeChat = await _twosomeChatRepository.GetById(id, message.ChatId??0);

                    if (twosomeChat is null)
                    {
                        throw new HubException("Chat not found.");
                    }
                    var reseaverId = twosomeChat.User2.UserId;
                    var connectionIdList = await _onlineUserRepository.GetAllConnectionIdOfUser(reseaverId);
                    connectionIdList = connectionIdList.Concat(await _onlineUserRepository.GetAllConnectionIdOfUser(id)).ToList();
                    await Clients.Clients(connectionIdList).SendAsync("MessageDeleted", messageId);
                }
                else
                {
                    throw new HubException(result.Message ?? "Failed to delete message");

                }
            }
        }

    }
}
