using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;

public class LinnworksSettingsConfigurations : Rishvi.Core.Data.IEntityTypeConfiguration<LinnworksSettings>
{
    public void Map(EntityTypeBuilder<LinnworksSettings> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        
        builder.Property(x => x.DownloadOrderFromStream);
        builder.Property(x => x.DownloadOrderFromEbay);
        builder.Property(x => x.PrintLabelFromStream);
        builder.Property(x => x.PrintLabelFromLinnworks);
        builder.Property(x => x.DispatchOrderFromStream);
        builder.Property(x => x.DispatchOrderFromEbay);
        builder.Property(x => x.SendChangeToEbay);
        builder.Property(x => x.SendChangeToStream);
        

        // Timestamp fields
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
    
}

