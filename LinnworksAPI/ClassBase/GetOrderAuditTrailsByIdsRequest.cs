using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetOrderAuditTrailsByIdsRequest
    {
        /// <summary>
        /// List of order ids 
        /// </summary>
		public List<Guid> OrderIds { get; set; }
    }
}