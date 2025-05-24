using Z.EntityFramework.Plus;

namespace Rishvi.Modules.Configurations.Admin.CacheManagers
{
    public static class ConfigurationCacheManager
    {
        public static void ClearCache()
        {
            QueryCacheManager.ExpireTag(Name);
        }

        public static string Name { get; set; } = "Configurations";
    }
}