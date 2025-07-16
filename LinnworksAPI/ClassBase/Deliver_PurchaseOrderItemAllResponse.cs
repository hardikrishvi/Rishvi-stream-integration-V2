using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Response from delivering all items in an open/partial PO 
    /// </summary>
    public class Deliver_PurchaseOrderItemAllResponse
    {
        /// <summary>
        /// Delivered items 
        /// </summary>
		public List<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        /// <summary>
        /// Recalculated purchase order header 
        /// </summary>
		public PurchaseOrderHeader PurchaseOrderHeader { get; set; }
    }
}