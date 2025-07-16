using System;

namespace LinnworksAPI
{
    public class GetOrderItemIndicatorRequest
    {
        public Guid LocationId { get; set; }

        public IndicatorRequest[] Items { get; set; }
    }
}