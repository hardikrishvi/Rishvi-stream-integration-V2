using System;

namespace LinnworksAPI
{
    public class GetLatestCurrencyRateRequest
    {
        /// <summary>
        /// Base currency for conversion rates, if null, USD is used 
        /// </summary>
		public String BaseCurrency { get; set; }
    }
}