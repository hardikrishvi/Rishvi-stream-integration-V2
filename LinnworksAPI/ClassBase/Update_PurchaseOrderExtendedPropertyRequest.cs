using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Update_PurchaseOrderExtendedPropertyRequest
    {
        /// <summary>
        /// Purchase order uniqueidentifier 
        /// </summary>
		public Guid PurchaseId { get; set; }

        /// <summary>
        /// items to update 
        /// </summary>
		public List<Update_PurchaseOrderExtendedPropertyItem> ExtendedPropertyItems { get; set; }
    }
}