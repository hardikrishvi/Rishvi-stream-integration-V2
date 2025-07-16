namespace LinnworksAPI
{
    public interface IWmsController
    {
        AddWarehouseZoneResponse AddWarehouseZone(AddWarehouseZoneRequest request);
        AddWarehouseZoneTypeResponse AddWarehouseZoneType(AddWarehouseZoneTypeRequest request);
        DeleteWarehouseZoneResponse DeleteWarehouseZone(DeleteWarehouseZoneRequest request);
        DeleteWarehouseZoneTypeResponse DeleteWarehouseZoneType(DeleteWarehouseZoneTypeRequest request);
        GetBinrackZonesByBinrackIdOrNameResponse GetBinrackZonesByBinrackIdOrName(GetBinrackZonesByBinrackIdOrNameRequest request);
        GetBinrackZonesByZoneIdOrNameResponse GetBinrackZonesByZoneIdOrName(GetBinrackZonesByZoneIdOrNameRequest request);
        GetWarehouseZonesByLocationResponse GetWarehouseZonesByLocation(GetWarehouseZonesByLocationRequest request);
        GetWarehouseZoneTypesResponse GetWarehouseZoneTypes(GetWarehouseZoneTypesRequest request);
        UpdateWarehouseBinrackBinrackToZoneResponse UpdateWarehouseBinrackBinrackToZone(UpdateWarehouseBinrackBinrackToZoneRequest request);
        UpdateWarehouseZoneResponse UpdateWarehouseZone(UpdateWarehouseZoneRequest request);
        UpdateWarehouseZoneTypeResponse UpdateWarehouseZoneType(UpdateWarehouseZoneTypeRequest request);
    }
}