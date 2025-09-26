namespace SocialMediaApp.Core.DTO.User
{
    public class ShowUserDTO
    {
        public string Name { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserId {  get; set; }
        public bool IsOnline { get; set; }
    }
}
