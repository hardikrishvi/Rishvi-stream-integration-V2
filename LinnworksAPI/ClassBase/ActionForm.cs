using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ActionForm
    {
        public String Caption { get; set; }

        public List<ActionFormControl> Controls { get; set; }
    }
}