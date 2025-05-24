using Z.EntityFramework.Plus;

namespace Rishvi.Modules.AdminUsers.Admin.CacheManagers
{
    public static class AdminUsersCacheManager
    {
        public static void ClearCache()
        {
            QueryCacheManager.ExpireTag(Name);
        }

        public static string Name { get; set; } = "AdminUsers";
    }
}
