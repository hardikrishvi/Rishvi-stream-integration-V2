using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AddProductIdentifiersRequest
    {
        public IEnumerable<StockItemProductIdentifier> ProductIdentifiers { get; set; }
    }
}