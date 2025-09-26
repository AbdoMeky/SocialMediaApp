using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.DTO.MessageDTO;

namespace SocialMediaApp.Core.DTO.GroupChatDTO
{
    public class ShowGroupChatWithMessagesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? GroupPicture { get; set; }
        public List<ShowChatMemberDTO>? Members { get; set; }
        public List<ShowMessageInChatDTO>? Messages { get; set; }
    }
}
