using System;

namespace LinnworksAPI
{
    public class SearchOrdersRequest
    {
        public Guid? LocationId { get; set; }

        public String SearchTerm { get; set; }
    }
}