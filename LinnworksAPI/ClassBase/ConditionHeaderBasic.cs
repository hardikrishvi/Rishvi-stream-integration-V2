using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ConditionHeaderBasic
    {
        public Int32 pkConditionId { get; set; }

        public Int32? fkParentConditionId { get; set; }

        public Int32 fkRuleId { get; set; }

        public String ConditionName { get; set; }

        public Boolean Enabled { get; set; }

        public List<ConditionItemBasic> Conditions { get; set; }
    }
}