using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ValidationResult
    {
        /// <summary>
        /// Validation result 
        /// </summary>
		public ValidationResultTypes Type { get; set; }

        /// <summary>
        /// List of affected items 
        /// </summary>
		public List<RefundItem> InvalidItems { get; set; }

        /// <summary>
        /// Additional information 
        /// </summary>
		public String AdditionalInformation { get; set; }
    }
}