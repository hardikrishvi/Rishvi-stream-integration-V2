using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetInventoryItemsCompositionByIdsResponse
    {
        public Dictionary<Guid, List<StockItemComposition>> InventoryItemsCompositionByIds { get; set; }
    }
}