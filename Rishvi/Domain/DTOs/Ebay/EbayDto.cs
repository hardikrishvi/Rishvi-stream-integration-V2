namespace Rishvi.Domain.DTOs.Ebay;

public class EbayDto
{
    public bool DownloadOrderFromEbay { get; set; }
    public bool SendOrderToStream { get; set; }
    public bool UpdateInformationFromEbayToStream { get; set; }
    public bool DispatchOrderFromEbay { get; set; }
    public bool UpdateTrackingDetailsFromStream { get; set; }
}