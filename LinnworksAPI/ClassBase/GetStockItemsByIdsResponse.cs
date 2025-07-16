using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetStockItemsByIdsResponse
    {
        /// <summary>
        /// List of stock item headers. 
        /// </summary>
		public List<StockItemHeader> Items { get; set; }
    }
}