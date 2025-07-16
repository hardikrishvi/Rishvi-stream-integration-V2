using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetRefundLinesByHeaderIdResponse
    {
        /// <summary>
        /// The unique identifier for the loaded refund 
        /// </summary>
		public Int32 RefundHeaderId { get; set; }

        /// <summary>
        /// A collection of all refunds associated with the loaded header 
        /// </summary>
		public ISet<VerifiedRefund> RefundLines { get; set; }

        /// <summary>
        /// Channel-specific information about refund requirements, e.g. accepted refund reasons 
        /// </summary>
		public RefundOptions RefundOptions { get; set; }
    }
}