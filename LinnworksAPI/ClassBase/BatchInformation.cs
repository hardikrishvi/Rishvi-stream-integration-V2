using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BatchInformation
    {
        /// <summary>
        /// Stock item id 
        /// </summary>
		public Guid StockItemId { get; set; }

        /// <summary>
        /// Stock item batch data 
        /// </summary>
		public List<StockItemBatch> ItemBatchInformation { get; set; }
    }
}