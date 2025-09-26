using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.DTO.AuthDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.DTO.User;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Core.Interface
{
    public interface IUserRepository
    {
        Task<StringResult> Add(RegisterDTO User);
        Task<StringResult> ChangeName(string Id, string FirstName, string LastName);
        Task<ApplicationUser> GetWithRefreshToken(string id);
        Task<StringResult> ChangeProfilePictureUrl(UpdateProfilePictureUrlDTO image, string id);
        Task<ShowUserDTO> GetUser(string id);
        Task<List<ShowUserDTO>> Search(string searchKey);
        Task SaveAsync();
        //List<ShowExternalChatForUSerDTO> GetExternalChatsForUser(string id);
        //List<int> ChatsIdForUser(string id);
        //string UserName(string id);
    }
}
