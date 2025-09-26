using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.ChatMemberDTO
{
    public class ShowChatMemberDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
