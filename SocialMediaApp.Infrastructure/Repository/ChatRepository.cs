using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.GeneralChatDTO;
using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;
        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<GeneralGetChat>> GetChatsForUser(string userId, int page, int size)
        {
            var countSql = $@"
                                 SELECT COUNT(*) as Count
                                From ChatMember
                                where UserId ='{userId}' and (IsOut =0 or Isout is null)
                            ";
            var countRow = await _context.Counts.FromSqlRaw(countSql).FirstOrDefaultAsync();
            if(countRow is null)
            {
                return null;
            }
            int count = countRow.Count;
            int skaped = (page - 1) * size;
            if (count < skaped)
            {
                return new List<GeneralGetChat>();
            }
            int pagesize = Math.Min(10, count - skaped);

            var getChatsSql = $@"
                                select 
                                  CASE ChatMember.Discriminator when 'TwoSomeChatMember' then ChatMember.TwosomeChatID else ChatMember.GroupChatId END as Id,
                                  CASE ChatMember.Discriminator when 'TwoSomeChatMember' then [User].FirstName+' ' +[User].LastName else GroupChat.GroupName END as [Name],
                                  CASE ChatMember.Discriminator when 'TwoSomeChatMember' then [User].ProfilePictureUrl else GroupChat.GroupPicture END as Picture,
                                  CASE ChatMember.Discriminator when 'TwoSomeChatMember' then TwosomeChat.LastMessageTime else GroupChat.LastMessageTime END as LastUpdate
                                from ChatMember 
                                  Left JOIN Chat GroupChat ON ChatMember.GroupChatId=GroupChat.Id
                                  Left Join Chat TwosomeChat ON ChatMember.TwosomeChatID=TwosomeChat.Id
                                  Left Join ChatMember chatmember2 on ChatMember.TwosomeChatID=chatmember2.TwosomeChatID and ChatMember.UserId != chatmember2.UserId
                                  LEFT JOIN [User] ON chatmember2.UserId=[User].Id 
	                                where ChatMember.UserId ='{userId}' and (ChatMember.IsOut =0 or ChatMember.Isout is null)
	                                order by LastUpdate desc
	                                OFFSET {skaped} ROWS FETCH NEXT {size} ROWS ONLY
                                ";
            var chats =await _context.GeneralGetChats.FromSqlRaw(getChatsSql).ToListAsync();
            return chats;
        }
    }
}
