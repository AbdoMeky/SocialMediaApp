using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class GroupChatMemberConfig : IEntityTypeConfiguration<GroupChatMember>
    {
        public void Configure(EntityTypeBuilder<GroupChatMember> builder)
        {
            builder.HasOne(x => x.GroupChat).WithMany(x => x.Members).HasForeignKey(x => x.GroupChatId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
