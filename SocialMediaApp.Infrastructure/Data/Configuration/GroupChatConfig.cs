using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class GroupChatConfig : IEntityTypeConfiguration<GroupChat>
    {
        public void Configure(EntityTypeBuilder<GroupChat> builder)
        {
            builder.Property(x => x.GroupName).HasMaxLength(50).IsRequired();
        }
    }
}
