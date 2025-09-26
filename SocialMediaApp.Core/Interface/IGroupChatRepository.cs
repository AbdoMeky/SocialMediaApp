using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.DTO.GroupChatDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface IGroupChatRepository
    {
        Task<IntResult> Add(string userId,AddGroupChatDTO group);
        Task<IntResult> Delete(string userId, int id);
        Task<IntResult> UpdateName(string userId, int groupId, string groupName);
        Task<IntResult> UpdatePicture(string userId,IFormFile groupPicture, int groupId);
        Task<ShowGroupChatDTO> Get(string userId,int id);
        Task<ShowGroupChatWithMessagesDTO> GetWithMessages(string userId,int id);
    }
}
