namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisCompletedShipmentDetail
    {
        public bool UsDomestic { get; set; }

        public bool UsDomesticSpecified { get; set; }

        //        private CarrierCodeType carrierCodeField;

        public bool CarrierCodeSpecified { get; set; }

        //        private TrackingId masterTrackingIdField;

        public string ServiceTypeDescription { get; set; }

        public string PackagingDescription { get; set; }

        //        private ShipmentOperationalDetail operationalDetailField;

        //        private PendingShipmentAccessorDetail[] accessDetailField;

        //        private CompletedTagDetail tagDetailField;

        //        private CompletedSmartPostDetail smartPostDetailField;

        //        private CompletedHazardousShipmentDetail hazardousShipmentDetailField;

        //        private ShipmentRating shipmentRatingField;

        //        private CompletedHoldAtLocationDetail completedHoldAtLocationDetailField;

        public string ExportComplianceStatement { get; set; }

        //        private CompletedEtdDetail completedEtdDetailField;

        //        private ShippingDocument[] shipmentDocumentsField;

        //        private AssociatedShipmentDetail[] associatedShipmentsField;

        //        private CompletedCodDetail completedCodDetailField;

        //        private CompletedPackageDetail[] completedPackageDetailsField;
    }
}