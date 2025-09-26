using SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO;
using SocialMediaApp.Core.Enums;

namespace SocialMediaApp.Core.DTO.MessageDTO
{
    public class ShowMessageDTO
    {
        public int Id { get; set; }
        public int? ChatId { get; set; }
        public string? SenderUserName { get; set; }
        public string Content { get; set; }
        public string MediaUrl { get; set; }
        public MessageStatusEnum Status { get; set; }
        public List<StatusOfUserInMessageDTO>? StatusOfMessageWithUser {  get; set; }
        public DateTime TimeSended { get; set; }
    }
}
