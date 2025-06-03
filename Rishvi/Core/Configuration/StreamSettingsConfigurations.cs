using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;
 
public class StreamSettingsConfigurations : Rishvi.Core.Data.IEntityTypeConfiguration<StreamSettings>
{
    public void Map(EntityTypeBuilder<StreamSettings> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        
        builder.Property(x => x.GetTrackingDetails);
        builder.Property(x => x.EnableWebhook);
        builder.Property(x => x.SendChangeFromLinnworksToStream);
        builder.Property(x => x.SendChangesFromEbayToStream);
        builder.Property(x => x.CreateProductToStream);
        builder.Property(x => x.DownloadProductFromStreamToLinnworks);
        builder.Property(x => x.GetRoutePlanFromStream);
        builder.Property(x => x.GetDepotListFromStream);
        

        // Timestamp fields
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    } 
}
