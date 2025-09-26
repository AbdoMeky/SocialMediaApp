using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Enums;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Utilities;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository.MessageRepository
{
    public class GroupChatMessageRepository : IMessageRepository
    {
        protected readonly AppDbContext _context;
        protected readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImagesForGroupMessageMedia");
        protected readonly string _backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "BackedUpImagesForGroupMessageMedia");
        public GroupChatMessageRepository(AppDbContext context) 
        {
            _context = context;
        }
        public async Task<IntResult> Add(string userId, AddMessageDTO message)
        {
            var currentMember = await _context.GroupChatMembers.FirstOrDefaultAsync(x => x.UserId == userId && x.GroupChatId == message.ChatId);
            if (currentMember is null)
            {
                return new IntResult { Message = "the Id is not valid" };
            }
            var filePath = AddImageHelper.chickImagePath(message.Media, _storagePath);
            if (! string.IsNullOrEmpty(filePath.Message))
            {
                return new IntResult { Message = filePath.Message };
            }
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var newMessage = new GroupChatMessage
            {
                ChatId = message.ChatId,
                Content = message.Content,
                MemberId = currentMember.Id,
            };
            var chat = await _context.GroupChats.FindAsync(message.ChatId);
            chat.LastMessageTime = newMessage.TimeSended;
            await _context.GroupChatMessages.AddAsync(newMessage);
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(filePath.Id))
                    {
                        newMessage.Url = filePath.Id;
                        await AddImageHelper.AddFile(message.Media, filePath.Id);
                    }
                    var members = await _context.GroupChatMembers.Where(x => x.GroupChatId == message.ChatId && !x.IsOut &&x.UserId!=userId).ToListAsync();
                    var statuses = members
                        .Where(x => x.Id != currentMember.Id)
                        .Select(x => new MessageStatus
                        {
                            MessageId = newMessage.Id
                        }).ToList();
                    var userIds = members.Where(x=>x.UserId!=userId).Select(x => x.UserId);
                    var userIdsString = userIds.Any()? string.Join(",", userIds.Select(x => $"'{x}'")): "'-1'";
                    var sql = $@"
                                SELECT *
                                FROM CurrentOnlineUsers c
                                WHERE c.UserID IN ({userIdsString});
                                ";
                    var onlineUsers =await _context.CurrentOnlineUsers.FromSqlRaw(sql).ToListAsync();
                    for (int i = 0; i < members.Count; i++)
                    {
                        statuses[i].MemberId = members[i].Id;
                        if (onlineUsers.Any(x => x.UserID == members[i].UserId))
                        {
                            statuses[i].Status = MessageStatusEnum.reseave;
                        }
                    }
                    /*foreach(GroupChatMember member in members)
                    {
                        if (onlineUsers.Any(x => x.UserID == member.UserId))
                        {
                            statuses.Add(new MessageStatus { MemberId=member.Id,MessageId=newMessage.Id})
                        }
                    }*/
                    await _context.MessageStatuses.AddRangeAsync(statuses);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    AddImageHelper.DeleteFiles(filePath.Id);
                    return new IntResult { Message = ex.Message };
                }
            }
            return new IntResult { Id = newMessage.Id };
        }
        public async Task<IntResult> Delete(string userId, int id)
        {
            var message = await _context.GroupChatMessages.Include(x => x.Member).Include(x => x.MessageStatus).FirstOrDefaultAsync(x => x.Id == id);
            if (message is null)
            {
                return new IntResult { Message = "No message has this Id" };
            }
            if (message.Member?.UserId != userId)
            {
                return new IntResult { Message = "you are not allow to delete this message becouse you are not the owner of it." };
            }
            if (message.MessageStatus is not null)
            {
                _context.MessageStatuses.RemoveRange(message.MessageStatus);
            }
            if (!Directory.Exists(_backUpPath))
            {
                Directory.CreateDirectory(_backUpPath);
            }
            _context.GroupChatMessages.Remove(message);
            var backUp = await AddImageHelper.BackupFiles(message.Url, _backUpPath);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(backUp) && !string.IsNullOrEmpty(message.Url))
                {
                    await AddImageHelper.RestoreFile(backUp, message.Url);
                }
                return new IntResult { Message = ex.Message };
            }
            AddImageHelper.DeleteFiles(backUp);
            return new IntResult { Id = 1 };
        }
        public async Task<ShowMessageDTO> Get(string userId,int id)
        {

            var message =await GetWithOutUserId(id);
            if (await _context.GroupChatMembers.AnyAsync(x => x.UserId == userId && x.GroupChatId == message.ChatId&&!x.IsOut))
            {
                return message;
            }
            return null;
        }
        public async Task<List<ShowMessageInChatDTO>> ShowMessageInChat(string userId,int chatId, int page)
        {
            if (!await _context.GroupChatMembers.AnyAsync(x => x.UserId == userId && x.GroupChatId == chatId&&!x.IsOut))
            {
                return null;
            }
            int skaped = (page - 1) * 10;
            var itemNumber = await _context.GroupChatMessages.Where(x => x.ChatId == chatId).CountAsync();
            if (itemNumber < skaped)
            {
                return new List<ShowMessageInChatDTO>();
            }
            int pagesize = Math.Min(10, itemNumber - skaped);
            List<ShowMessageInChatDTO> chatMessages = await _context.GroupChatMessages.Where(x => x.ChatId == chatId)
                .OrderBy(x => x.TimeSended).Skip(skaped).Take(pagesize)
                .Select(x => new ShowMessageInChatDTO
                {
                    Id = x.Id,
                    Content = x.Content,
                    TimeSended = x.TimeSended,
                    SenderUserName = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "Deleted User",
                    Status = x.MessageStatus.Any(x => x.Status == MessageStatusEnum.send) ? MessageStatusEnum.send : x.MessageStatus.Any(x => x.Status == MessageStatusEnum.reseave) ? MessageStatusEnum.reseave : MessageStatusEnum.seen,
                    MediaUrl = x.Url ?? ""
                }).ToListAsync();
            return chatMessages;
        }
        public async Task<ShowMessageDTO> GetWithOutUserId(int id)
        {
            var message = await _context.GroupChatMessages.Select(x => new ShowMessageDTO
            {
                ChatId = x.ChatId,
                Content = x.Content,
                SenderUserName = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "Deleted User",
                Status = x.MessageStatus.Any(x => x.Status == MessageStatusEnum.send) ?
                        MessageStatusEnum.send : x.MessageStatus.Any(x => x.Status == MessageStatusEnum.reseave) ?
                        MessageStatusEnum.reseave : MessageStatusEnum.seen,
                Id = x.Id,
                TimeSended = x.TimeSended,
                MediaUrl = x.Url ?? "",
                StatusOfMessageWithUser = x.MessageStatus.Select
                    (x => new StatusOfUserInMessageDTO
                    {
                        MemberUsername = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "",
                        UserPicture = x.Member != null ? x.Member.User.ProfilePictureUrl ?? "" : "",
                        MessageStatusOfUser = x.Status
                    }).ToList()
            }).FirstOrDefaultAsync(x => x.Id == id);
            return message;
        }

        public async Task<List<string>> ListOfReseaverIds(string userId)
        {
            return await _context.GroupChatMembers.Where(x=>x.UserId== userId).Select(x=>x.GroupChatId.ToString()).ToListAsync();
        }
    }
}
