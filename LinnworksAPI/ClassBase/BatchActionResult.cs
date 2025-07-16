using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BatchActionResult<OrderItemBatchExtended, Guid>
    {
        public List<OrderItemBatchExtended> ProcessedOrders { get; set; }

        public Dictionary<String, List<Guid>> UnprocessedOrders { get; set; }
    }
}