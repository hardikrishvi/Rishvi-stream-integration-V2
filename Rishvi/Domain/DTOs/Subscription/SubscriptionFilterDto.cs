using Rishvi.Modules.Core.Filters;

namespace Rishvi.Domain.DTOs.Subscription
{
    public class SubscriptionFilterDto : BaseFilterDto
    {
        public SubscriptionFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "desc";
        }
        public string ModuleName { get; set; }
        public string Status { get; set; }
        public bool? IsError { get; set; }
    }
}

