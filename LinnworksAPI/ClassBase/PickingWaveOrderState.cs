using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PickingWaveOrderState
    {
        Unpicked,
        PartialPicked,
        Picked,
        Processed,
        LockedOrParked,
        Cancelled,
        Deleted,
    }
}