using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Core.DTO.GroupChatDTO
{
    public class AddGroupChatDTO
    {
        [Required]
        public string GroupName { get; set; }
        public IFormFile? Image { get; set; }
    }
}
