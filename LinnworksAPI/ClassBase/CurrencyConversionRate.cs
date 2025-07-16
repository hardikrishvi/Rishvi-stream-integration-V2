using System;

namespace LinnworksAPI
{
    public class CurrencyConversionRate
    {
        /// <summary>
        /// Currency code (e.g. GBP) 
        /// </summary>
		public String Currency { get; set; }

        /// <summary>
        /// Currency conversion rate to default currency 
        /// </summary>
		public Double ConversionRate { get; set; }
    }
}