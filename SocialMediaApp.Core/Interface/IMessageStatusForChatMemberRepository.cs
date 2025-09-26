using SocialMediaApp.Core.DTO.MessageStatusForChatMemberRepositoryDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface IMessageStatusForChatMemberRepository
    {
        //Task<IntResult> Add(string userId, int messageId);
        //Task<IntResult> MakeItReseaved(string userId,int messageId);
        Task<IntResult> MakeItSeen(string userId, int messageId);
        Task<IntResult> MakeItReseavedForOnlineUser(string userId);
        //Task<IntResult> MakeItSeenForUserChat(string userId, int chatId);
        Task<ShowStatusOfMessageDTO> GetStatusOfMessage(string userId, int messageId);
        //ShowStatusofMessageToUserDTO Get(int id);
        //ShowStatusOfMessageDTO GetStatusOfMessage(int MessageId);
    }
}
