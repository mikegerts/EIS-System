using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum PackageType
    {
        Package = 0,

        [Description("Flat Rate Envelope")]
        FlatRateEnvelope = 1,

        [Description("Large Flat Rate Box")]
        LargeFlatRateBox = 2,

        [Description("Medium Flat Rate Box")]
        MediumFlatRateBox = 3,

        [Description("Regional Rate Box A")]
        RegionalRateBoxA= 4,

        [Description("Regional Rate Box B")]
        RegionalRateBoxB = 4,

        [Description("Small Flat Rate Box")]
        SmallFlatRateBox = 5,

        [Description("Thick Envelope")]
        ThickEnvelope = 6
    }
}
