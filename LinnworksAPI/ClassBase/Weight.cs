using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Weight
    {
        none,
        oz,
        lb,
        gm,
        kg,
    }
}