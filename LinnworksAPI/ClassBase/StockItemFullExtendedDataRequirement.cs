using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StockItemFullExtendedDataRequirement
    {
        StockLevels,
        Supplier,
        ChannelTitle,
        ChannelDescription,
        ChannelPrice,
        ExtendedProperties,
        Images,
    }
}