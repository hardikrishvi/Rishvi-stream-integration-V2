using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Add_PurchaseOrderExtendedPropertyRequest
    {
        /// <summary>
        /// Purchase order uniqueidentifier 
        /// </summary>
		public Guid PurchaseId { get; set; }

        /// <summary>
        /// List of Extended Properties to be added to the purchase order 
        /// </summary>
		public List<Add_PurchaseOrderExtendedProperty_Item> ExtendedPropertyItems { get; set; }
    }
}