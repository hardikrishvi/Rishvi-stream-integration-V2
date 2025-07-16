using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class DeleteProductIdentifiersRequest
    {
        public IEnumerable<Int64> ProductIdentifierIds { get; set; }
    }
}