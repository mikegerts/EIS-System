using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisRequestedShipment
    {
        public DateTime ShipTimestamp { get; set; }
        public EisDropoffType DropoffType { get; set; }
        public EisServiceType ServiceType { get; set; }
        public bool ServiceTypeSpecified { get; set; }
        public EisPackagingType PackagingType { get; set; }
        public bool PackagingTypeSpecified { get; set; }
        public EisWeight TotalWeight { get; set; }
        public EisMoney TotalInsuredValue { get; set; }
        public string PreferredCurrency { get; set; }
        public EisParty Shipper { get; set; }
        public EisParty Recipient { get; set; }
        public string RecipientLocationNumber { get; set; }
        public EisParty SoldTo { get; set; }
        public EisPayment ShippingChargesPayment { get; set; }
        public EisShipmentSpecialServicesRequested SpecialServicesRequested { get; set; }
        public string DeliveryInstructions { get; set; }
        public bool BlockInsightVisibility { get; set; }
        public bool BlockInsightVisibilitySpecified { get; set; }
        public EisLabelSpecification LabelSpecification { get; set; }
        public string PackageCount { get; set; }
        public IList<EisRequestedPackageLineItem> RequestedPackageLineItems { get; set; }
    }

    public enum EisDropoffType
    {
        BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
    }

    public enum EisServiceType
    {
        EUROPE_FIRST_INTERNATIONAL_PRIORITY, FEDEX_1_DAY_FREIGHT, FEDEX_2_DAY, FEDEX_2_DAY_AM, FEDEX_2_DAY_FREIGHT, FEDEX_3_DAY_FREIGHT, FEDEX_DISTANCE_DEFERRED,
        FEDEX_EXPRESS_SAVER, FEDEX_FIRST_FREIGHT, FEDEX_FREIGHT_ECONOMY, FEDEX_FREIGHT_PRIORITY, FEDEX_GROUND, FEDEX_NEXT_DAY_AFTERNOON, FEDEX_NEXT_DAY_EARLY_MORNING,
        FEDEX_NEXT_DAY_END_OF_DAY, FEDEX_NEXT_DAY_FREIGHT, FEDEX_NEXT_DAY_MID_MORNING, FIRST_OVERNIGHT, GROUND_HOME_DELIVERY, INTERNATIONAL_ECONOMY, INTERNATIONAL_ECONOMY_FREIGHT,
        INTERNATIONAL_FIRST, INTERNATIONAL_PRIORITY, INTERNATIONAL_PRIORITY_FREIGHT, PRIORITY_OVERNIGHT, SAME_DAY, SAME_DAY_CITY, SMART_POST, STANDARD_OVERNIGHT
    }

    public enum EisPackagingType
    {
        FEDEX_10KG_BOX, FEDEX_25KG_BOX, FEDEX_BOX, FEDEX_ENVELOPE, FEDEX_EXTRA_LARGE_BOX, FEDEX_LARGE_BOX, FEDEX_MEDIUM_BOX, FEDEX_PAK, FEDEX_SMALL_BOX, FEDEX_TUBE, YOUR_PACKAGING
    }
}
