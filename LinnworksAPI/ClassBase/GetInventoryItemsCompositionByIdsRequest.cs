using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetInventoryItemsCompositionByIdsRequest
    {
        public List<Guid> InventoryItemIds { get; set; }
    }
}