namespace SocialMediaApp.Core.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public DateTime LastMessageTime { get; set; } = DateTime.UtcNow;
    }

}
