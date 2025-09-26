namespace SocialMediaApp.Core.Entities
{
    public class GroupChatMember : ChatMember
    {
        public int? GroupChatId { get; set; }
        public GroupChat GroupChat { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsOut { get; set; } = false;
        public List<GroupChatMessage>? Messages { get; set; } = new List<GroupChatMessage>();
    }
}
