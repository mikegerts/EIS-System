using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class ShipStationShipmentViewModel
    {

        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ShipDate { get; set; }
        public decimal ShipmentCost { get; set; }
        public decimal InsuranceCost { get; set; }
        public string TrackingNumber { get; set; }
        public bool? IsReturnLabel { get; set; }
        public string BatchNumber { get; set; }
        public string CarrierCode { get; set; }
        public string ServiceCode { get; set; }
        public string PackageCode { get; set; }
        public string Confirmation { get; set; }
        public int WarehouseId { get; set; }
        public bool? Voided { get; set; }
        public DateTime? VoidDate { get; set; }
        public bool? MarketplaceNotified { get; set; }
        public string nNotifyErrorMessage { get; set; }
        public ShipStationAddressViewModel ShipTo { get; set; }
        public ShipStationWeight Weight { get; set; }
    }
}
