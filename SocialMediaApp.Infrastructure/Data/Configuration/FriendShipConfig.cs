using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class FriendShipConfig : IEntityTypeConfiguration<FriendShip>
    {
        public void Configure(EntityTypeBuilder<FriendShip> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.User).WithMany(x => x.FriendShipsAddedByMe).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.Friend).WithMany(x => x.FriendShipsAddedMe).HasForeignKey(x => x.FriendId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
