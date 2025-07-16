using System;

namespace LinnworksAPI
{
    public class ExtendedProperty
    {
        /// <summary>
        /// Record row ID 
        /// </summary>
		public Guid RowId { get; set; }

        public String Name { get; set; }

        public String Value { get; set; }

        public String Type { get; set; }
    }
}