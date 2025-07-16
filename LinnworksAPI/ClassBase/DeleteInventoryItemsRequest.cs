using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class DeleteInventoryItemsRequest
    {
        public List<Guid> InventoryItemIds { get; set; }

        public List<Tuple<Int32, Int32>> SelectedRegions { get; set; }

        public Guid Token { get; set; }
    }
}