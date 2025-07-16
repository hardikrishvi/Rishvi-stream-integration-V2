using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Get_OpenOrderBasicInfoFromItemsRequest
    {
        /// <summary>
        /// List of open order iems ids to get info 
        /// </summary>
		public List<Guid> OpenOrderItems { get; set; }
    }
}