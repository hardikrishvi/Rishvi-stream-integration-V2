using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetOrderItemBatchesByOrderIdsRequest
    {
        public List<Guid> pkOrderIds { get; set; }
    }
}