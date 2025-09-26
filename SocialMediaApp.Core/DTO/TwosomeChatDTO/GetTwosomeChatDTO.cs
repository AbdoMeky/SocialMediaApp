using SocialMediaApp.Core.DTO.User;

namespace SocialMediaApp.Core.DTO.TwosomeChatDTO
{
    public class GetTwosomeChatDTO
    {
        public int Id { get; set; }
        public ShowUserDTO User1 { get; set; }
        public ShowUserDTO User2 { get; set; }
    }
}
