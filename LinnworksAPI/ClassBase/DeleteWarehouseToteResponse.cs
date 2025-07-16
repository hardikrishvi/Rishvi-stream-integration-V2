using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class DeleteWarehouseToteResponse
    {
        /// <summary>
        /// deleted list of totes 
        /// </summary>
		public List<Int32> DeletedToteIds { get; set; }
    }
}