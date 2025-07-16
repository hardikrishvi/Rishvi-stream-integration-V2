using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetGroupListResponse
    {
        /// <summary>
        /// List of groups. Does not return condition and actions objects 
        /// </summary>
		public List<Group> GroupList { get; set; }
    }
}