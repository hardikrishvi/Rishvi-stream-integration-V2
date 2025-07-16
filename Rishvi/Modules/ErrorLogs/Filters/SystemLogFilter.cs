using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.ErrorLogs.Models.DTOs;
namespace Rishvi.Modules.ErrorLogs.Filters
{
    public class SystemLogFilter : BaseFilter<SystemLogListDto, SystemLogFilterDto>
    {
        public SystemLogFilter(IQueryable<SystemLogListDto> query, SystemLogFilterDto dto) : base(query, dto)
        {
        }
        internal void ModuleName()
        {
            Query = Query.Where(w => w.ModuleName.Contains(Dto.ModuleName));
        }
        internal void IsError()
        {
            Query = Query.Where(w => w.IsError == Dto.IsError);
        }
        internal void Status()
        {
            Query = Query.Where(w => w.Status.Equals(Dto.Status));
        }
        internal void FromCreatedAt()
        {
            Query = Query.Where(w => w.CreatedAt >= Dto.FromCreatedAt);
        }
        internal void ToCreatedAt()
        {
            Query = Query.Where(w => w.CreatedAt <= Dto.ToCreatedAt);
        }
    }
}
