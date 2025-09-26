using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Enums;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class MessageStatusForChatMemberRepository : IMessageStatusForChatMemberRepository
    {
        private readonly AppDbContext _context;
        public MessageStatusForChatMemberRepository(AppDbContext context)
        {
            _context = context;
        }
        /*public async Task<IntResult> Add(string userId, int messageId)
        {
            if
            var member = await _context.ChatMembers.FirstOrDefaultAsync(x => x.UserId == userId);

            var status = new MessageStatus
            {
                MemberId = memberId,
                MessageId = memberId
            };
            _context.MessageStatuses.Add(status);
            var result = SaveChanges();
            if (result.Id == 1)
            {
                result.Id = status.Id;
            }
            return result;
        }
        /*public ShowStatusofMessageToUserDTO Get(int id)
        {
            var status = _context.MessageStatuses.Where(x => x.Id == id).Select(x => new ShowStatusofMessageToUserDTO
            {
                Id = x.Id,
                MemberUsername = x.Member.User.UserName,
                MessageId = x.MessageId,
                Status=x.Status
            }).FirstOrDefault();
            return status;
        }*/
        public async Task<ShowStatusOfMessageDTO> GetStatusOfMessage(string userId,int messageId)
        {
            if (!await _context.GroupChatMessages.AnyAsync(x => x.Id == messageId && x.Member.UserId == userId)&&
                !await _context.TwosomeChatMessages.AnyAsync(x => x.Id == messageId && x.Member.UserId == userId))
            {
                return null;
            }
            var statuses =await _context.MessageStatuses.Where(x => x.MessageId == messageId).Select(x => new StatusOfUserInMessageDTO
            {
                MemberUsername = (x.Member != null) ? x.Member.User.FirstName + " " + x.Member.User.LastName : "",
                UserPicture = (x.Member.User != null) ? x.Member.User.ProfilePictureUrl??"" : "",
                MessageStatusOfUser = x.Status
            }).ToListAsync();
            var result = new ShowStatusOfMessageDTO { MessageId = messageId, StatusOfUserInMessage = statuses };
            return result;
        }
        /*public async Task<IntResult> MakeItReseaved(string userId,int messageId)
        {
            var status = await _context.MessageStatuses
                .Include(x => x.Member).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(x => x.MessageId == messageId && x.Member.User.Id == userId);
            if (status is null)
            {
                return new IntResult { Message = "Id is not valid" };
            }
            status.Status = MessageStatusEnum.reseave;
            return await SaveChanges();
        }*/
        public async Task<IntResult> MakeItSeen(string userId, int messageId)
        {
            var status = await _context.MessageStatuses
                .Include(x => x.Member).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(x => x.MessageId == messageId && x.Member.User.Id == userId);
            if (status is null)
            {
                return new IntResult { Message = "Id is not valid" };
            }
            status.Status = MessageStatusEnum.seen;
            return await SaveChanges();
        }
        public async Task<IntResult> MakeItReseavedForOnlineUser(string userId)
        {
            var statuses = await _context.MessageStatuses
        .Where(s => s.Member.UserId == userId && s.Status == MessageStatusEnum.send)
        .ToListAsync();

            if (!statuses.Any())
            {
                return new IntResult { Message = "No unread messages found" };
            }

            foreach (var status in statuses)
            {
                status.Status = MessageStatusEnum.reseave;
            }

            return await SaveChanges();

        }
        /*public async Task<IntResult> MakeItSeenForUserChat(string userId, int chatId)
        {
            var statuses = _context.GroupChatMembers.Where(x => x.UserId == userId && x.GroupChatId == chatId)
                .SelectMany(x => x.Status.Where(x=>x.Status!=MessageStatusEnum.seen));
            if (!statuses.Any())
            {
                statuses = _context.TwoSomeChatMembers.Where(x => x.UserId == userId && x.TwosomeChatID == chatId).SelectMany(x => x.Status.Where(x => x.Status != MessageStatusEnum.seen));
                if (!statuses.Any())
                {
                    return new IntResult { Message = "Id is not valid." };
                }
            }
            foreach (var status in statuses)
            {
                status.Status = MessageStatusEnum.seen;
            }
            return await SaveChanges();
        }*/
        async Task<IntResult> SaveChanges()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = 1 };
        }
    }
}
