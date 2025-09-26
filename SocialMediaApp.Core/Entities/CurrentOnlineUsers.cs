namespace SocialMediaApp.Core.Entities
{
    public class CurrentOnlineUsers
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
    }
}
