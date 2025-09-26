using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class CurrentOnlineUserRepository : ICurrentOnlineUserRepository
    {
        private readonly AppDbContext _context;
        public CurrentOnlineUserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IntResult> Add(string connectionId, string userId)
        {
            var currentUser = new CurrentOnlineUsers { ConnectionId = connectionId, UserID = userId };
            await _context.CurrentOnlineUsers.AddAsync(currentUser);
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
        public async Task<IntResult> Delete(string connectionId, string userId)
        {
            var currentUser =await _context.CurrentOnlineUsers.FirstOrDefaultAsync(x => x.ConnectionId == connectionId && x.UserID == userId);
            if (currentUser is null)
            {
                return new IntResult { Message = "No User Matching this" };
            }
            _context.CurrentOnlineUsers.Remove(currentUser);
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

        public async Task<List<string>> GetAllConnectionIdOfUser(string userId)
        {
            var ids = new List<string>();
            ids=await _context.CurrentOnlineUsers.Where(x=>x.UserID == userId).Select(x=>x.ConnectionId).ToListAsync();
            return ids;
        }

        public async Task<bool> IsOnline(string userId)
        {
            return await _context.CurrentOnlineUsers.AnyAsync(x=>x.UserID==userId);      
        }
    }
}
