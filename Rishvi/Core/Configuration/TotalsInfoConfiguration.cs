using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class TotalsInfoConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<TotalsInfo>
{
    public void Map(EntityTypeBuilder<TotalsInfo> builder)
    {
        // Primary Key
        builder.HasKey(x => x.TotalsInfoId);
        builder.Property(x => x.TotalsInfoId).HasDefaultValueSql("NEWID()");

        // String properties
        builder.Property(x => x.PaymentMethod).HasMaxLength(100);
        builder.Property(x => x.Currency).HasMaxLength(10);

        // Optional timestamps
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
}
