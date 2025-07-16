using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetInventoryItemBatchInformationByIdsRequest
    {
        /// <summary>
        /// A list of stock item ids 
        /// </summary>
		public List<Guid> StockItemIds { get; set; }

        /// <summary>
        /// The location to get the batch information from 
        /// </summary>
		public Guid? StockLocationId { get; set; }

        /// <summary>
        /// Defines whether we should only return available items 
        /// </summary>
		public Boolean AvailableOnly { get; set; }
    }
}