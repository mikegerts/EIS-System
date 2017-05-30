using System;
using System.Collections.Generic;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models.Shippings;

namespace EIS.Shipping.Endicia
{
    public class EndiciaShippingProvider : IShippingProvider
    {
        public IList<EisShipmentRate> GetShipmentRate()
        {
            throw new NotImplementedException();
        }

        public void SetRequest(EisRequestedShipment eisRequest)
        {
            throw new NotImplementedException();
        }
    }
}
