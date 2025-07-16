using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration;

public class EbayConfigurations : Rishvi.Core.Data.IEntityTypeConfiguration<Ebay>
{
    public void Map(EntityTypeBuilder<Ebay> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        builder.Property(x => x.DownloadOrderFromEbay);
        builder.Property(x => x.SendOrderToStream);
        builder.Property(x => x.UpdateInformationFromEbayToStream);
        builder.Property(x => x.DispatchOrderFromEbay);
        builder.Property(x => x.UpdateTrackingDetailsFromStream);


        // Timestamp fields
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
}