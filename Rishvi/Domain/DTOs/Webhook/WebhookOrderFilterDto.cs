using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;

namespace Rishvi.Domain.DTOs.Webhook
{
    public class WebhookOrderFilterDto : BaseFilterDto
    {
        public WebhookOrderFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "desc";
        }
        public string ModuleName { get; set; }
        public string Status { get; set; }
        public bool? IsError { get; set; }
    }
}
