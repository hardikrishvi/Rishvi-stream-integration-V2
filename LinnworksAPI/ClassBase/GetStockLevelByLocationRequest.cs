using System;

namespace LinnworksAPI
{
    public class GetStockLevelByLocationRequest
    {
        public Guid StockItemId { get; set; }

        public Guid LocationId { get; set; }
    }
}