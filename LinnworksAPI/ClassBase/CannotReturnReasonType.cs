using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CannotReturnReasonType
    {
        None,
        NotImplemented,
        DisabledInConfig,
        MissingOrderInLinnworks,
        OpenOrderInLinnworks,
        AlreadyReturnedOnChannel,
        AlreadyReturnedInLinnworks,
        NoNewRMAsSpecified,
        SubStatusTagNotInTheSystem,
        SubStatusTagIsNotActionable,
        MultipleSubStatusTags,
        Other,
    }
}