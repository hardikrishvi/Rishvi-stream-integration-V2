using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CannotRefundReasonType
    {
        None,
        NotImplemented,
        DisabledInConfig,
        MissingOrderInLinnworks,
        OpenOrderInLinnworks,
        OrderIsFullyRefundedInLinnworks,
        NoNewRefundsSpecified,
        NoUpdatedRefundsSpecified,
        MultipleReferences,
        SubStatusTagNotInTheSystem,
        SubStatusTagIsNotActionable,
        MultipleSubStatusTags,
        RefundNotInSystem,
        Other,
    }
}