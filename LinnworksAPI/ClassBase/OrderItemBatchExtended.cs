using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class OrderItemBatchExtended
    {
        public Guid pkOrderId { get; set; }

        public Guid OrderId { get; set; }

        public List<OrderItemBatch> Batches { get; set; }
    }
}