using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class PostalServicesConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<PostalServices>
    {
        public void Map(EntityTypeBuilder<PostalServices> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Auto-incrementing identity (optional if DB handles this)
            builder.Property(x => x.Id)
                   .ValueGeneratedOnAdd();

            // Basic field constraints
            builder.Property(x => x.AuthorizationToken)
                   .HasMaxLength(255);

            builder.Property(x => x.PostalServiceId)
                   .HasMaxLength(100);

            builder.Property(x => x.PostalServiceName)
                   .HasMaxLength(255);

            // Timestamps
            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);
        }
    }
}
