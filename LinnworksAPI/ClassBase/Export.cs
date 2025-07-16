using System.Collections.Generic;

namespace LinnworksAPI
{
    public class Export
    {
        /// <summary>
        /// Export Base Settings (Delimeters, Columns mapping etc) 
        /// </summary>
		public ExportSpecification Specification { get; set; }

        public ExportRegister Register { get; set; }

        public List<Schedule> Schedules { get; set; }
    }
}