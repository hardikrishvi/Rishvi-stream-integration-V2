using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Get_AdditionalCostTypesResponse
    {
        /// <summary>
        /// List of additional costs types 
        /// </summary>
		public List<PurchaseOrderAdditionalCostType> AdditionalTypes { get; set; }
    }
}