using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class PartialUpdateParameter
    {
        public Guid pkId { get; set; }

        public List<KeyValuePair<String, String>> fieldList { get; set; }
    }
}