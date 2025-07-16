namespace LinnworksAPI
{
    public interface IPostSaleController
    {
        ValidatedCancellation CreateCancellation(CancellationRequest request);
    }
}