using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GenerateFreeTextEmailRequest
    {
        /// <summary>
        /// List of ids to send template for 
        /// </summary>
		public List<Guid> ids { get; set; }

        /// <summary>
        /// Subject of email 
        /// </summary>
		public String subject { get; set; }

        /// <summary>
        /// Body of email 
        /// </summary>
		public String body { get; set; }

        public String templateType { get; set; }
    }
}