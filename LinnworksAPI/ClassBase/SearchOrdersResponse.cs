using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class SearchOrdersResponse
    {
        public OrderViewIds[] OpenOrders { get; set; }

        public HashSet<Guid> ProcessedOrders { get; set; }
    }
}