using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
namespace Rishvi.Modules.ErrorLogs.Data.Seed
{
    public class SystemLogPermissionsSeed : BaseSeed
    {
        public SystemLogPermissionsSeed(SqlContext context) : base(context)
        {
            OrderId = 520;
        }
        public override void Seed()
        {
        }
        private void CreateSystemPermissions()
        {
        }
    }
}
