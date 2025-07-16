using System.Collections.Generic;

namespace LinnworksAPI
{
    /// <summary>
    /// Response of GetWorkflow 
    /// </summary>
    public class GetWorkflowResponse
    {
        /// <summary>
        /// List of workflow groups with up to 50 jobs arranged in To Do, and then decending datetime stamp 
        /// </summary>
		public List<WorkflowGroup> WorkflowGroups { get; set; }
    }
}