using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ContactDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class FriendShipRepository : IFriendShipRepository
    {
        private readonly AppDbContext _context;
        public FriendShipRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public async Task<IntResult> Add(string userId, string friendId)
        {
            var user1 = await _context.Users.FindAsync(userId);
            var user2 = await _context.Users.FindAsync(friendId);
            if (user1 is null || user2 is null)
            {
                return new IntResult { Message = "Id is not true" };
            }
            if(!user1.EmailConfirmed)
            {
                return new IntResult { Message = "you should verify your email to could send friendship request." };
            }
            if (!user2.EmailConfirmed)
            {
                return new IntResult { Message = "this user did not verify his email so you can not friendship request to him." };
            }
            var frienShip = await _context.FriendShips.FirstOrDefaultAsync(x => (x.UserId == friendId && x.FriendId == userId) || (x.FriendId == friendId && x.UserId == userId));
            if (frienShip is not null)
            {
                return new IntResult { Message = "You are already have friendship with this user" };
            }
            var request = await _context.FriendShipRequests.FirstOrDefaultAsync(x => x.UserId == friendId && x.ReseaverId == userId);
            if (request is null)
            {
                return new IntResult { Message = "No request come from " + user2.FirstName + " " + user2.LastName + " to make friendship with him" };
            }
            var newFriendShip = new FriendShip
            {
                UserId = userId,
                FriendId = friendId
            };
            await _context.FriendShips.AddAsync(newFriendShip);
            _context.FriendShipRequests.Remove(request);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = newFriendShip.Id };
        }
        public async Task<IntResult> Add(string userId, int id)
        {
            var user1 = await _context.Users.FindAsync(userId);
            if (user1 is null)
            {
                return new IntResult { Message = "Id is not true" };
            }
            if (!user1.EmailConfirmed)
            {
                return new IntResult { Message = "you should verify your email to could send friendship request." };
            }
            var request = await _context.FriendShipRequests.FindAsync(id);
            if (request is null||request.ReseaverId!=userId)
            {
                return new IntResult { Message = "No request send to you with this Id "};
            }
            var newFriendShip = new FriendShip
            {
                UserId = userId,
                FriendId = request.UserId
            };
            await _context.FriendShips.AddAsync(newFriendShip);
            _context.FriendShipRequests.Remove(request);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = newFriendShip.Id };
        }
        public async Task<IntResult> Delete(int id, string userId)
        {
            var friendShip = await _context.FriendShips.FindAsync(id);
            if (friendShip is null || (friendShip.UserId != userId && friendShip.FriendId != userId))
            {
                return new IntResult { Message = "No friendShip has this Id" };
            }
            _context.FriendShips.Remove(friendShip);
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
        public async Task<IntResult> RemoveFriendShipFromUserPage(string userId, string friendId)
        {
            var user = await _context.Users.FindAsync(userId);
            var friend = await _context.Users.FindAsync(friendId);
            if (friend is null || user is null)
            {
                return new IntResult { Message = "no user has this Id" };
            }
            var friendShip = await _context.FriendShips.FirstOrDefaultAsync(x => (x.UserId == friendId && x.FriendId == userId) || (x.FriendId == friendId && x.UserId == userId));
            if (friendShip is null)
            {
                return new IntResult { Message = "you already have no friendship with " + friend.Email };
            }
            _context.FriendShips.Remove(friendShip);
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
        public async Task<ShowFriendShipOfUserDTO> GetAllFriendShipsForUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return null;
            }
            var friendShips = await _context.FriendShips.Where(x => x.UserId == id || x.FriendId == id)
                .Select(x => new ShowFriendShipsInListDTO
                {
                    Id = x.Id,
                    FriendName = (x.FriendId != id) ? x.Friend.FirstName + " " + x.Friend.LastName : x.User.FirstName + " " + x.User.LastName,
                    FriendEmail = (x.FriendId != id) ? x.Friend.Email : x.User.Email
                }).ToListAsync();
            var ShowFriends = new ShowFriendShipOfUserDTO
            {
                FriendShips = friendShips,
                Email = user.Email,
                UserName = user.FirstName + " " + user.LastName
            };
            return ShowFriends;
        }
        public async Task<ShowFriendShipsInListDTO> GetById(int id, string userId)
        {
            var friendShip = await _context.FriendShips.Include(x => x.User).Include(x => x.Friend).FirstOrDefaultAsync(x => x.Id == id);
            if (friendShip == null || (friendShip.FriendId != userId && friendShip.UserId != userId))
            {
                return null;
            }
            var result = new ShowFriendShipsInListDTO
            {
                Id = friendShip.Id,
                FriendName = (friendShip.FriendId != userId) ? friendShip.Friend.FirstName + " " + friendShip.Friend.LastName : friendShip.User.FirstName + " " + friendShip.User.LastName,
                FriendEmail = (friendShip.FriendId != userId) ? friendShip.Friend.Email : friendShip.User.Email
            };
            return result;
        }
    }
}
