using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.DTO.GroupChatDTO;
using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Enums;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Utilities;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class GroupChatRepository : IGroupChatRepository
    {
        private readonly AppDbContext _context;
        private readonly IGroupChatMemberRepository _groupChatMemberRepository;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedImagesForGroupImage");
        private readonly string _backupDirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BackupForGroupImage");
        public GroupChatRepository(AppDbContext context, IGroupChatMemberRepository chatMemberRepository)
        {
            _context = context;
            _groupChatMemberRepository = chatMemberRepository;
        }
        public async Task<IntResult> Add(string userId, AddGroupChatDTO group)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            if (!Directory.Exists(_backupDirPath))
            {
                Directory.CreateDirectory(_backupDirPath);
            }
            var filePath = AddImageHelper.chickImagePath(group.Image, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new IntResult() { Message = filePath.Message };
            }
            var newGroup = new GroupChat { GroupName = group.GroupName, GroupPicture = filePath.Id };
            _context.GroupChats.Add(newGroup);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                var result = await _groupChatMemberRepository.AddFirstMember(new AddGroupChatMemberDTO { ChatId = newGroup.Id, UserId = userId});
                if (result.Id == 0)
                {
                    return result;
                }
                await AddImageHelper.AddFile(group.Image, filePath.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                AddImageHelper.DeleteFiles(filePath.Id);
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = newGroup.Id };
        }
        public async Task<IntResult> UpdateName(string userId, int groupId, string groupName)
        {
            var newGroup = await _context.GroupChats.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == groupId);
            var currentMember = newGroup?.Members?.FirstOrDefault(x => x.UserId == userId);
            if (newGroup is null || currentMember is null)
            {
                return new IntResult { Message = "id is not valid." };
            }
            if (!currentMember.IsAdmin)
            {
                return new IntResult { Message = "you are not allow to change group name." };
            }
            newGroup.GroupName = groupName;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = 1 };
        }
        public async Task<IntResult> UpdatePicture(string userId, IFormFile groupPicture, int groupId)
        {
            if (!Directory.Exists(_backupDirPath))
            {
                Directory.CreateDirectory(_backupDirPath);
            }
            var newGroup = await _context.GroupChats.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == groupId);
            var currentMember = newGroup?.Members?.FirstOrDefault(x => x.UserId == userId);
            if (newGroup is null || currentMember is null)
            {
                return new IntResult { Message = "id is not valid." };
            }
            if (!currentMember.IsAdmin)
            {
                return new IntResult { Message = "you are not allowed to change group picture." };
            }

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);

            if (!Directory.Exists(_backupDirPath))
                Directory.CreateDirectory(_backupDirPath);

            var filePath = AddImageHelper.chickImagePath(groupPicture, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
                return new IntResult() { Message = filePath.Message };

            string newFilePath = filePath.Id;
            string oldFilePath = newGroup.GroupPicture;
            string backupPath = "";

            try
            {
                newGroup.GroupPicture = newFilePath;

                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    backupPath = await AddImageHelper.BackupFiles(oldFilePath, _backupDirPath);
                }

                if (!string.IsNullOrEmpty(newFilePath))
                {
                    await AddImageHelper.AddFile(groupPicture, newFilePath);
                }

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(backupPath))
                {
                    AddImageHelper.DeleteFiles(backupPath);
                }

                return new IntResult { Id = 1 };
            }
            catch (Exception ex)
            {
                try
                {
                    if (!string.IsNullOrEmpty(backupPath))
                    {
                        await AddImageHelper.RestoreFile(backupPath, oldFilePath);
                        AddImageHelper.DeleteFiles(backupPath);
                    }
                }
                catch (Exception restoreEx)
                {
                    return new IntResult { Message = $"Error occurred: {ex.Message}. Also failed to restore backup: {restoreEx.Message}" };
                }

                AddImageHelper.DeleteFiles(newFilePath);

                return new IntResult { Message = ex.Message };
            }
        }

        public async Task<IntResult> Delete(string userId, int id)
        {
            if (!Directory.Exists(_backupDirPath))
            {
                Directory.CreateDirectory(_backupDirPath);
            }
            var group = await _context.GroupChats.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id);
            var currentMember = group?.Members?.FirstOrDefault(x => x.UserId == userId);
            if (group is null || currentMember is null)
            {
                return new IntResult { Message = "id is not valid." };
            }
            if (!currentMember.IsAdmin)
            {
                return new IntResult { Message = "you are not allow to Delete group." };
            }
            if (group.Members.Any(x => x.IsAdmin && x.UserId != userId))
            {
                return new IntResult { Message = "You're not allowed to delete the group because you are not the only admin." };
            }
            string oldFilePath = group.GroupPicture;
            string backupPath = "";
            _context.GroupChats.Remove(group);
            try
            {
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    backupPath = await AddImageHelper.BackupFiles(oldFilePath, _backupDirPath);
                }
                AddImageHelper.DeleteFiles(oldFilePath);
                _context.ChatMembers.RemoveRange(group.Members);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(backupPath))
                {
                    await AddImageHelper.RestoreFile(backupPath, group.GroupPicture);
                }
                return new IntResult { Message = ex.Message };
            }
            AddImageHelper.DeleteFiles(backupPath);
            return new IntResult { Id = 1 };
        }
        public async Task<ShowGroupChatDTO> Get(string userId, int id)
        {
            if (!await _context.GroupChatMembers.AnyAsync(x => x.UserId == userId && x.GroupChatId == id && !x.IsOut))
            {
                return null;
            }
            var result = await _context.GroupChats.Where(x => x.Id == id).Select(x => new ShowGroupChatDTO
            {
                Id = x.Id,
                GroupPicture = x.GroupPicture,
                Name = x.GroupName,
                Members = x.Members.Where(x => !x.IsOut).Select(x => new ShowChatMemberDTO
                {
                    Id = x.Id,
                    IsAdmin = x.IsAdmin,
                    Name = x.User.FirstName + " " + x.User.LastName,
                    ProfilePictureUrl = x.User.ProfilePictureUrl ?? ""
                }).OrderByDescending(x => x.IsAdmin).ToList()
            }).FirstOrDefaultAsync();
            return result;
        }
        public async Task<ShowGroupChatWithMessagesDTO> GetWithMessages(string userId, int id)
        {
            if (!await _context.GroupChatMembers.AnyAsync(x => x.UserId == userId && x.GroupChatId == id && !x.IsOut))
            {
                return null;
            }
            var result = await _context.GroupChats.Where(x => x.Id == id).Select(x => new ShowGroupChatWithMessagesDTO
            {
                Id = x.Id,
                Name = x.GroupName,
                GroupPicture = x.GroupPicture,
                Members = x.Members.Where(x => !x.IsOut).Select(x => new ShowChatMemberDTO
                {
                    Id = x.Id,
                    IsAdmin = x.IsAdmin,
                    Name = x.User.FirstName + " " + x.User.LastName,
                    ProfilePictureUrl = x.User.ProfilePictureUrl ?? ""
                }).OrderByDescending(x => x.IsAdmin).ToList(),
                Messages = x.Messages.OrderBy(x => x.TimeSended)
                .Select(x => new ShowMessageInChatDTO
                {
                    Id = x.Id,
                    Content = x.Content,
                    TimeSended = x.TimeSended,
                    SenderUserName = x.Member != null ? x.Member.User.FirstName + " " + x.Member.User.LastName : "Deleted User",
                    Status = x.MessageStatus.Any(s => s.Status == MessageStatusEnum.send) ? MessageStatusEnum.send :
                             x.MessageStatus.Any(s => s.Status == MessageStatusEnum.reseave) ? MessageStatusEnum.reseave :
                             MessageStatusEnum.seen,
                    MediaUrl = x.Url ?? ""
                }).ToList()
            }).FirstOrDefaultAsync();
            return result;
        }
    }
}
