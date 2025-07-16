using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PickingWaveItemState
    {
        Normal,
        OrderStateInvalid,
        Abandonned,
        DeletedFromOrder,
    }
}