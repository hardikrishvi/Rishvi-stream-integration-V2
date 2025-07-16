using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Delete_PurchaseOrderExtendedPropertyRequest
    {
        /// <summary>
        /// Purchase order unique identifier 
        /// </summary>
		public Guid PurchaseId { get; set; }

        /// <summary>
        /// List of ids to delete 
        /// </summary>
		public List<Int32> RowIds { get; set; }
    }
}