namespace SocialMediaApp.Core.Entities
{
    public class GroupChatMessage : Message
    {
        public int? ChatId { get; set; }
        public GroupChat? Chat { get; set; }
        public int? MemberId { get; set; }
        public GroupChatMember? Member { get; set; }
    }
}
