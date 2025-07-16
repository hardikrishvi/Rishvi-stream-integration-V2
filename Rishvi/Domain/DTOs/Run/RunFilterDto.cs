using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.Run
{
    public class RunFilterDto : BaseFilterDto
    {
        public RunFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "desc";
        }
        public string ModuleName { get; set; }
        public string Status { get; set; }
        public bool? IsError { get; set; }
    }
}
