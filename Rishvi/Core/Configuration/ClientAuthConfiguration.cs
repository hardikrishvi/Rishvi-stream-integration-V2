using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Core.Configuration
{
    public class ClientAuthConfiguration
    {
        public void Map(EntityTypeBuilder<ClientAuth> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ClientId)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.AccessToken).HasMaxLength(100);
            builder.Property(x => x.ExpireTime)
                .IsRequired();
            builder.Property(x => x.scope).HasMaxLength(255);
            builder.Property(x => x.TokenType).HasMaxLength(50);
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(x => x.UpdatedAt).IsRequired(false);
        }
    }
}
