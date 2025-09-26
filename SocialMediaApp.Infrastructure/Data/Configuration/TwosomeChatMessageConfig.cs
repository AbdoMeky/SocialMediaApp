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
    internal class TwosomeChatMessageConfig : IEntityTypeConfiguration<TwosomeChatMessage>
    {
        public void Configure(EntityTypeBuilder<TwosomeChatMessage> builder)
        {
            builder.HasOne(x => x.Member).WithMany(x => x.Messages).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.Chat).WithMany(x => x.Messages).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
