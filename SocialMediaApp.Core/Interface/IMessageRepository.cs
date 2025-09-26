using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface IMessageRepository
    {
        Task<IntResult> Add(string userId,AddMessageDTO message);
        Task<IntResult> Delete(string userId, int id);
        Task<ShowMessageDTO> Get(string userId, int id);
        Task<ShowMessageDTO> GetWithOutUserId(int id);
        Task<List<ShowMessageInChatDTO>> ShowMessageInChat(string userId, int chatId,int page);
        Task<List<string>> ListOfReseaverIds(string userId);
    }
}
