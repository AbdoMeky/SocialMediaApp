using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.ContactDTO
{
    public class ShowFriendShipOfUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<ShowFriendShipsInListDTO> FriendShips {  get; set; }
    }
}
