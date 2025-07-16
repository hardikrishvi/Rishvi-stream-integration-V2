using System;

namespace LinnworksAPI
{
    public class GetStockItemBatchesByLocationRequest
    {
        public Guid StockItemId { get; set; }

        public Guid LocationId { get; set; }

        public Boolean OnlyAvailable { get; set; }
    }
}