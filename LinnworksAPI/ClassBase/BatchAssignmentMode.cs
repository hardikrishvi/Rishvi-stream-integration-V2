using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BatchAssignmentMode
    {
        ALL,
        AUTO_ONLY,
        UNASSIGNED_ONLY,
    }
}