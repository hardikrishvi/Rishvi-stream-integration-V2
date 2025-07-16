using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Request for allocatioable orders to pickwave. 
    /// </summary>
    public class CheckAllocatableToPickwaveRequest
    {
        /// <summary>
        /// List of integer order ids 
        /// </summary>
		public List<Int32> OrderIds { get; set; }
    }
}