using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class MultiOptionResponse
    {
        /// <summary>
        /// Evaluation field 
        /// </summary>
		public String Field { get; set; }

        /// <summary>
        /// Evaluation key 
        /// </summary>
		public String Key { get; set; }

        /// <summary>
        /// Evaluation options 
        /// </summary>
		public List<String> Options { get; set; }

        public List<OptionBase> KeyedOptions { get; set; }
    }
}