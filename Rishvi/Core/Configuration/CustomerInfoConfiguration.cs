using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class CustomerInfoConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<CustomerInfo>
{
    public void Map(EntityTypeBuilder<CustomerInfo> builder)
    {
        // Primary Key
        builder.HasKey(x => x.CustomerInfoId);
        builder.Property(x => x.CustomerInfoId).HasDefaultValueSql("NEWID()");

        // Channel buyer name constraint
        builder.Property(x => x.ChannelBuyerName).HasMaxLength(255);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Relationships with Address
        builder.HasOne(x => x.Address)
            .WithMany() // assuming Address is not linked back to CustomerInfo
            .HasForeignKey("AddressId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BillingAddress)
            .WithMany()
            .HasForeignKey("BillingAddressId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}