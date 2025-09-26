namespace SocialMediaApp.Core.Entities
{
    public class TwosomeChat : Chat
    {
        public List<TwoSomeChatMember> Members { get; set; }= new List<TwoSomeChatMember>();
        public ICollection<TwosomeChatMessage>? Messages { get; set; } = new List<TwosomeChatMessage>();

    }
}
