using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class EventConfiguration : Data.IEntityTypeConfiguration<Event>
    {
        public void Map(EntityTypeBuilder<Event> builder)
        {
            builder.Property(x => x.id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.event_code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.event_code_desc).IsRequired().HasMaxLength(100);
            builder.Property(x => x.event_desc).IsRequired().HasMaxLength(200);
            builder.Property(x => x.event_date).IsRequired().HasMaxLength(50);
            builder.Property(x => x.event_time).IsRequired().HasMaxLength(50);
            builder.Property(x => x.event_text).IsRequired().HasMaxLength(500);
            builder.Property(x => x.event_link).IsRequired().HasMaxLength(200);
        }
    }
}
