using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class StatusDetails
    {
        public StateType State { get; set; }

        public String Reason { get; set; }

        public Dictionary<String, String> Parameters { get; set; }
    }
}