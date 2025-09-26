using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Core.DTO.User
{
    public class ChangeNameDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}
