using System;
using System.Collections.Generic;

namespace LinnworksAPI
{
    public interface IShipStationController
    {
        ShipStationConfig CreateShipStationIntegration(ShipStationConfig integration);
        void DeleteShipStationIntegration(String integrationId);
        Boolean EditShipStationIntegration(ShipStationConfig integration);
        ShipStationConfig GetShipStationIntegration(String integrationId);
        List<ShipStationConfig> GetShipStationIntegrations();
    }
}