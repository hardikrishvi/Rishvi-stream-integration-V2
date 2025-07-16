using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetProductIdentifiersByStockItemIdResponse
    {
        public IEnumerable<StockItemProductIdentifier> ProductIdentifiers { get; set; }
    }
}