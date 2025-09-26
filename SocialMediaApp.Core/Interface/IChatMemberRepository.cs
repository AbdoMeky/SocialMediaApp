using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Interface
{
    public interface IGroupChatMemberRepository
    {
        Task<IntResult> AddFirstMember(AddGroupChatMemberDTO member);
        Task<IntResult> Add(string userId, AddGroupChatMemberDTO member);
        Task<IntResult> RemoveAdminFromMember(string userId,int id);
        Task<IntResult> AddAdminToMember(string userId,int id);
        Task<IntResult> Delete(string userId,int MemberId);
        Task<List<ShowChatMemberDTO>> GetMembersInGroupChat(string userId, int chatId);
    }
}
