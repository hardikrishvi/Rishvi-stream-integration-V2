namespace LinnworksAPI
{
    public interface IShippingServiceController
    {
        CancelOrderShippingLabelResponse CancelOrderShippingLabel(CancelOrderShippingLabelRequest request);
        void PostShipmentUpload(PostShipmentUploadRequest request);
    }
}