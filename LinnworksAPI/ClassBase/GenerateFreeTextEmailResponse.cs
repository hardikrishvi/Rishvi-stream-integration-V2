using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GenerateFreeTextEmailResponse
    {
        /// <summary>
        /// Send custom emails completed 
        /// </summary>
		public Boolean isComplete { get; set; }

        /// <summary>
        /// List of failed recipients 
        /// </summary>
		public List<String> FailedRecipients { get; set; }
    }
}