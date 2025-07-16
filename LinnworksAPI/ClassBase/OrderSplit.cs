using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Split Order 
    /// </summary>
    public class OrderSplit
    {
        /// <summary>
        /// Items 
        /// </summary>
		public List<OrderSplitOutItem> Items { get; set; }

        /// <summary>
        /// Postal Service 
        /// </summary>
		public Guid PostalServiceId { get; set; }
    }
}