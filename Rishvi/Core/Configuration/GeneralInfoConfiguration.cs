using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;

public class GeneralInfoConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<GeneralInfo>
{
    public void Map(EntityTypeBuilder<GeneralInfo> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        // Basic Properties
        builder.Property(x => x.Status);
        builder.Property(x => x.LabelPrinted);
        builder.Property(x => x.LabelError).HasMaxLength(255);
        builder.Property(x => x.InvoicePrinted);
        builder.Property(x => x.PickListPrinted);
        builder.Property(x => x.IsRuleRun);
        builder.Property(x => x.Notes);
        builder.Property(x => x.PartShipped);
        builder.Property(x => x.Marker);
        builder.Property(x => x.IsParked);
        builder.Property(x => x.ReferenceNum).HasMaxLength(100);
        builder.Property(x => x.SecondaryReference).HasMaxLength(100);
        builder.Property(x => x.ExternalReferenceNum).HasMaxLength(100);
        builder.Property(x => x.ReceivedDate);
        builder.Property(x => x.Source).HasMaxLength(100);
        builder.Property(x => x.SubSource).HasMaxLength(100);
        builder.Property(x => x.SiteCode).HasMaxLength(100);
        builder.Property(x => x.HoldOrCancel);
        builder.Property(x => x.DespatchByDate);
        builder.Property(x => x.HasScheduledDelivery);
        builder.Property(x => x.Location);
        builder.Property(x => x.NumItems);

        // Timestamps
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Ignore unsupported object types
        builder.Ignore(x => x.Identifiers);
        builder.Ignore(x => x.ScheduledDelivery);
        builder.Ignore(x => x.PickwaveIds);
        builder.Ignore(x => x.StockAllocationType);
    }
}
