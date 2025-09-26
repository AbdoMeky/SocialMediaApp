using Microsoft.AspNetCore.Identity;

namespace SocialMediaApp.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Navigation Properties
        public ICollection<FriendShip> FriendShipsAddedByMe { get; set; }=new List<FriendShip>();
        public ICollection<FriendShip> FriendShipsAddedMe { get; set; } = new List<FriendShip>();
        public ICollection<FriendShipRequest> FriendShipRequestsISend { get; set; } = new List<FriendShipRequest>();
        public ICollection<FriendShipRequest> FriendShipRequestsSendedToMe { get; set; } = new List<FriendShipRequest>();
        public ICollection<ChatMember> ChatMemberships { get; set; } = new List<ChatMember>();
        public ICollection<RefreshToken>? RefreshTokens { get; set; }= new List<RefreshToken>();
        public ICollection<CurrentOnlineUsers>? CurrentConnectionId { get; set; } = new List<CurrentOnlineUsers>();
    }
}
