using Z.EntityFramework.Plus;

namespace Rishvi.Modules.AdminRolePermissions.Admin.CacheManagers
{
    public class AdminPermissionCacheManager
    {
        public static void ClearCache()
        {
            QueryCacheManager.ExpireTag(Name);
        }

        public static string Name { get; set; } = "List";
    }
}
