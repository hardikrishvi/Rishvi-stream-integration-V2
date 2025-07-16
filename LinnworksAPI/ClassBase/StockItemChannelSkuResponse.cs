using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class StockItemChannelSkuResponse
    {
        public Guid StockItemId { get; set; }

        public List<StockItemChannelSKU> ChannelSkus { get; set; }
    }
}