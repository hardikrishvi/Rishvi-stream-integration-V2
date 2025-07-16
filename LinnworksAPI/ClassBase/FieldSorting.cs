using System;

namespace LinnworksAPI
{
    public class FieldSorting
    {
        /// <summary>
        /// Field code to sort by 
        /// </summary>
		public FieldCode FieldCode { get; set; }

        /// <summary>
        /// Sorting direction (Ascending:0, descending:1) 
        /// </summary>
		public ListSortDirection Direction { get; set; }

        /// <summary>
        /// Priority of sorting (e.g. sort by date first, then by status) 
        /// </summary>
		public Int32 Order { get; set; }
    }
}