using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Add_PurchaseOrderExtendedPropertyResponse
    {
        /// <summary>
        /// Added purchase order extended properties 
        /// </summary>
		public List<PurchaseOrderExtendedProperty> Items { get; set; }
    }
}