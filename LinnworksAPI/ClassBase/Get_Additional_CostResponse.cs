using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Get_Additional_CostResponse
    {
        /// <summary>
        /// List of additional cost items 
        /// </summary>
		public List<PurchaseOrderAdditionalCost> items { get; set; }
    }
}