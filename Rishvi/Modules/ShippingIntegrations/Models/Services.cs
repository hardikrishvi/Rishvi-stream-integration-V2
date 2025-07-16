namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public static class Services
    {
        //private static Guid _serviceUniqueId = Guid.Empty;
        //public static void Initialize(IOptions<CourierSettings> serviceUniqueId) => _serviceUniqueId = serviceUniqueId.Value.SelectedServiceId;
        public static List<Classes.CourierService> GetServices
        {
            get
            {
                //if (_serviceUniqueId == Guid.Empty)
                //    throw new InvalidOperationException("Services class not initialized with ServiceUniqueId.");

                return new List<Classes.CourierService>() {
                    new Classes.CourierService() {
                        ServiceCode="STREAM",
                        ServiceGroup="Generic",
                        ServiceName="Stream Generic Service",
                        ServiceTag = "STREAM",
                        ServiceUniqueId = CourierSettings.SelectedServiceId,
                        //ServiceUniqueId = Guid.Parse("6A476315-04DB-4D25-A25C-E6917A1BCAD9"),
                        ServiceProperty = new List<Classes.ServiceProperty>(),
                        ConfigItems = new List<Classes.ConfigItem>()
                    }
                };
            }
        }
    }
}
