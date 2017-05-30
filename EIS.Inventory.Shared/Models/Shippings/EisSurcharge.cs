﻿namespace EIS.Inventory.Shared.Models.Shippings
{
    public partial class EisSurcharge
    {
        public EisSurchargeType SurchargeType { get; set; }
        public bool SurchargeTypeSpecified { get; set; }
        public string Level { get; set; }
        public bool LevelSpecified { get; set; }
        public string Description { get; set; }
        public EisMoney Amount { get; set; }
    }

    public enum EisSurchargeType
    {
        ADDITIONAL_HANDLING, ANCILLARY_FEE, APPOINTMENT_DELIVERY, BROKER_SELECT_OPTION, CANADIAN_DESTINATION, CLEARANCE_ENTRY_FEE,
        COD, CUT_FLOWERS, DANGEROUS_GOODS, DELIVERY_AREA, DELIVERY_CONFIRMATION, DELIVERY_ON_INVOICE_ACCEPTANCE, DOCUMENTATION_FEE,
        DRY_ICE, EMAIL_LABEL, EUROPE_FIRST, EXCESS_VALUE, EXHIBITION, EXPORT, EXTRA_SURFACE_HANDLING_CHARGE, EXTREME_LENGTH, FEDEX_INTRACOUNTRY_FEES,
        FEDEX_TAG, FICE, FLATBED, FREIGHT_GUARANTEE, FREIGHT_ON_VALUE, FREIGHT_TO_COLLECT, FUEL, HOLD_AT_LOCATION, HOME_DELIVERY_APPOINTMENT,
        HOME_DELIVERY_DATE_CERTAIN, HOME_DELIVERY_EVENING, INSIDE_DELIVERY, INSIDE_PICKUP, INSURED_VALUE, INTERHAWAII, LIFTGATE_DELIVERY, LIFTGATE_PICKUP,
        LIMITED_ACCESS_DELIVERY, LIMITED_ACCESS_PICKUP, METRO_DELIVERY, METRO_PICKUP, NON_MACHINABLE, OFFSHORE, ON_CALL_PICKUP, OTHER, OUT_OF_DELIVERY_AREA,
        OUT_OF_PICKUP_AREA, OVERSIZE, OVER_DIMENSION, PIECE_COUNT_VERIFICATION, PRE_DELIVERY_NOTIFICATION, PRIORITY_ALERT, PROTECTION_FROM_FREEZING,
        REGIONAL_MALL_DELIVERY, REGIONAL_MALL_PICKUP, REROUTE, RESCHEDULE, RESIDENTIAL_DELIVERY, RESIDENTIAL_PICKUP, RETURN_LABEL, SATURDAY_DELIVERY,
        SATURDAY_PICKUP, SIGNATURE_OPTION, TARP, THIRD_PARTY_CONSIGNEE, TRANSMART_SERVICE_FEE,
    }
}