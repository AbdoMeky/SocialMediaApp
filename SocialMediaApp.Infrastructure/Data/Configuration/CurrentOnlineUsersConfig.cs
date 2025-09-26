using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    internal class CurrentOnlineUsersConfig : IEntityTypeConfiguration<CurrentOnlineUsers>
    {
        public void Configure(EntityTypeBuilder<CurrentOnlineUsers> builder)
        {
            builder.HasOne(x => x.User).WithMany(x => x.CurrentConnectionId).HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
