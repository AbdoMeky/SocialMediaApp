using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class ResetPasswordDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public required string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
