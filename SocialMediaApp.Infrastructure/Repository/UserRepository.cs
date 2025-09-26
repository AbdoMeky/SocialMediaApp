using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.DTO.AuthDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.DTO.User;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Utilities;
using SocialMediaApp.Infrastructure.Data;
using SocialMediaApp.Infrastructure.ExtentionMethods;
using System.Text.RegularExpressions;

namespace SocialMediaApp.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadedImagesForUserImage");

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<StringResult> Add(RegisterDTO user)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var filePath = AddImageHelper.chickImagePath(user.Image, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }
            var newUser = new ApplicationUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = filePath.Id,
                UserName = user.UserName,
                EmailConfirmed = false,
            };
            _context.Users.Add(newUser);
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(filePath.Id))
                    {
                        await AddImageHelper.AddFile(user.Image, filePath.Id);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    return new StringResult() { Message = ex.Message };
                }
            }
            return new StringResult() { Id = newUser.Id };
        }
        public async Task<StringResult> ChangeName(string Id, string FirstName, string LastName)
        {
            var oldUser = _context.Users.Find(Id);
            if (oldUser is null)
            {
                return new StringResult { Message = "No user has this Id" };
            }
            oldUser.FirstName = FirstName;
            oldUser.LastName = LastName;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StringResult() { Message = ex.Message };
            }
            return new StringResult { Id = Id };
        }
        public async Task<StringResult> ChangeProfilePictureUrl(UpdateProfilePictureUrlDTO image, string id)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var user = _context.Users.Find(id);

            if (user is null)
            {
                return new StringResult { Message = "No user has this Id" };
            }
            var filePath = AddImageHelper.chickImagePath(image.Image, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }
            var oldImagePath = user.ProfilePictureUrl;
            user.ProfilePictureUrl = filePath.Id;
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    AddImageHelper.DeleteFiles(oldImagePath);
                    if (!string.IsNullOrEmpty(filePath.Id))
                    {
                        await AddImageHelper.AddFile(image.Image, filePath.Id);
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    return new StringResult() { Message = ex.Message };
                }
            }
            return new StringResult { Id = user.Id };
        }
        /*public async Task<StringResult> AddImage(IFormFile image, string id)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            var filePath = AddImageHelper.chickImagePath(image, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }
            var user = _context.Users.Find(id);
            user.ProfilePictureUrl = filePath.Id;
            try
            {

                await _context.SaveChangesAsync();
                if (!string.IsNullOrEmpty(filePath.Id))
                {
                    using (var stream = new FileStream(filePath.Id, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                return new StringResult() { Message = ex.Message };
            }
            return new StringResult() { Id = filePath.Id };
        }*/
        public async Task<ApplicationUser> GetWithRefreshToken(string id)
        {
            return await _context.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        }
        /*public void DeleteImage(string oldImagePath)
        {
            if (File.Exists(oldImagePath))
            {
                File.Delete(oldImagePath);
            }
        }*/

        public async Task<ShowUserDTO> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return null;
            }
            return user.showUserDTO();
        }
        public async Task<List<ShowUserDTO>> Search(string searchKey)
        {
            //string usernamePattern = @"^[A-Za-z][A-Za-z0-9]*$";
            string numberPattern = @"^[0-9]+$";
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var result = new List<ShowUserDTO>();
            /*if (Regex.IsMatch(searchKey, usernamePattern))
            {
                //result = await _context.Users.Select(x => new { user = x, score = Fuzz.Ratio(x.UserName.ToLower(), searchKey.ToLower()) }).Where(x => x.score >= 70).OrderByDescending(x => x.score).Select(x => x.user.showUserDTO()).ToListAsync();
                var firstResult = await _context.Users.Select(x => x.showUserDTO()).ToListAsync();
                result = firstResult.Select(x => new { user = x, score = Fuzz.Ratio(x.UserName.ToLower(), searchKey.ToLower()) }).Where(x => x.score >= 70).OrderByDescending(x => x.score).Select(x => x.user).ToList();

            }
            else*/ if (Regex.IsMatch(searchKey, numberPattern))
            {
                result = await _context.Users.Where(x => x.PhoneNumber.Equals(searchKey)).Select(x => x.showUserDTO()).ToListAsync();
            }
            else if (Regex.IsMatch(searchKey, emailPattern))
            {
                result = await _context.Users.Where(x => x.Email.Equals(searchKey)).Select(x => x.showUserDTO()).ToListAsync();
            }
            else
            {
                var firstResult = await _context.Users.Select(x => x.showUserDTO()).ToListAsync();
                result = firstResult.Select(x => new { user = x, score = Fuzz.Ratio(x.UserName.ToLower(), searchKey.ToLower()) }).Where(x => x.score >= 50).OrderByDescending(x => x.score).Select(x => x.user).ToList();
            }
            return result;
        }
        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        /*public List<SHowExternalChatForUSerDTO> GetExternalChatsForUser(string id)
        {
            var chats = _context.Chats
                .Where(x => x.Members.Any(m => m.UserId == id))
                .Select(x => x is GroupChat ?
                     new SHowExternalChatForUSerDTO
                     {
                         Id = x.Id,
                         Name = ((GroupChat)x).GroupName,
                         ChatPicture = ((GroupChat)x).GroupPicture
                     }
                    : new SHowExternalChatForUSerDTO
                    {
                        Id = x.Id,
                        Name = x.Members.Where(m => m.UserId != id).Select(x => x.User.Name).FirstOrDefault(),
                        ChatPicture = x.Members.Where(m => m.UserId != id).Select(x => x.User.ProfilePictureUrl).FirstOrDefault()
                    })
                .ToList();

            return chats;
        }*/
        /*public List<int> ChatsIdForUser(string id)
        {
            return _context.Chats.Where(x => x.Members.Any(m => m.UserId == id)).Select(x => x.Id).ToList();
        }*/
        /*public string UserName(string id)
        {
            return _context.Users.Find(id).Name;
        }*/
    }
}

