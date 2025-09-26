using SocialMediaApp.Core.Enums;

namespace SocialMediaApp.Core.Entities
{
    public class MessageStatus
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public MessageStatusEnum Status = MessageStatusEnum.send;
        public Message Message { get; set; }
        public int MemberId { get; set; }
        public ChatMember Member { get; set; }
    }
}
