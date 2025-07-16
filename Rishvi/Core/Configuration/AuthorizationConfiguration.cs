using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class AuthorizationConfiguration
    {
        public void Map(EntityTypeBuilder<Authorization> builder)
        {

            // Define the primary key
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.PrimitiveCollection(x => x.IntegratedDateTime)
                .IsRequired();
            builder.Property(x => x.IsConfigActive).IsRequired();
            builder.Property(x => x.ConfigStatus).HasMaxLength(50);
            builder.Property(x => x.AddressLine1).HasMaxLength(255);
            builder.Property(x => x.CompanyName).HasMaxLength(255);
            builder.Property(x => x.AddressLine2).HasMaxLength(255);
            builder.Property(x => x.AddressLine3).HasMaxLength(255);
            builder.Property(x => x.City).HasMaxLength(100);
            builder.Property(x => x.ContactName).HasMaxLength(100);
            builder.Property(x => x.ContactPhoneNo).HasMaxLength(50);
            builder.Property(x => x.CountryCode).HasMaxLength(10);
            builder.Property(x => x.County).HasMaxLength(100);
            builder.Property(x => x.PostCode).HasMaxLength(20);
            builder.Property(x => x.SessionID).HasMaxLength(100);
            builder.Property(x => x.LabelReference).HasMaxLength(100);
            builder.Property(x => x.Email).HasMaxLength(255);
            builder.Property(x => x.LinnworksUniqueIdentifier).HasMaxLength(50);
            builder.Property(x => x.AuthorizationToken).HasMaxLength(50);
            builder.Property(x => x.AccountName).HasMaxLength(100);
            builder.Property(x => x.ClientId).HasMaxLength(100);
            builder.Property(x => x.ClientSecret).HasMaxLength(255);
            builder.Property(x => x.access_token).HasMaxLength(255);
            builder.Property(x => x.ExpirationTime).IsRequired();
            builder.Property(x => x.expires_in).IsRequired(false);
            builder.Property(x => x.refresh_token).HasMaxLength(255);
            builder.Property(x => x.refresh_token_expires_in).HasMaxLength(30);
            builder.Property(x => x.token_type).HasMaxLength(50);
            builder.Property(x => x.FtpHost).HasMaxLength(255);
            builder.Property(x => x.FtpUsername).HasMaxLength(100);
            builder.Property(x => x.FtpPassword).HasMaxLength(255);
            builder.Property(x => x.FtpPort).IsRequired();
            builder.Property(x => x.LinnworksToken).HasMaxLength(50);
            builder.Property(x => x.LinnworksServer).HasMaxLength(255);
            builder.Property(x => x.LinnRefreshToken).HasMaxLength(50);
            builder.Property(x => x.fulfiilmentLocation).HasMaxLength(255);
            builder.Property(x => x.PartyFileCreated)
                .IsRequired()
                .HasDefaultValue(false);

            // Timestamps
            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt)
                    .IsRequired(false);
        }
    }
}
