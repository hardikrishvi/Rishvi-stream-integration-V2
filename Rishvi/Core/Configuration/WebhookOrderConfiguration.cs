using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class WebhookOrderConfiguration : Data.IEntityTypeConfiguration<WebhookOrder>
    {
        public void Map(EntityTypeBuilder<WebhookOrder> builder)
        {
            builder.Property(x => x.id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.sequence).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.order).IsRequired().HasMaxLength(500)
                .HasDefaultValue(string.Empty);
        }
    }
}
