using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum ServiceType
    {
        [Description("USPS First Class Mail")]
        USPS_FirstClassMail = 1,

        [Description("USPS Priority Mail")]
        USPX_PriorityMail = 2,


        [Description("FedEx Ground®")]
        FedEx_Ground = 6,

        [Description("FedEx Home Delivery®")]
        FedEx_HomeDelivery = 7,

        [Description("FedEx SmartPost Parcel")]
        FedEx_SmartPostParcel = 8,
    }
}
