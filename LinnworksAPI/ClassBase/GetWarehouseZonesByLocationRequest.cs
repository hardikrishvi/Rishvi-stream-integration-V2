using System;

namespace LinnworksAPI
{
    public class GetWarehouseZonesByLocationRequest
    {
        public Int32 StockLocationIntId { get; set; }

        public Boolean OnlyBinrackAssignable { get; set; }
    }
}