using System;

namespace LinnworksAPI
{
    public class PrintingKey
    {
        public Guid Key { get; set; }

        public Int32? Quantity { get; set; }

        public Int32? BatchInventoryId { get; set; }

        public Guid? LocationId { get; set; }
    }
}