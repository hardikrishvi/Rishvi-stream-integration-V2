using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetStockItemsFullByIdsRequest
    {
        public List<Guid> StockItemIds { get; set; }

        public List<StockItemFullExtendedDataRequirement> DataRequirements { get; set; }
    }
}