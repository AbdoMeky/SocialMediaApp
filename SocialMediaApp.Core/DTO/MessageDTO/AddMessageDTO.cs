using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Core.DTO.MessageDTO
{
    public class AddMessageDTO
    {
        [Required]
        public int ChatId { get; set; }
        public string? Content { get; set; }
        public IFormFile? Media { get; set; }
    }
}
