using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class DeleteVariationGroupsRequest
    {
        /// <summary>
        /// /// A list of VariationsGroups Guids 
        /// </summary>
		public IEnumerable<Guid> VariationGroupsIdList { get; set; }
    }
}