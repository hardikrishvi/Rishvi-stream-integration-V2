using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class FulfillmentConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<Fulfillment>
{
    public void Map(EntityTypeBuilder<Fulfillment> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        // Field Constraints
        builder.Property(x => x.FulfillmentState)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(x => x.PurchaseOrderState)
            .HasMaxLength(100)
            .IsRequired(false);

        // Timestamp Fields
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);
    }
}
