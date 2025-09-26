using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasDiscriminator<string>("Type").HasValue<GroupChatMessage>("GROP").HasValue<TwosomeChatMessage>("TWIC");
            builder.Property(x => x.Content).HasMaxLength(2048);
            builder.ToTable("Messages");
        }
    }
}
