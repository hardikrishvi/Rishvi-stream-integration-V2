using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetOrderItemBatchesByOrderIdsResponse
    {
        public List<OrderItemBatchExtended> OrderBatches { get; set; }
    }
}