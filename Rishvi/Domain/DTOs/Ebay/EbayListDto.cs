namespace Rishvi.Domain.DTOs.Ebay;

public class EbayListDto
{
    public Guid Id { get; set; }

    public bool DownloadOrderFromEbay { get; set; }
    public bool SendOrderToStream { get; set; }
    public bool UpdateInformationFromEbayToStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool UpdateTrackingDetailsFromStream { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}