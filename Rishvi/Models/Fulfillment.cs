using Rishvi.Core.Data;

namespace Rishvi.Models;

public class Fulfillment : IModificationHistory
{
    public Guid Id { get; set; }
    public string FulfillmentState { get; set; }
    public string PurchaseOrderState { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}