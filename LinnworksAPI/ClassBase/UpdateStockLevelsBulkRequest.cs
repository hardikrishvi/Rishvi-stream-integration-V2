using System.Collections.Generic;

namespace LinnworksAPI
{
    public class UpdateStockLevelsBulkRequest
    {
        public List<UpdateStockLevelsBulkRequestItem> Items { get; set; }
    }
}