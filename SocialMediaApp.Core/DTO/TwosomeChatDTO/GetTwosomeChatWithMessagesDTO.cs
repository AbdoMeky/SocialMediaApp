using SocialMediaApp.Core.DTO.MessageDTO;

namespace SocialMediaApp.Core.DTO.TwosomeChatDTO
{
    public class GetTwosomeChatWithMessagesDTO
    {
        public int Id { get; set; }
        public string UserTalkedName { get; set; }
        public string? UserPicture {  get; set; }
        public string UserId { get; set; }
        public bool IsOnline { get; set; }
        public List<ShowMessageInChatDTO>? Messages { get; set; }
    }
}
