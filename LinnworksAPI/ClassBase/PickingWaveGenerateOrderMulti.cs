using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class PickingWaveGenerateOrderMulti
    {
        /// <summary>
        /// Items to be added to the pickwave. 
        /// </summary>
		public List<PickingWaveGenerateItemMulti> Items { get; set; }

        public Int32 OrderId { get; set; }
    }
}