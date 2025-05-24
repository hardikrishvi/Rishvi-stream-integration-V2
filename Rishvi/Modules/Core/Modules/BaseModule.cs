using System;
using Microsoft.AspNetCore.Routing;

namespace Rishvi.Modules.Core.Modules
{
    public abstract class BaseModule : IComparable<BaseModule>
    {
        public string ModuleName { get; set; }
        public int OrderId { get; set; }

        public abstract void RegisterRoutes(RouteCollection routes);

        public int CompareTo(BaseModule other)
        {
            return OrderId.CompareTo(other.OrderId);
        }
    }
}