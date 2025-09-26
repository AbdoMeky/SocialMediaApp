namespace SocialMediaApp.Core.DTO.AuthDTO
{
    public class GoogleUserInfoDTO
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string Picture { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
