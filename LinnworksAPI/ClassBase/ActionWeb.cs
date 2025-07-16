using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ActionWeb
    {
        public Int32? pkActionId { get; set; }

        public Int32 fkConditionId { get; set; }

        public String ActionName { get; set; }

        public ActionType ActionType { get; set; }

        public List<ActionWebProperty> Properties { get; set; }
    }
}