using SocialMediaApp.Core.DTO.ChatMemberDTO;

namespace SocialMediaApp.Core.DTO.GroupChatDTO
{
    public class ShowGroupChatDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? GroupPicture {  get; set; }
        public List<ShowChatMemberDTO>? Members { get; set; }
    }
}
