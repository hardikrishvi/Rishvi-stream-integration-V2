using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class CustomScriptResult
    {
        public Boolean IsError { get; set; }

        public String ErrorMessage { get; set; }

        public Int64 TotalResults { get; set; }

        public List<CustomScriptColumn> Columns { get; set; }

        public List<Dictionary<String, Object>> Results { get; set; }
    }
}