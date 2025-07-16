using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BatchAssignmentForOrderItems
    {
        /// <summary>
        /// Identifies the order to be updated 
        /// </summary>
		public Guid orderId { get; set; }

        /// <summary>
        /// Maps order items to batches 
        /// </summary>
		public List<batchAssignment> batchToItemMapping { get; set; }
    }
}