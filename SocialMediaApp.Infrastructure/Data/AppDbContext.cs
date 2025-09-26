using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Entities;
namespace SocialMediaApp.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<CurrentOnlineUsers> CurrentOnlineUsers { get; set; }
        public DbSet<FriendShip> FriendShips { get; set; }
        public DbSet<FriendShipRequest> FriendShipRequests { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<GroupChatMember> GroupChatMembers { get; set; }
        public DbSet<GroupChatMessage> GroupChatMessages { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageStatus> MessageStatuses { get; set; }
        public DbSet<TwosomeChat> TwosomeChats { get; set; }
        public DbSet<TwoSomeChatMember> TwoSomeChatMembers { get; set; }
        public DbSet<TwosomeChatMessage> TwosomeChatMessages { get; set; }

        //Keyless Entity (Query Type)
        public DbSet<CountResult> Counts { get; set; }
        public DbSet<GeneralGetChat> GeneralGetChats { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
