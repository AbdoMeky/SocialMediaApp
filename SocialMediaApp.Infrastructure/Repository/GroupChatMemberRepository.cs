using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class GroupChatMemberRepository : IGroupChatMemberRepository
    {
        private readonly AppDbContext _context;
        public GroupChatMemberRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IntResult> Add(string userId,AddGroupChatMemberDTO member)
        {
            if(!await _context.GroupChatMembers.AnyAsync(x=>x.UserId == userId && x.GroupChatId == member.ChatId && x.IsAdmin))
            {
                return new IntResult { Message = "you are not allow to add members to this group." };
            }
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.EmailConfirmed)
            {
                return new IntResult { Message = "you should verify your email to could send friendship request." };
            }
            if (await _context.FriendShips.FirstOrDefaultAsync(x => (x.UserId == userId && x.FriendId == member.UserId) || (x.UserId == member.UserId && x.FriendId == userId)) is null)
            {
                return new IntResult { Message = "you should have a friendship with this user to could add him to this group." };
            }
            var newMember =await _context.GroupChatMembers.FirstOrDefaultAsync(x => x.GroupChatId == member.ChatId && x.UserId == member.UserId);
            if (newMember != null)
            {
                if (!newMember.IsOut)
                {
                    return new IntResult { Message = "the user is already in the group." };
                }
                newMember.IsOut = false;
                newMember.AddedTime = DateTime.UtcNow;
            }
            else
            {
                newMember = new GroupChatMember
                {
                    GroupChatId = member.ChatId,
                    UserId = member.UserId,
                    IsAdmin = false
                };
                await _context.ChatMembers.AddAsync(newMember);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = member.ChatId };
        }
        public async Task<IntResult> AddFirstMember( AddGroupChatMemberDTO member)
        {
            if (await _context.GroupChatMembers.AnyAsync(x => x.GroupChatId == member.ChatId && x.UserId == member.UserId))
            {
                return new IntResult { Message = "User is already in the group." };
            }
            var newMember = new GroupChatMember
            {
                GroupChatId = member.ChatId,
                UserId = member.UserId,
                IsAdmin = true
            };
            await _context.ChatMembers.AddAsync(newMember);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = member.ChatId };
        }
        public async Task<IntResult> AddAdminToMember(string userId,int memberId)
        {
            var member =await _context.GroupChatMembers.FindAsync(memberId);
            if (member is null)
            {
                return new IntResult { Message = "id is not valid." };
            }
            if (member.IsAdmin == true)
            {
                return new IntResult { Message = "The user is already admin" };
            }
            var admin = await _context.GroupChatMembers.FirstOrDefaultAsync(x => x.GroupChatId == member.GroupChatId && x.UserId == userId);
            if(admin is null || !admin.IsAdmin)
            {
                return new IntResult { Message = "you should be admin to do this process." };
            }
            member.IsAdmin = true;
            return await saveChangesAsync();
        }
        public async Task<IntResult> Delete(string userId,int MemberId)
        {
            var member =await _context.GroupChatMembers.FindAsync(MemberId);
            if (member is null||member.IsOut)
            {
                return new IntResult { Message = "id is not valid." };
            }
            var admin = await _context.GroupChatMembers.FirstOrDefaultAsync(x => x.GroupChatId == member.GroupChatId && x.UserId == userId);
            if (admin is null || (admin.Id != MemberId && !admin.IsAdmin))
            {
                return new IntResult { Message = "you should be admin to do this process." };
            }
            if (member.IsAdmin && admin.Id != member.Id)
            {
                return new IntResult { Message = "he is admin too, i can not remove him." };
            }
            var chat = _context.GroupChats
                .Include(x => x.Members)
                .FirstOrDefault(x => x.Id == member.GroupChatId);
            if (chat is null)
            {
                return new IntResult { Message = "No chat found for this member" };
            }
            bool wasAdmin = member.IsAdmin;
            member.IsOut = true;
            member.IsAdmin = false;
            var remainingMembers = chat.Members.Where(x => !x.IsOut).ToList();
            if (remainingMembers.Any())
            {
                if (wasAdmin)
                {
                    if (!remainingMembers.Any(x => x.IsAdmin && x.Id != member.Id))
                    {
                        var newAdmin = remainingMembers
                            .Where(x => x.Id != member.Id)
                            .OrderBy(x => x.AddedTime)
                            .FirstOrDefault();
                        if (newAdmin is not null)
                        {
                            newAdmin.IsAdmin = true;
                        }
                    }
                }
            }
            else
            {
                _context.GroupChats.Remove(chat);
                _context.GroupChatMembers.RemoveRange(chat.Members);
            }
            return await saveChangesAsync();
        }

        public async Task<List<ShowChatMemberDTO>> GetMembersInGroupChat(string userId,int chatId)
        {
            if(!await _context.GroupChatMembers.AnyAsync(x=>x.GroupChatId == chatId&&x.UserId==userId&&!x.IsOut))
            {
                return null;
            }
            var result =await _context.GroupChatMembers.Include(x=>x.User).Where(x => x.GroupChatId == chatId&&!x.IsOut).Select(x => new ShowChatMemberDTO
            {
                Id = x.Id,
                IsAdmin = x.IsAdmin,
                Name = x.User.FirstName + " " + x.User.LastName,
                ProfilePictureUrl = x.User.ProfilePictureUrl??""
            }).OrderByDescending(x => x.IsAdmin).ToListAsync();
            return result;
        }
        public async Task<IntResult> RemoveAdminFromMember(string userId,int id)
        {
            var member =await _context.GroupChatMembers.FindAsync(id);
            if (member is null)
            {
                return new IntResult { Message = "No member has this Id" };
            }
            if (member.UserId != userId)
            {
                return new IntResult { Message = "you are not allow to do this process." };
            }
            if (member.IsAdmin == false)
            {
                return new IntResult { Message = "you already are not admin" };
            }
            member.IsAdmin = false;
            var chat =await _context.GroupChats.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == member.GroupChatId);
            var remainingMembers = chat?.Members?.Where(x => !x.IsOut).ToList();
            if (!remainingMembers.Any(x => x.IsAdmin&&x.UserId!=userId))
            {
                var newAdmin = chat.Members.Where(x=>x.Id!=id).MinBy(x => x.AddedTime);
                if(newAdmin is null)
                {
                    newAdmin = member;
                }
                newAdmin.IsAdmin = true;
            }
            return await saveChangesAsync();
        }
        async Task<IntResult> saveChangesAsync()
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
