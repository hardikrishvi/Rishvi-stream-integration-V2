using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RefundUnitType
    {
        Item,
        Shipping,
        Service,
        Additional,
    }
}