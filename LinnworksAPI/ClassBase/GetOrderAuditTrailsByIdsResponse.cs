using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetOrderAuditTrailsByIdsResponse
    {
        public List<OrderAuditTrailExtended> AuditTrails { get; set; }
    }
}