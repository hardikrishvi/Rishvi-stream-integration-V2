using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class MoveToFulfilmentCenterResult
    {
        /// <summary>
        /// List of errors 
        /// </summary>
		public List<String> Errors { get; set; }

        /// <summary>
        /// List of pkOrderIds that were moved 
        /// </summary>
		public List<Guid> OrdersMoved { get; set; }
    }
}