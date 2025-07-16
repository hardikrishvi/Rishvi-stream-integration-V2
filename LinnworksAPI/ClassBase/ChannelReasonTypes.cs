using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChannelReasonTypes
    {
        None,
        Cancellation,
        ItemRefund,
        ShippingRefund,
        ServiceRefund,
        ItemReturn,
        InsufficientRefund,
        AdditionalRefund,
    }
}