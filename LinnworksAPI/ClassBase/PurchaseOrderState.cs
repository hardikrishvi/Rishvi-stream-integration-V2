using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PurchaseOrderState
    {
        Insufficient_PO,
        Sufficient_PO,
    }
}