using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;

public class IntegrationSettingsConfigurations : Rishvi.Core.Data.IEntityTypeConfiguration<IntegrationSettings>
{
    public void Map(EntityTypeBuilder<IntegrationSettings> builder)
    {
        // Define primary key if needed (assuming Email is unique for this case)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        // Configure owned types
        builder.OwnsOne(x => x.Linnworks, linn =>
        {
            linn.Property(p => p.DownloadOrderFromStream);
            linn.Property(p => p.DownloadOrderFromEbay);
            linn.Property(p => p.PrintLabelFromStream);
            linn.Property(p => p.PrintLabelFromLinnworks);
            linn.Property(p => p.DispatchOrderFromStream);
            linn.Property(p => p.DispatchOrderFromEbay);
            linn.Property(p => p.SendChangeToEbay);
            linn.Property(p => p.SendChangeToStream);
        });

        builder.OwnsOne(x => x.Stream, stream =>
        {
            stream.Property(p => p.GetTrackingDetails);
            stream.Property(p => p.EnableWebhook);
            stream.Property(p => p.SendChangeFromLinnworksToStream);
            stream.Property(p => p.SendChangesFromEbayToStream);
            stream.Property(p => p.CreateProductToStream);
            stream.Property(p => p.DownloadProductFromStreamToLinnworks);
            stream.Property(p => p.GetRoutePlanFromStream);
            stream.Property(p => p.GetDepotListFromStream);
        });

        builder.OwnsOne(x => x.Sync, sync =>
        {
            sync.Property(p => p.SyncEbayOrder);
            sync.Property(p => p.SyncLinnworksOrder);
            sync.Property(p => p.CreateEbayOrderToStream);
            sync.Property(p => p.CreateLinnworksOrderToStream);
            sync.Property(p => p.DispatchLinnworksOrderFromStream);
            sync.Property(p => p.DispatchEbayOrderFromStream);
        });

        // Optional: Set max length or constraints if needed
        builder.Property(p => p.Name).HasMaxLength(150);
        builder.Property(p => p.Email).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Password).IsRequired();
        builder.Property(p => p.AuthorizationToken).HasMaxLength(100);
        builder.Property(p => p.LinnworksSyncToken).HasMaxLength(100);
        builder.Property(p => p.LastSyncOnDate);
        builder.Property(p => p.LastSyncOn).HasMaxLength(50);

        builder.Property(p => p.ebaypage);
        builder.Property(p => p.ebayhour);
        builder.Property(p => p.linnpage);
        builder.Property(p => p.linnhour);

        // Ignore or convert the Ebay property since it's object/null
        builder.Ignore(p => p.Ebay);
        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);
    }
}