using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(30, ErrorMessage = "First name cannot exceed 30 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9]*$", ErrorMessage = "Username must start with a letter and contain only letters and numbers.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number must contain digits only.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Confirm phone number is required.")]
        [Compare("PhoneNumber", ErrorMessage = "Phone numbers do not match.")]
        public string ConfirmPhoneNumber { get; set; }

        public IFormFile? Image { get; set; }
    }

}
