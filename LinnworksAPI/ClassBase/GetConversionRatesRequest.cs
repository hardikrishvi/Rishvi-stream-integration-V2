using System;

namespace LinnworksAPI
{
    public class GetConversionRatesRequest
    {
        /// <summary>
        /// If you want to get currencies from orders 
        /// </summary>
		public Boolean GetCurrenciesFromOrders { get; set; }

        /// <summary>
        /// Currency 
        /// </summary>
		public String Currency { get; set; }
    }
}