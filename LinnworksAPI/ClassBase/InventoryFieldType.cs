using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InventoryFieldType
    {
        Int,
        Double,
        String,
        Boolean,
        Select,
        Date,
        Channel,
        Other,
        Datetime2,
        Calculated,
    }
}