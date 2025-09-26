using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.ContactDTO
{
    public class ShowFriendShipDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FriendName { get; set; }
        public string FriendEmail { get; set; }
    }
}
