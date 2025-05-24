using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Data.Configurations
{
    public class AdminRolesAdminPermissionsConfiguration : IEntityTypeConfiguration<AdminRolesAdminPermissions>
    {
        public void Map(EntityTypeBuilder<AdminRolesAdminPermissions> builder)
        {
            builder.HasKey(t => new { t.AdminRoleId, t.AdminPermissionId });
        }
    }
}
