using SocialMediaApp.Core.Enums;

namespace SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO
{
    public class StatusOfUserInMessageDTO
    {
        public string MemberUsername { get; set; }
        public string UserPicture { get; set; }
        public MessageStatusEnum MessageStatusOfUser {  get; set; }
    }
}
