using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PickingWaveState
    {
        Unallocated,
        Allocated,
        InProgress,
        Paused,
        Complete,
        Abandoned,
        Packing,
        Shipped,
    }
}