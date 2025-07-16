using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BinracksResponse
    {
        /// <summary>
        /// List of binracks available for the given item in the order applicability. 
        /// </summary>
		public List<WarehouseBinRack> BinRacks { get; set; }

        public List<String> Decisions { get; set; }
    }
}