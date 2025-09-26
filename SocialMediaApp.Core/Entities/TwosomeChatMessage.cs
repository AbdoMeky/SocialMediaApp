using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Entities
{
    public class TwosomeChatMessage:Message
    {
        public int? ChatId { get; set; }
        public TwosomeChat? Chat { get; set; }
        public int? MemberId { get; set; }
        public TwoSomeChatMember? Member { get; set; }
    }
}
