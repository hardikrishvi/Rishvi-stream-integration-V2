using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class ShippingInfoConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<ShippingInfo>
{
    public void Map(EntityTypeBuilder<ShippingInfo> builder)
    {
        // Primary key
        builder.HasKey(x => x.ShippingId);
        builder.Property(x => x.ShippingId).HasDefaultValueSql("NEWID()");

        // Properties
        builder.Property(x => x.Vendor).HasMaxLength(255);
        builder.Property(x => x.PostalServiceName).HasMaxLength(255);
        builder.Property(x => x.PackageCategory).HasMaxLength(100);
        builder.Property(x => x.PackageType).HasMaxLength(100);
        builder.Property(x => x.TrackingNumber).HasMaxLength(255);

        // Optional value setup (if needed, like float precision or default values)
        builder.Property(x => x.TotalWeight).HasColumnType("float");
        builder.Property(x => x.ItemWeight).HasColumnType("float");
        builder.Property(x => x.PostageCost).HasColumnType("float");
        builder.Property(x => x.PostageCostExTax).HasColumnType("float");

        builder.Property(x => x.ManualAdjust);

        // Timestamps
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
}
