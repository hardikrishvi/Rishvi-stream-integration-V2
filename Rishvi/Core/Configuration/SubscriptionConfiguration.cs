using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class SubscriptionConfiguration : Data.IEntityTypeConfiguration<Subscription>
    {
        public void Map(EntityTypeBuilder<Subscription> builder)
        {
            builder.Property(x => x.id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.party_id).IsRequired().HasMaxLength(50);
            builder.Property(x => x.@event).IsRequired().HasMaxLength(100);
            builder.Property(x => x.event_type).IsRequired().HasMaxLength(50);
            builder.Property(x => x.url_path).IsRequired().HasMaxLength(200);
            builder.Property(x => x.http_method).IsRequired().HasMaxLength(10);
        }
    }
}
