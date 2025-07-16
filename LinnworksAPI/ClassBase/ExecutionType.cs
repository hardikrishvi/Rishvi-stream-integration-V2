using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionType
    {
        API,
        Scheduled,
        RulesEngine_Order,
    }
}