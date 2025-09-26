namespace SocialMediaApp.Core.Entities
{
    public class GroupChat : Chat
    {
        public string GroupName { get; set; }
        public string? GroupPicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<GroupChatMember>? Members { get; set; } = new List<GroupChatMember>();
        public ICollection<GroupChatMessage>? Messages { get; set; } = new List<GroupChatMessage>();

    }
}
