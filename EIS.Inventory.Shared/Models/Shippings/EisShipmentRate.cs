using System.Collections.Generic;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisShipmentRate
    {
        public EisShipmentRate()
        {
            Surcharges = new List<EisSurcharge>();
        }
        public EisWeight TotalBillingWeight { get; set; }
        public EisMoney TotalBaseCharge { get; set; }
        public EisMoney TotalFreightDiscounts { get; set; }
        public EisMoney TotalSurcharges { get; set; }
        public IList<EisSurcharge> Surcharges { get; set; }
        public EisMoney TotalNetCharge { get; set; }
    }
}
