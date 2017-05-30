using System.Collections.Generic;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Managers
{
    public interface IShippingRateManager
    {
        /// <summary>
        /// Calculate and get the shipment rates from the specified provider name and its shipment details
        /// </summary>
        /// <param name="providerName">The shipping provider name</param>
        /// <param name="shipment">The shipment details</param>
        /// <returns></returns>
        List<ShippingRate> GetShipmentRates(string providerName, Shipment shipment);
    }
}
