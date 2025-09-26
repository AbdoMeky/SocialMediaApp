using SocialMediaApp.Core.DTO.ResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Interface
{
    public interface ITwosomeChatMemberRepository
    {
        Task<IntResult> Add(int chatId, string userID);
    }
}
