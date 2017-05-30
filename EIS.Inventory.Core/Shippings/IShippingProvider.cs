using EIS.Inventory.Shared.Models.Shippings;
using System.Collections.Generic;

namespace EIS.Inventory.Core.Shippings
{
    public interface IShippingProvider
    {
        /// <summary>
        /// Get the list of shipment rates from the shipping provider
        /// </summary>
        /// <returns></returns>
        IList<EisShipmentRate> GetShipmentRate();
        void SetRequest(EisRequestedShipment eisRequest);
    }
}
