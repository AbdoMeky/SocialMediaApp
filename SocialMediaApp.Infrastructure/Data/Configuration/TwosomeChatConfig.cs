using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMediaApp.Core.Entities;

namespace SocialMediaApp.Infrastructure.Data.Configuration
{
    internal class TwosomeChatConfig : IEntityTypeConfiguration<TwosomeChat>
    {
        public void Configure(EntityTypeBuilder<TwosomeChat> builder)
        {
        }
    }
}
