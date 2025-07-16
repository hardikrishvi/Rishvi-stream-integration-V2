using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class BulkScrapBatchedItemsRequest
    {
        public Guid LocationId { get; set; }

        public List<ScrapItemExtended> ScrapItems { get; set; }
    }
}