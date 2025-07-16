using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;


public class StreamOrderRecordConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<StreamOrderRecord>
{
    public void Map(EntityTypeBuilder<StreamOrderRecord> builder)
    {

        // Primary key
        builder.HasKey(x => x.Id);

        // Auto-generate GUID for Id
        builder.Property(x => x.Id)
            .HasDefaultValueSql("NEWID()");

        // JSON payload (can be large)
        builder.Property(x => x.JsonData)
            .HasMaxLength(5000)
            .IsRequired();

        // Standard string fields
        builder.Property(x => x.AuthorizationToken)
            .HasMaxLength(255);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.EbayOrderId)
            .HasMaxLength(255);

        builder.Property(x => x.LinnworksOrderId)
            .HasMaxLength(100);

        builder.Property(x => x.ConsignmentId)
            .HasMaxLength(100);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(50);

        builder.Property(x => x.TrackingUrl)
            .HasMaxLength(255); // increased for realistic URL length

        builder.Property(x => x.Order)
            .HasMaxLength(100);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);
    }
}