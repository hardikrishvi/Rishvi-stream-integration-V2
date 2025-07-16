using System.Collections.Generic;

namespace LinnworksAPI
{
    public class UpdateProductIdentifiersRequest
    {
        public IEnumerable<StockItemProductIdentifier> ProductIdentifiers { get; set; }
    }
}