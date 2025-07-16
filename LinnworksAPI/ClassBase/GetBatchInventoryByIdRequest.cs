using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class GetBatchInventoryByIdRequest
    {
        public List<Int32> BatchInventoryIds { get; set; }

        public Boolean LoadRelatedInventoryLines { get; set; }
    }
}