using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Pickwave item update request. 
    /// </summary>
    public class PickingWaveItemUpdateRequest
    {
        /// <summary>
        /// List of pickwave items to update 
        /// </summary>
		public List<PickingWaveItemUpdate> WaveItemUpdates { get; set; }
    }
}