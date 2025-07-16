using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ConfigRule
    {
        public String FieldName { get; set; }

        public List<PropertyRule> Rules { get; set; }
    }
}