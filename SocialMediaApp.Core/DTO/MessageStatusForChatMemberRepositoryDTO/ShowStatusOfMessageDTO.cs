using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO
{
    public class ShowStatusOfMessageDTO
    {
        public int MessageId {  get; set; }
        public List<StatusOfUserInMessageDTO> StatusOfUserInMessage { get; set; }
    }
}
