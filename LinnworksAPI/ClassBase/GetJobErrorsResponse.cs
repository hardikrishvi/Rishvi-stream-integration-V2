using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetJobErrorsResponse
    {
        /// <summary>
        /// List of errors for a job 
        /// </summary>
		public List<JobError> JobErrors { get; set; }
    }
}