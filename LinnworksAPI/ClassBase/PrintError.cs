using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class PrintError
    {
        public String TemplateType { get; set; }

        public Guid Id { get; set; }

        public String Error { get; set; }

        public List<PrintErrorTags> Tags { get; set; }
    }
}