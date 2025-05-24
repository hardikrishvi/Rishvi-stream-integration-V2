using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.AdminRolePermissions.Models;

namespace Rishvi.Modules.AdminRolePermissions.Data.Configurations
{
    public class AdminPermissionConfiguration : Core.Data.IEntityTypeConfiguration<AdminPermission>
    {
        public void Map(EntityTypeBuilder<AdminPermission> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

            builder.Property(t => t.Name)
              .IsRequired()
              .HasMaxLength(100).IsUnicode(false);

            builder.Property(t => t.DisplayName)
              .IsRequired()
              .HasMaxLength(100);

            builder.Ignore(t => t.Depth);
        }
    }
}
