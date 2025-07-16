using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class AssignStockToOrdersRequest
    {
        public List<Guid> OrderIds { get; set; }

        public BatchAssignmentMode BatchAssignmentMode { get; set; }
    }
}