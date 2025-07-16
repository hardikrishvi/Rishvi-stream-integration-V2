using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class DeleteVariationItemsRequest
    {
        public Guid VariationItemId { get; set; }

        public List<Guid> StockItemIds { get; set; }
    }
}