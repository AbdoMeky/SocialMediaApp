namespace SocialMediaApp.Core.Entities
{
    public class TwoSomeChatMember : ChatMember
    {
        public int? TwosomeChatID { get; set; }
        public TwosomeChat? TwosomeChat { get; set; }
        public List<TwosomeChatMessage> Messages { get; set; } = new List<TwosomeChatMessage>();

    }
}
