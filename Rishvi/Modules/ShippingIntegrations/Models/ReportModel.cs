namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class ReportModel
    {
        public string _id { get; set; }
        public string AuthorizationToken { get; set; }
        public string email { get; set; }

        public string LinnNumOrderId { get; set; }
        public string EbayChannelOrderRef { get; set; }

        public string OrderLineItemId { get; set; }
        public string StreamOrderId { get; set; }
        public string StreamConsignmentId { get; set; }

        public string StreamTrackingNumber { get; set; }

        public string StreamTrackingURL { get; set; }

        public DateTime createdDate { get; set; }
        public DateTime updatedDate { get; set; }

        public DateTime DownloadLinnOrderInSystem { get; set; }
        public DateTime DownloadEbayOrderInSystem { get; set; }
        public DateTime DispatchEbayOrderInStream { get; set; }
        public DateTime DispatchEbayOrderFromStream { get; set; }
        public DateTime CreateLinnOrderInStream { get;set; }
        public DateTime LastUpdateLinnOrderForStream { get; set; }

        public DateTime DispatchLinnOrderFromStream { get; set; }

        public DateTime DispatchLinnOrderInStream { get; set; }
        public DateTime CreateEbayOrderInStream { get; set; }


        public bool IsLinnOrderCreatedInStream { get; set; }
        public bool IsEbayOrderCreatedInStream { get; set; }

        public bool IsLinnOrderDispatchInStream { get; set; }
        public bool IsEbayOrderDispatchInStream { get; set; }

        public bool IsLinnOrderDispatchFromStream { get; set; }
        public bool IsEbayOrderDispatchFromStream { get; set; }

        public string EbayOrderDetaailJson { get; set; }
        public string LinnOrderDetailsJson { get; set; }
        public string StreamOrderCreateJson { get; set; }
        public string DispatchOrderInEbayJson { get; set; }
        public string DispatchOrderInLinnJson { get; set; }
    }
}
