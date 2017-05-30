
using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class ShipStationShipmentLabel
    {
        public int orderId { get; set; }
        public string carrierCode { get; set; }
        public string serviceCode { get; set; }
        public string packageCode { get; set; }
        public string confirmation { get; set; }
        public DateTime? shipDate { get; set; }
        public ShipStationWeight weight { get; set; }
        public bool testLabel { get; set; }
        public string dimensions { get; set; }
        public string insuranceOptions { get; set; }
        public string internationalOptions { get; set; }
        public string advancedOptions { get; set; }

    }
}
