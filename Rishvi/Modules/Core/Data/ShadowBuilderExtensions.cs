using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Rishvi.Modules.Core.Data
{
    public static class ShadowBuilderExtensions
    {
        public static void ShadowProperties(this ModelBuilder modelBuilder)
        {
            var entityTypes = typeof(SqlContext).GetTypeInfo().Assembly.GetTypes().Where(t => typeof(IModificationHistory).IsAssignableFrom(t) && typeof(IModificationHistory) != t);
            foreach (var tp in entityTypes)
            {
                var method = SetAuditingShadowPropertiesMethodInfo.MakeGenericMethod(tp);
                method.Invoke(modelBuilder, new object[] { modelBuilder });
            }
        }

        private static readonly MethodInfo SetAuditingShadowPropertiesMethodInfo = typeof(ShadowBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
           .Single(t => t.IsGenericMethod && t.Name == "SetAuditingShadowProperties");

        public static void SetAuditingShadowProperties<T>(ModelBuilder builder) where T : class, IModificationHistory
        {
            builder.Entity<T>().Property<DateTime>("CreatedAt").HasDefaultValueSql("GetDate()");
            builder.Entity<T>().Property<DateTime?>("UpdatedAt").HasDefaultValueSql("GetDate()");
        }
    }
}
