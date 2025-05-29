using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.Event
{
    public class EventFilterDto : BaseFilterDto
    {
        public EventFilterDto() 
        {
            SortColumn = "CreatedAt";
            SortType = "desc";
        }
        public string ModuleName { get; set; }
        public string Status { get; set; }
        public bool? IsError { get; set; }
    }
}
