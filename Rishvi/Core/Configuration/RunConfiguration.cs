using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class RunConfiguration : Data.IEntityTypeConfiguration<Run>
    {
        public void Map(EntityTypeBuilder<Run> builder)
        {
            builder.Property(x => x.id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.loadId).IsRequired().HasMaxLength(50);
            builder.Property(x => x.status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.description).IsRequired().HasMaxLength(500);
        }
    }
}
