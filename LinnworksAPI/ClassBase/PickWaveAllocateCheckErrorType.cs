using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LinnworksAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PickWaveAllocateCheckErrorType
    {
        Unknown,
        NeedsStockAllocation,
        IsLocked,
        IsParked,
        IsInFulfilmentCentre,
        IsCancelled,
        IsProcessed,
        NoVaidItems,
        UnlinkedItem,
        AlreadyExistsInWave,
        OrderDoesntExist,
        AllCombinationOfItemsDontExist,
        DuplicateCombinationsOfItemsExist,
        DifferentLocation,
    }
}