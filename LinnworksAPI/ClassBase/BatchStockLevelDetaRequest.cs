using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BatchStockLevelDetaRequest
    {
        public List<BatchStockLevelDelta> StockLevelDeltas { get; set; }

        public Guid StockLocationId { get; set; }
    }
}