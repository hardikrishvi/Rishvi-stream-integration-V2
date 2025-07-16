using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetStockItemTypeInfoRequest
    {
        public List<String> SKUS { get; set; }

        public List<Int32> StockItemIntIds { get; set; }
    }
}