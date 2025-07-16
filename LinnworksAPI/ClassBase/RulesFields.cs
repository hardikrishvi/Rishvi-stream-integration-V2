using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class RulesFields
    {
        public List<String> FieldNames { get; set; }

        public Dictionary<String, List<String>> FieldKeys { get; set; }

        public String IdName { get; set; }
    }
}