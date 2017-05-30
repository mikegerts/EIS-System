using System;

namespace EIS.Inventory.Shared.Models
{
    public class ShippingRate
    {
        public string PackageType { get; set; }
        public string MailClass { get; set; }
        public string MailService { get; set; }
        public string Zone { get; set; }
        public string Pricing { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
    }

    public class ShippingRateDB
    {
        public int Id { get; set; }
        public int? WeightFrom { get; set; }
        public int? WeightTo { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
    }
}
