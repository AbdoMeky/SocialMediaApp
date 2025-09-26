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
    public class TwoSomeChatMessageRepository : IMessageRepository
    {
        protected readonly AppDbContext _context;
        protected readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedImagesForTwosomeChatMessageMedia");
        protected readonly string _backUpPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BuckedUpImagesForTwosomeChatMessageMedia");

        public TwoSomeChatMessageRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IntResult> Add(string userId, AddMessageDTO message)
        {
            var currentMember = await _context.TwoSomeChatMembers.Include(x => x.TwosomeChat).ThenInclude(x => x.Members).FirstOrDefaultAsync(x => x.UserId == userId && x.TwosomeChatID == message.ChatId);
            if (currentMember is null)
            {
                return new IntResult { Message = "the Id is not valid" };
            }
            var filePath = AddImageHelper.chickImagePath(message.Media, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new IntResult { Message = filePath.Message };
            }
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var newMessage = new TwosomeChatMessage
            {
                ChatId = message.ChatId,
                Content = message.Content,
                MemberId = currentMember.Id,
                Url = filePath.Id
            };
            var chat = await _context.TwosomeChats.FindAsync(message.ChatId);
            chat.LastMessageTime = newMessage.TimeSended;
            await _context.TwosomeChatMessages.AddAsync(newMessage);
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    var member = currentMember.TwosomeChat.Members.FirstOrDefault(x => x.UserId != userId);
                    if (member is null)
                    {
                        return new IntResult { Message = "something is wrong in thuis chat." };
                    }
                    var status = new MessageStatus
                    {
                        MemberId = member.Id,
                        MessageId = newMessage.Id
                    };
                    if (await _context.CurrentOnlineUsers.AnyAsync(x=>x.UserID== member.UserId))
                    {
                        status.Status = MessageStatusEnum.reseave;
                    }
                    _context.MessageStatuses.Add(status);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await AddImageHelper.AddFile(message.Media, filePath.Id);
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
            var message = await _context.TwosomeChatMessages.Include(x => x.Member).Include(x => x.MessageStatus).FirstOrDefaultAsync(x => x.Id == id);
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
            _context.TwosomeChatMessages.Remove(message);
            var backUp = "";
            if (!string.IsNullOrEmpty(message.Url))
            {
                backUp = await AddImageHelper.BackupFiles(message.Url, _backUpPath);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(backUp))
                    await AddImageHelper.RestoreFile(backUp, message.Url);
                return new IntResult { Message = ex.Message };
            }
            AddImageHelper.DeleteFiles(backUp);
            return new IntResult { Id = 1 };
        }
        public async Task<ShowMessageDTO> Get(string userId, int id)
        {
            var message = await GetWithOutUserId(id);
            if (message is null || !await _context.TwoSomeChatMembers.AnyAsync(x => x.UserId == userId && x.TwosomeChatID == message.ChatId))
            {
                return null;
            }
            return message;
        }

        public async Task<ShowMessageDTO> GetWithOutUserId(int id)
        {
            var message = _context.TwosomeChatMessages.AsNoTracking().Select(x => new ShowMessageDTO
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
                StatusOfMessageWithUser = x.MessageStatus.ToList().Select
                            (x => new StatusOfUserInMessageDTO
                            {
                                MemberUsername = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "",
                                UserPicture = x.Member != null ? x.Member.User.ProfilePictureUrl ?? "" : "",
                                MessageStatusOfUser = x.Status
                            }).ToList()
            }).FirstOrDefault(x => x.Id == id);
            return message;
        }

        public async Task<List<ShowMessageInChatDTO>> ShowMessageInChat(string userId, int chatId, int page)
        {
            if (!await _context.TwoSomeChatMembers.AsNoTracking().AnyAsync(x => x.UserId == userId && x.TwosomeChatID == chatId))
            {
                return null;
            }
            int skaped = (page - 1) * 10;
            var itemNumber = await _context.TwosomeChatMessages.Where(x => x.ChatId == chatId).CountAsync();
            if(itemNumber < skaped) {
                return new List<ShowMessageInChatDTO>();
            }
            int pagesize = Math.Min(10, itemNumber - skaped);
            List<ShowMessageInChatDTO> chatMessages = await _context.TwosomeChatMessages.AsNoTracking().Where(x => x.ChatId == chatId)
                .OrderBy(x => x.TimeSended).Skip(skaped).Take(pagesize)
                .Select(x => new ShowMessageInChatDTO
                {
                    Id = x.Id,
                    Content = x.Content,
                    TimeSended = x.TimeSended,
                    SenderUserName = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "Deleted User",
                    Status = x.MessageStatus.Any(x => x.Status == MessageStatusEnum.send) ?
                    MessageStatusEnum.send : x.MessageStatus.Any(x => x.Status == MessageStatusEnum.reseave) ?
                    MessageStatusEnum.reseave : MessageStatusEnum.seen,
                    MediaUrl = x.Url ?? ""
                }).ToListAsync();
            
            
            return chatMessages;
        }
        public async Task<List<string>> ListOfReseaverIds(string userId)
        {
            return await _context.TwoSomeChatMembers.Where(x => x.UserId == userId).Select(x => x.TwosomeChat.Members.FirstOrDefault(x=>x.UserId!=userId).UserId).ToListAsync();
        }
    }
}
