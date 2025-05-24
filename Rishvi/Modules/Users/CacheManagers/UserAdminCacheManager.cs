using Z.EntityFramework.Plus;

namespace Rishvi.Modules.Users.CacheManagers
{
    public class UserAdminCacheManager
    {
        public static void ClearCache()
        {
            QueryCacheManager.ExpireTag(Name);
        }

        public static string Name { get; set; } = "Users";
    }
}
