namespace SocialMediaApp.Core.Entities
{
    public class ChatMember
    {
        public int Id { get; set; }
        public DateTime AddedTime { get; set; } = DateTime.UtcNow;
        //public List<Message> Messages { get; set; }=new List<Message>();
        public List<MessageStatus> Status { get; set; }=new List<MessageStatus>();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
