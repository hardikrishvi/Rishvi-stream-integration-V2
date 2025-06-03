using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;

public class SyncSettingsConfigurations : Rishvi.Core.Data.IEntityTypeConfiguration<SyncSettings>
{
    public void Map(EntityTypeBuilder<SyncSettings> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        
        builder.Property(x => x.SyncEbayOrder);
        builder.Property(x => x.SyncLinnworksOrder);
        builder.Property(x => x.CreateEbayOrderToStream);
        builder.Property(x => x.CreateLinnworksOrderToStream);
        builder.Property(x => x.DispatchLinnworksOrderFromStream);
        builder.Property(x => x.DispatchEbayOrderFromStream);
        

        // Timestamp fields
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    } 
}
