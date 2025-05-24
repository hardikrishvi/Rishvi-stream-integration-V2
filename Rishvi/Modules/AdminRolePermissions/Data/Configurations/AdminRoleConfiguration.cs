using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.AdminRolePermissions.Models;

namespace Rishvi.Modules.AdminRolePermissions.Data.Configurations
{
    public class AdminRoleConfiguration : Core.Data.IEntityTypeConfiguration<AdminRole>
    {
        public void Map(EntityTypeBuilder<AdminRole> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

            builder.Property(t => t.Name)
              .IsRequired()
              .HasMaxLength(50);

            builder.Property(t => t.SystemName)
              .IsRequired()
              .HasMaxLength(50);
        }
    }
}
