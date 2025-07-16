using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class VariationGroupTemplate
    {
        /// <summary>
        /// Variation parent title 
        /// </summary>
		public String VariationGroupName { get; set; }

        /// <summary>
        /// Variation parent SKU 
        /// </summary>
		public String ParentSKU { get; set; }

        /// <summary>
        /// Variation parent stock item id 
        /// </summary>
		public Guid ParentStockItemId { get; set; }

        /// <summary>
        /// List of variation children 
        /// </summary>
		public List<Guid> VariationItemIds { get; set; }
    }
}