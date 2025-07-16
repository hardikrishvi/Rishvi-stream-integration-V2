using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class OrderAuditTrailExtended
    {
        /// <summary>
        /// Order id 
        /// </summary>
		public Guid pkOrderId { get; set; }

        /// <summary>
        /// List of OrderAuditTrail entities 
        /// </summary>
		public List<OrderAuditTrail> AuditTrail { get; set; }
    }
}