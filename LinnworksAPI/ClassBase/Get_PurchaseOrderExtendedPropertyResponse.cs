using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Get_PurchaseOrderExtendedPropertyResponse
    {
        /// <summary>
        /// Purchase order extended properties 
        /// </summary>
		public List<PurchaseOrderExtendedProperty> Items { get; set; }
    }
}