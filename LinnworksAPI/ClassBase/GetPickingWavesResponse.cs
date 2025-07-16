using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetPickingWavesResponse
    {
        /// <summary>
        /// Pickwaves 
        /// </summary>
		public List<PickingWaveDetailed> PickingWaves { get; set; }

        /// <summary>
        /// List of SKUs. 
        /// </summary>
		public List<StockItemInfo> Skus { get; set; }

        /// <summary>
        /// List of bins and batches of items in the bins. 
        /// </summary>
		public List<BinRackStockItem> Bins { get; set; }
    }
}