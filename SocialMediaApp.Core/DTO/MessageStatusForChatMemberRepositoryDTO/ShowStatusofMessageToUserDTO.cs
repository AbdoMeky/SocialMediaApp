using SocialMediaApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO
{
    public class ShowStatusofMessageToUserDTO
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string MemberUsername { get; set; }
        public MessageStatusEnum Status { get; set; }
    }
}
