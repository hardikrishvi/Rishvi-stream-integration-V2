using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetProductIdentifiersBulkByStockItemIdRequest
    {
        public IEnumerable<Guid> StockItemIds { get; set; }
    }
}