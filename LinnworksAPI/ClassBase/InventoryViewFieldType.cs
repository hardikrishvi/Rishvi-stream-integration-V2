using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InventoryViewFieldType
    {
        Int,
        Double,
        String,
        Boolean,
        Select,
        Date,
        Datetime2,
    }
}