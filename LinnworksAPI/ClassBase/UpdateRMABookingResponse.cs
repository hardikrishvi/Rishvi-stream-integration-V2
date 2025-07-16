using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class UpdateRMABookingResponse
    {
        public Int32? RMAHeaderId { get; set; }

        public List<VerifiedRMAItem> Items { get; set; }

        public List<String> Errors { get; set; }

        public List<String> Info { get; set; }
    }
}