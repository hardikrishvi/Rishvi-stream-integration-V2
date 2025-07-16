using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Modules.Core.Helpers;

namespace Rishvi
{
    public static class SeedExtension
    {
        public static void ExecuteSeeds(string connection)
        {
            //using var scope = host.Services.CreateScope();
            //var service = scope.ServiceProvider;

            var contextOptions = new DbContextOptionsBuilder<SqlContext>()
                                   .UseSqlServer(connection).Options;

            using var context = new SqlContext(contextOptions);

            //var context = service.GetRequiredService<SqlContext>();

            ///it will apply all pending migration, but we will apply migration manually, so I have commented following line
            //context.Database.Migrate();
            context.Database.EnsureCreated();

            ObjectHelper.GetEnumerableOfType<BaseSeed>(context)
                    .ForEach(seedClass => seedClass.Seed());

            //NestedSet.BuildTree(context, "AdminPermission");
        }
    }
}
