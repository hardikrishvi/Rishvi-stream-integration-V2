using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PostSaleStatusType
    {
        OPEN,
        PROCESSED,
        ERROR,
        ERROR_ACKED,
        PENDING,
    }
}