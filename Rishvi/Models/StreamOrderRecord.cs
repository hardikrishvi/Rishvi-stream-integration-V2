using Rishvi.Core.Data;

namespace Rishvi.Models;

public class StreamOrderRecord : IModificationHistory
{
    public Guid Id { get; set; }
    public string JsonData { get; set; }
    public string AuthorizationToken { get; set; }
    public string Email { get; set; }
    public string EbayOrderId { get; set; }
    public string LinnworksOrderId { get; set; }
    public string ConsignmentId { get; set; }
    public string TrackingNumber { get; set; }
    public string TrackingUrl { get; set; }
    public string TrackingId { get; set; } 
    public string Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}