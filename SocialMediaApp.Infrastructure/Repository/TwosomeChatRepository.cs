using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.DTO.TwosomeChatDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Enums;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;
using SocialMediaApp.Infrastructure.ExtentionMethods;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class TwosomeChatRepository : ITwosomeChatRepository
    {
        private readonly AppDbContext _context;
        private readonly ITwosomeChatMemberRepository _memberRepository;
        private readonly ICurrentOnlineUserRepository _onlineUserRepository;
        public TwosomeChatRepository(AppDbContext context, ITwosomeChatMemberRepository memberRepository, ICurrentOnlineUserRepository onlineUserRepository)
        {
            _context = context;
            _memberRepository = memberRepository;
            _onlineUserRepository = onlineUserRepository;

        }
        public async Task<IntResult> Add(string userId, string userTwoId)
        {
            if (userId == userTwoId)
                return new IntResult { Message = "You can't chat with yourself." };
            var checkChat = await _context.TwosomeChats.FirstOrDefaultAsync(x =>
                    x.Members.Any(m => m.UserId == userId) &&
                    x.Members.Any(m => m.UserId == userTwoId));
            if (checkChat is not null)
            {
                return new IntResult { Message = "there is chat between you and this user." };
            }
            if (await _context.FriendShips.FirstOrDefaultAsync(x => (x.UserId == userId && x.FriendId == userTwoId) || (x.UserId == userTwoId && x.FriendId == userId)) is null)
            {
                return new IntResult { Message = "you should have a friendship with this user to could have chat with him." };
            }
            var chat = new TwosomeChat();
            _context.TwosomeChats.Add(chat);
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    var result = await _memberRepository.Add(chat.Id, userId);
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        return result;
                    }
                    result = await _memberRepository.Add(chat.Id, userTwoId);
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        return result;
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    return new IntResult { Message = ex.Message };
                }
            }
            return new IntResult { Id = chat.Id };
        }
        public async Task<GetTwosomeChatWithMessagesDTO> Get(string userId, string userTwoId)
        {
            if (userId == userTwoId)
            {
                return null;
            }
            var result = await _context.TwosomeChats.AsNoTracking().Include(x => x.Members).ThenInclude(x => x.User).FirstOrDefaultAsync(x =>
            x.Members.Any(m => m.UserId == userId) &&
            x.Members.Any(m => m.UserId == userTwoId));
            if (result is null)
            {
                var newResult = await Add(userId, userTwoId);
                if (newResult.Id == 0)
                {
                    return null;
                }
            }
            return await GetWithMessages(userId, result.Id);
        }
        public async Task<GetTwosomeChatDTO> GetById(string userId, int id)
        {
            var result =await _context.TwosomeChats.AsNoTracking().Include(x => x.Members).ThenInclude(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (result is null || !result.Members.Any(x => x.UserId == userId)||result.Members.Count()!=2)
            {
                return null;
            }
            var ReturnedResult = new GetTwosomeChatDTO
            {
                Id = result.Id,
                User1 = result.Members.FirstOrDefault(x => x.UserId == userId).User.showUserDTO(),
                User2 = result.Members.FirstOrDefault(x => x.UserId != userId).User.showUserDTO(),
                
            };
            ReturnedResult.User2.IsOnline = await _onlineUserRepository.IsOnline(ReturnedResult.User2.UserId);
            return ReturnedResult;
        }
        public async Task<GetTwosomeChatWithMessagesDTO> GetWithMessages(string userId, int id)
        {
            var result = _context.TwosomeChats.AsNoTracking().Select(x => new GetTwosomeChatWithMessagesDTO
            {
                Id = x.Id,
                UserPicture = x.Members.FirstOrDefault(x => x.UserId != userId).User.ProfilePictureUrl,
                UserId= x.Members.FirstOrDefault(x => x.UserId != userId).UserId,
                UserTalkedName = x.Members.FirstOrDefault(x => x.UserId != userId).User.FirstName + " " + x.Members.FirstOrDefault(x => x.UserId != userId).User.LastName,
                Messages = x.Messages.OrderBy(x => x.TimeSended)
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
                }).ToList()
            }).FirstOrDefault(x => x.Id == id);
            if(result is null)
            {
                return null;
            }
            result.IsOnline=await _onlineUserRepository.IsOnline(result.UserId);
            return result;
        }
    }
}
