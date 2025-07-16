using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FulfillmentState
    {
        Unknown,
        Insufficient_Stock,
        Insufficient_To_Fulfill_All,
        Sufficient_Stock,
    }
}