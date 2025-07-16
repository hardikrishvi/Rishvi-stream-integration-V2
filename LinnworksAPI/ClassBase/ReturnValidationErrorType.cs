using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReturnValidationErrorType
    {
        None,
        MissingOrderOnChannel,
        WrongStateOnChannel,
        NegativeOrZeroReturnQuantity,
        OverReturning,
        NeedsConfirmation,
        AlreadyReturnedOnChannel,
        AlreadyReturnedInLinnworks,
        FreeTextReasonNotSupported,
        OnlyFreeTextReasonSupported,
        ReasonTagIncorrect,
        SubReasonTagIncorrect,
        ItemNotFoundInLinnworks,
        ItemNotFoundOnChannel,
        IncorrectOwnership,
        InvalidRefundAmount,
        UnsupportedPaymentMethod,
        ChannelAuthenticationIssue,
        Other,
    }
}