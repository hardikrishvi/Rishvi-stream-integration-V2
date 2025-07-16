using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Deliver_PurchaseItems_WithQuantityRequest
    {
        public Guid pkPurchaseId { get; set; }

        public List<PODeliveredItems> Items { get; set; }
    }
}