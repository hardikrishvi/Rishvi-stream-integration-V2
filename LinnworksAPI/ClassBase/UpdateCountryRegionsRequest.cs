using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Country region information to update 
    /// </summary>
    public class UpdateCountryRegionsRequest
    {
        public List<CountryRegion> regions { get; set; }
    }
}