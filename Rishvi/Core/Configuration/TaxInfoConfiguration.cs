using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class TaxInfoConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<TaxInfo>
{
    public void Map(EntityTypeBuilder<TaxInfo> builder)
    {
        // Primary Key
        builder.HasKey(x => x.TaxInfoId);
        builder.Property(x => x.TaxInfoId).HasDefaultValueSql("NEWID()");

        // Properties
        builder.Property(x => x.TaxNumber).HasMaxLength(100);

        // Timestamps
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
}