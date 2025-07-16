using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ChannelReason
    {
        public String Tag { get; set; }

        public String DisplayName { get; set; }

        public ChannelReasonTypes Types { get; set; }

        public List<ChannelSubReason> SubReasons { get; set; }
    }
}