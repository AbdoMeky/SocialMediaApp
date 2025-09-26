using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class MessageStatusConfig : IEntityTypeConfiguration<MessageStatus>
    {
        public void Configure(EntityTypeBuilder<MessageStatus> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(m => m.Status).HasConversion<int>();
            builder.HasOne(x => x.Member).WithMany(x => x.Status).HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Message).WithMany(x => x.MessageStatus).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.NoAction);
            builder.ToTable("MessageStatus");
        }
    }
}
