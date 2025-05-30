using Rishvi.Modules.Core.Filters;

namespace Rishvi.DTOs.Fulfillment;

public class FulfillmentFilterDto : BaseFilterDto
{
    
    public FulfillmentFilterDto()
    {
        SortColumn = "CreatedAt";
        SortType = "DESC";
    }
    public string FulfillmentState { get; set; }
    public string PurchaseOrderState { get; set; }
}