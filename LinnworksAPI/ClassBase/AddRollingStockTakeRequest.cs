using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AddRollingStockTakeRequest
    {
        /// <summary>
        /// Location Id 
        /// </summary>
		public Guid? LocationId { get; set; }

        /// <summary>
        /// Time in seconds the stock take session has taken 
        /// </summary>
		public Int32? SessionDuriationSeconds { get; set; }

        /// <summary>
        /// List of stock take items. Maximum 1000 items in a stock take session 
        /// </summary>
		public List<StockTakeItem> Items { get; set; }
    }
}