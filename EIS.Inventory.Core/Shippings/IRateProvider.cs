using System.Collections.Generic;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Shippings
{
    public interface IRateProvider
    {
        /// <summary>
        /// Get the name of the rates provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Get the list of shipment rates from the shipping provider
        /// </summary>
        /// <returns></returns>
        List<ShippingRate> GetShipmentRates(Shipment shipment);        
    }
}
