using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetPickingWaveHeadersResponse
    {
        /// <summary>
        /// List of pickingwave headers without order details 
        /// </summary>
		public List<PickingWave> PickwaveHeaders { get; set; }
    }
}