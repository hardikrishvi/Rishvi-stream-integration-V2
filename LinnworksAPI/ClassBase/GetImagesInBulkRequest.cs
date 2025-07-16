using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetImagesInBulkRequest
    {
        public List<Guid> StockItemIds { get; set; }

        public List<String> SKUS { get; set; }
    }
}