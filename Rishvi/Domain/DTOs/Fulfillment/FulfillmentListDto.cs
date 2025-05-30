namespace Rishvi.DTOs.Fulfillment;

public class FulfillmentListDto
{
    public Guid Id { get; set; }
    public string FulfillmentState { get; set; }
    public string PurchaseOrderState { get; set; }
}