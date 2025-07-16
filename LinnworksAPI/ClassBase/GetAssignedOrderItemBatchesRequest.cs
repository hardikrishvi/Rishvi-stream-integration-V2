using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetAssignedOrderItemBatchesRequest
    {
        public List<Guid> OrderItemRows { get; set; }
    }
}