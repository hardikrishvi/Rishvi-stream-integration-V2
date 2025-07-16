using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetStockItemsByIdsRequest
    {
        /// <summary>
        /// Collection of Stock item id (uniqueidentifier) 
        /// </summary>
		public List<Guid> StockItemIds { get; set; }
    }
}