using SocialMediaApp.Core.DTO.User;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.ExtentionMethods
{
    public static class TwoSomeChatExtentionMethods
    {
        public static ShowUserDTO showUserDTO(this ApplicationUser user)
        {
            if (user == null) return null;
            return new ShowUserDTO
            {
                Email = user.Email,
                Name = user.FirstName + " " + user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                UserName = user.UserName,
                UserId = user.Id
            };
        }
    }
}
