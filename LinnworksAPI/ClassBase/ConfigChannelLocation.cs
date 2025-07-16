using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public class ConfigChannelLocation
    {
        public Int32 pkChannelLocationId { get; set; }

        public Boolean Deleted { get; set; }

        public String Identifier { get; set; }

        public String Name { get; set; }

        public Guid OrderDownloadLocation { get; set; }

        public List<Guid> InventorySyncLocations { get; set; }

        public Boolean IsFulfilment { get; set; }
    }
}