using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;
using Rishvi.Modules.ErrorLogs.Models.DTOs;
using System.Linq;
namespace Rishvi.Modules.ErrorLogs.ListOrders
{
    public class SystemLogListOrder : BaseListOrder<SystemLogListDto>
    {
        public SystemLogListOrder(IQueryable<SystemLogListDto> query, BaseFilterDto dto) : base(query, dto)
        {
        }
        internal void CreatedAt()
        {
            Query = OrderBy(t => t.CreatedAt);
        }
    }
}
