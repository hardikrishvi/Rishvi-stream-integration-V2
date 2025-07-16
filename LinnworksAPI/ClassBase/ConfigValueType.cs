using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConfigValueType
    {
        STRING,
        INT,
        DOUBLE,
        BOOLEAN,
        PASSWORD,
        LIST,
        URL,
        BUTTON,
    }
}