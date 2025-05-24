namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamDeleteOrderResponse
    {
        public DeleteResponse response { get; set; }
    }

    public class DeleteResponse
    {
        public bool valid { get; set; }
        public List<Error> errors { get; set; }
    }
}
