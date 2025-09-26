using SocialMediaApp.Core.DTO.ContactDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface IFriendShipRepository
    {
        Task<IntResult> Add(string userId, string friendId);
        Task<IntResult> Add(string userId, int Id);
        Task<IntResult> Delete(int id,string userId);
        Task<IntResult> RemoveFriendShipFromUserPage(string userId, string friendId);
        Task<ShowFriendShipsInListDTO> GetById(int id, string userId);
        Task<ShowFriendShipOfUserDTO> GetAllFriendShipsForUser(string id);
    }
}
