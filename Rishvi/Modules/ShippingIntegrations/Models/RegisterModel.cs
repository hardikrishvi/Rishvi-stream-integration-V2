namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class RegisterModel
    {

    }
    public class RegistrationData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string AuthorizationToken { get; set; }

        public string LinnworksSyncToken { get; set; }

        public LinnworksModel Linnworks { get; set; }
        public StreamModel Stream { get; set; }
        public EbayModel Ebay { get; set; }

        public SyncModel Sync { get; set; }
        public DateTime LastSyncOnDate { get; set; }
        public string LastSyncOn { get; set; }

        public int ebaypage { get; set; }
        public int ebayhour { get; set; }
        public int linnpage { get; set; }
        public int linnhour { get; set; }


    }

    public class LinnworksModel
    {
        public bool DownloadOrderFromStream { get; set; }
        public bool DownloadOrderFromEbay { get; set; }
        public bool PrintLabelFromStream { get; set; }
        public bool PrintLabelFromLinnworks { get; set; }
        public bool DispatchOrderFromStream { get; set; }
        public bool DispatchOrderFromEbay { get; set; }
        public bool SendChangeToEbay { get; set; }
        public bool SendChangeToStream { get; set; }
    }

    public class StreamModel
    {
        public bool GetTrackingDetails { get; set; }
        public bool EnableWebhook { get; set; }
        public bool SendChangeFromLinnworksToStream { get; set; }
        public bool SendChangesFromEbayToStream { get; set; }
        public bool CreateProductToStream { get; set; }
        public bool DownloadProductFromStreamToLinnworks { get; set; }
        public bool GetRoutePlanFromStream { get; set; }
        public bool GetDepotListFromStream { get; set; }
    }

    public class EbayModel
    {
        public bool DownloadOrderFromEbay { get; set; }
        public bool SendOrderToStream { get; set; }
        public bool UpdateInformationFromEbayToStream { get; set; }
        public bool DispatchOrderFromEbay { get; set; }
        public bool UpdateTrackingDetailsFromStream { get; set; }
    }

    public class SyncModel
    {
        public bool SyncEbayOrder { get; set; }
        public bool SyncLinnworksOrder { get; set; }
        public bool CreateEbayOrderToStream { get; set; }
        public bool CreateLinnworksOrderToStream { get; set; }
        public bool DispatchLinnworksOrderFromStream { get; set; }
        public bool DispatchEbayOrderFromStream { get; set; }

        public bool UpdateLinnworksOrderToStream { get; set; }
    }
}
