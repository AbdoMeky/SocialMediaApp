using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;
using System.Runtime.Intrinsics.X86;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class TwosomeChatMemberRepository : ITwosomeChatMemberRepository
    {
        private readonly AppDbContext _context;
        public TwosomeChatMemberRepository(AppDbContext _context)
        {
            this._context = _context;
        }
        public async Task<IntResult> Add(int chatId, string userID)
        {
            var chat =await _context.TwosomeChats.Include(x => x.Members).ThenInclude(x=>x.User).FirstOrDefaultAsync(x => x.Id == chatId);
            if(chat == null)
            {
                return new IntResult { Message = "no chat has this id." };
            }
            if (chat.Members.Count() >= 2)
            {
                return new IntResult { Message = "wrong logic of add member in twosome chat" };
            }
            if (chat.Members.Any(x => x.UserId == userID))
            {
                return new IntResult { Message = "you added to this chat already" };
            }
            var user = await _context.Users.FindAsync(userID);
            if (user == null || !user.EmailConfirmed)
            {
                return new IntResult { Message = "you should verify your email to could send friendship request." };
            }
            var member = new TwoSomeChatMember { TwosomeChatID = chatId, UserId = userID };
            chat.Members.Add(member);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = member.Id };
        }
    }
}
