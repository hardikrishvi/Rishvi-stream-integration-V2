using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AssignStockToOrderRequest
    {
        public Guid OrderId { get; set; }

        public List<Guid> OrderItemRows { get; set; }

        /// <summary>
        /// The way in which batches should be assigned 
        /// </summary>
		public BatchAssignmentMode BatchAssignmentMode { get; set; }
    }
}