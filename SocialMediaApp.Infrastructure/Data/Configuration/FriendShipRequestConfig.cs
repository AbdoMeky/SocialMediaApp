using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    internal class FriendShipRequestConfig : IEntityTypeConfiguration<FriendShipRequest>
    {
        public void Configure(EntityTypeBuilder<FriendShipRequest> builder)
        {
            builder.HasOne(x=>x.User).WithMany(x=>x.FriendShipRequestsISend).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.Reseaver).WithMany(x => x.FriendShipRequestsSendedToMe).HasForeignKey(x => x.ReseaverId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
