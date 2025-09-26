namespace SocialMediaApp.Core.Entities
{
    public class FriendShipRequest
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string? ReseaverId { get; set; }
        public ApplicationUser? Reseaver { get; set; }
    }
}
