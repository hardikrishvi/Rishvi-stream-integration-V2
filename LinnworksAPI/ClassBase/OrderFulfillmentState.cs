namespace LinnworksAPI
{
    public class OrderFulfillmentState
    {
        public FulfillmentState FulfillmentState { get; set; }

        public PurchaseOrderState? PurchaseOrderState { get; set; }
    }
}