using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class AddressConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<Address>
{
    public void Map(EntityTypeBuilder<Address> builder)
    {
        
        // Primary Key
        builder.HasKey(x => x.Id);

        // Auto-generate new GUID for Id
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

        // Basic field constraints
        builder.Property(x => x.EmailAddress).HasMaxLength(255);
        builder.Property(x => x.Address1).HasMaxLength(255);
        builder.Property(x => x.Address2).HasMaxLength(255);
        builder.Property(x => x.Address3).HasMaxLength(255);
        builder.Property(x => x.Town).HasMaxLength(100);
        builder.Property(x => x.Region).HasMaxLength(100);
        builder.Property(x => x.PostCode).HasMaxLength(50);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.Continent).HasMaxLength(100);
        builder.Property(x => x.FullName).HasMaxLength(255);
        builder.Property(x => x.Company).HasMaxLength(255);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);

        // Foreign key (if CountryId links to another table, else leave as is)
        builder.Property(x => x.CountryId);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);
    }
}

