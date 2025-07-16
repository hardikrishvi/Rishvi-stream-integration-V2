using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Check allocatable to pickwave response. 
    /// </summary>
    public class CheckAllocatableToPickwaveResponse
    {
        /// <summary>
        /// List of results 
        /// </summary>
		public List<PickWaveAllocateCheckResult> Results { get; set; }
    }
}