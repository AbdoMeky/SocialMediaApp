using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ContactDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class FriendShipRequiestRepository : IFriendShipRequiestRepository
    {
        private readonly AppDbContext _context;
        public FriendShipRequiestRepository(AppDbContext appDbContext)
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
            if (userId == friendId)
                return new IntResult { Message = "You can't send or accept friendship with yourself." };
            if (!user1.EmailConfirmed)
            {
                return new IntResult { Message = "you should verify your email to could send friendship request." };
            }
            if (!user2.EmailConfirmed)
            {
                return new IntResult { Message = "this user did not verify his email so you can not friendship request to him." };
            }
            var frienShipRequest = await _context.FriendShipRequests.FirstOrDefaultAsync(x => x.UserId == userId && x.ReseaverId == friendId);
            if (frienShipRequest is not null)
            {
                return new IntResult { Message = "You are already send friendship request to this user and he has not accept it yet" };
            }
            frienShipRequest = await _context.FriendShipRequests.FirstOrDefaultAsync(x => x.ReseaverId == userId && x.UserId == friendId);
            if (frienShipRequest is not null)
            {
                return new IntResult { Message = "You are already have friendship request from this user and you have not accept it yet" };
            }
            var frienShip = await _context.FriendShips.FirstOrDefaultAsync(x => (x.UserId == friendId && x.FriendId == userId) || (x.FriendId == friendId && x.UserId == userId));
            if (frienShip is not null)
            {
                return new IntResult { Message = "You are already have friendship with this user." };
            }
            var newFriendShip = new FriendShipRequest
            {
                UserId = userId,
                ReseaverId = friendId
            };
            await _context.FriendShipRequests.AddAsync(newFriendShip);
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
            var friendShipRequest = await _context.FriendShipRequests.FindAsync(id);
            if (friendShipRequest is null || (friendShipRequest.UserId != userId && friendShipRequest.ReseaverId != userId))
            {
                return new IntResult { Message = "No friendShip has this Id" };
            }
            _context.FriendShipRequests.Remove(friendShipRequest);
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

        public async Task<ShowFriendShipOfUserDTO> GetAllFriendShipRequestsUserSend(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
            {
                return null;
            }
            var friendShips = await _context.FriendShipRequests.Where(x => x.UserId == userId)
                .Select(x => new ShowFriendShipsInListDTO
                {
                    Id = x.Id,
                    FriendName =x.Reseaver.FirstName + " " + x.Reseaver.LastName,
                    FriendEmail =x.Reseaver.Email
                }).ToListAsync();
            var ShowFriends = new ShowFriendShipOfUserDTO
            {
                FriendShips = friendShips,
                Email = user.Email,
                UserName = user.FirstName + " " + user.LastName
            };
            return ShowFriends;
        }
        public async Task<ShowFriendShipOfUserDTO> GetAllFriendShipRequestsUserReseave(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
            {
                return null;
            }
            var friendShips = await _context.FriendShipRequests.Where(x=>x.ReseaverId == userId)
                .Select(x => new ShowFriendShipsInListDTO
                {
                    Id = x.Id,
                    FriendName = x.User.FirstName + " " + x.User.LastName,
                    FriendEmail = x.User.Email
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
            var friendShip = await _context.FriendShipRequests.Include(x => x.User).Include(x => x.Reseaver).FirstOrDefaultAsync(x => x.Id == id);
            if (friendShip == null || (friendShip.ReseaverId != userId && friendShip.UserId != userId))
            {
                return null;
            }
            var result = new ShowFriendShipsInListDTO
            {
                Id = friendShip.Id,
                FriendName = (friendShip.ReseaverId != userId) ? friendShip.Reseaver.FirstName + " " + friendShip.Reseaver.LastName : friendShip.User.FirstName + " " + friendShip.User.LastName,
                FriendEmail = (friendShip.ReseaverId != userId) ? friendShip.Reseaver.Email : friendShip.User.Email
            };
            return result;
        }

        public async Task<IntResult> RemoveFriendShipRequestFromUserPage(string userId, string friendId)
        {
            var user = await _context.Users.FindAsync(userId);
            var friend = await _context.Users.FindAsync(friendId);
            if (friend is null || user is null)
            {
                return new IntResult { Message = "no user has this Id" };
            }
            var friendShipRequest = await _context.FriendShipRequests.FirstOrDefaultAsync(x => (x.UserId == friendId && x.ReseaverId == userId) || (x.ReseaverId == friendId && x.UserId == userId));
            if (friendShipRequest is null)
            {
                return new IntResult { Message = "you already have no friendship with " + friend.Email };
            }
            _context.FriendShipRequests.Remove(friendShipRequest);
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
