using Rishvi.Modules.Core.Filters;
namespace Rishvi.Modules.ErrorLogs.Models.DTOs
{
    public class SystemLogFilterDto : BaseFilterDto
    {
        public SystemLogFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "DESC";
        }
        public string ModuleName { get; set; }
        public string Status { get; set; }  
        public bool? IsError { get; set; }
    }
}
