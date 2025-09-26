using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    public class ChatConfig : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasDiscriminator<string>("Type").HasValue<GroupChat>("GROP").HasValue<TwosomeChat>("TWIC");
            builder.Property("Type").HasMaxLength(4);
            builder.ToTable("Chat");
        }
    }
}
