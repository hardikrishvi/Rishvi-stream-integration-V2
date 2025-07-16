using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetOpenOrdersDetailsResponse
    {
        /// <summary>
        /// List of orders 
        /// </summary>
		public List<OrderDetails> Orders { get; set; }
    }
}