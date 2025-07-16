using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BatchStockLevelDeltaResponse
    {
        public List<BatchStockLevelDelta> ProcessedDeltas { get; set; }

        public Boolean ProcessedContainsErrors { get; set; }
    }
}