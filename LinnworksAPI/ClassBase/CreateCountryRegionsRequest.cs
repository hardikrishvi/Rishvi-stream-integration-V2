using System.Collections.Generic;

namespace LinnworksAPI
{
    public class CreateCountryRegionsRequest
    {
        /// <summary>
        /// List of country regions 
        /// </summary>
		public List<CountryRegion> regions { get; set; }
    }
}