using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    internal class GroupChatMessageConfig : IEntityTypeConfiguration<GroupChatMessage>
    {
        public void Configure(EntityTypeBuilder<GroupChatMessage> builder)
        {
            builder.HasOne(x => x.Member).WithMany(x => x.Messages).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(x => x.Chat).WithMany(x => x.Messages).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
