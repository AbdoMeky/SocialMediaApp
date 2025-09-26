using SocialMediaApp.Core.Enums;

namespace SocialMediaApp.Core.DTO.MessageDTO
{
    public class ShowMessageInChatDTO
    {
        public int Id { get; set; }
        public string? SenderUserName { get; set; }
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        public MessageStatusEnum Status { get; set; }
        public DateTime TimeSended { get; set; }
    }
}
