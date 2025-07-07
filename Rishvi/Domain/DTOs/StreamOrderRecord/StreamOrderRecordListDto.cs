namespace Rishvi.Domain.DTOs.StreamOrderRecord;

public class StreamOrderRecordListDto
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
    public string Order { get; set; }
}