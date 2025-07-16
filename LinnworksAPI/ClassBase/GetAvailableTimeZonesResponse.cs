using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetAvailableTimeZonesResponse
    {
        public IEnumerable<AvailableTimeZone> TimeZones { get; set; }
    }
}