namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class CreateManifestRequest : BaseClasses.BaseRequest
    {
        public List<string> OrderId = new List<string>();
    }

    public class PrintManifestRequest : BaseClasses.BaseRequest
    {
        public string ManifestReference;
    }
}
