using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface ICurrentOnlineUserRepository
    {
        Task<IntResult> Add(string connectionId, string userId);
        Task<IntResult> Delete(string connectionId, string userId);
        Task<List<string>> GetAllConnectionIdOfUser(string userId);
        Task<bool> IsOnline(string userId);
    }
}
