using SocialMediaApp.Core.DTO.GeneralChatDTO;
using SocialMediaApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Interface
{
    public interface IChatRepository
    {
        public Task<List<GeneralGetChat>> GetChatsForUser(string userId,int page , int size);
    }
}
