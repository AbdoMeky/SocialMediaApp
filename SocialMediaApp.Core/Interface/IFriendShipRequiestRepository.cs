using SocialMediaApp.Core.DTO.ContactDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Interface
{
    public interface IFriendShipRequiestRepository
    {
        Task<IntResult> Add(string userId, string friendId);
        Task<IntResult> Delete(int id, string userId);
        Task<ShowFriendShipOfUserDTO> GetAllFriendShipRequestsUserSend(string userId);
        Task<ShowFriendShipOfUserDTO> GetAllFriendShipRequestsUserReseave(string userId);
        Task<IntResult> RemoveFriendShipRequestFromUserPage(string userId, string friendId);
        Task<ShowFriendShipsInListDTO> GetById(int id, string userId);
    }
}
