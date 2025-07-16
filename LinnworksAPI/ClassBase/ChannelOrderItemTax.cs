using System;

namespace LinnworksAPI
{
    public class ChannelOrderItemTax
    {
        public String TaxType { get; set; }

        public Decimal TaxValue { get; set; }

        public Boolean IsSellerCollected { get; set; }
    }
}