using SocialMediaApp.Core.Enums;

namespace SocialMediaApp.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime TimeSended { get; set; }= DateTime.UtcNow;
        public string? Url { get; set; }
        public List<MessageStatus>? MessageStatus { get; set; } = new List<MessageStatus>();

    }
}
