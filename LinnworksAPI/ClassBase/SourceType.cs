using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SourceType
    {
        netcore10,
        netcore21,
        netcore31,
    }
}