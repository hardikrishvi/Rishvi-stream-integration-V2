namespace LinnworksAPI
{
    public class AssignStockToOrdersResponse<OrderItemBatchExtended, Guid>
    {
        public BatchActionResult<OrderItemBatchExtended, Guid> BatchActionResult { get; set; }
    }
}