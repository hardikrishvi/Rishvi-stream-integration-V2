using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderDetailLevel
    {
        FOLDER,
        NOTES,
        IDENTIFIERS,
        EXTENDEDPROPERTIES,
        BINRACKS,
    }
}