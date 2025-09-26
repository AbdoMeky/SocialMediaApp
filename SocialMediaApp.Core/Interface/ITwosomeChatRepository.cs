using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.DTO.TwosomeChatDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface ITwosomeChatRepository
    {
        Task<IntResult> Add(string userId, string userTwoId);
        Task<GetTwosomeChatDTO> GetById(string userId,int id);
        Task<GetTwosomeChatWithMessagesDTO> Get(string userOneId, string userTwoId);
        Task<GetTwosomeChatWithMessagesDTO> GetWithMessages(string userId, int id);
    }
}
