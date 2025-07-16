namespace LinnworksAPI
{
    public interface IListingsController
    {
        void EndListingsPendingRelist(EndListingsPendingRelistRequest request);
        GetEbayListingAuditResponse GetEbayListingAudit(GetEbayListingAuditRequest request);
        void SetListingStrikeOffState(SetListingStrikeOffStateRequest request);
    }
}