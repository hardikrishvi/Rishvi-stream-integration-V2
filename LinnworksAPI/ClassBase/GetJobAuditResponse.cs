using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetJobAuditResponse
    {
        /// <summary>
        /// List of audit trail rows 
        /// </summary>
		public List<JobAudit> AuditItems { get; set; }
    }
}