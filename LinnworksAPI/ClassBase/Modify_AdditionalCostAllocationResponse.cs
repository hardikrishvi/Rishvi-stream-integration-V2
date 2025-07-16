using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Modify_AdditionalCostAllocationResponse
    {
        /// <summary>
        /// list of modified items with Ids matched to CostAllocationId 
        /// </summary>
		public List<ModifiedAdditionalCostAllocationItem> ModifiedItems { get; set; }
    }
}