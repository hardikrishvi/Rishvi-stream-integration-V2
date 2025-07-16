using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AddRollingStockTakeResponse
    {
        /// <summary>
        /// List of items that have failed validation, if any items are returned then the stock take won't be submitted. 
        /// </summary>
		public List<StockTakeItemWithError> ErroredItems { get; set; }
    }
}