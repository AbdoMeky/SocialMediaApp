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
    public class TwosomeChatMemberConfig : IEntityTypeConfiguration<TwoSomeChatMember>
    {
        public void Configure(EntityTypeBuilder<TwoSomeChatMember> builder)
        {
            builder.HasOne(x => x.TwosomeChat).WithMany(x => x.Members).HasForeignKey(x => x.TwosomeChatID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
