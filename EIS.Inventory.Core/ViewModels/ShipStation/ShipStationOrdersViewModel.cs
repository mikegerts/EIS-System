using System;
using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class ShipStationOrdersViewModel
    {
        public int Id { get; set; }
        public int EisOrderId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderKey { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public Nullable<System.DateTime> ShipByDate { get; set; }
        public string OrderStatus { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public Nullable<decimal> AmountPaid { get; set; }
        public Nullable<decimal> TaxAmount { get; set; }
        public Nullable<decimal> ShippingAmount { get; set; }
        public string PaymentMethod { get; set; }
        public Nullable<System.DateTime> ShipDate { get; set; }
        public Nullable<System.DateTime> SentDate { get; set; }
        public ShipStationWeight Weight { get; set; }

        public string CarrierCode { get; set; }
        public string ServiceCode { get; set; }
        public string PackageCode { get; set; }
        public string Confirmation { get; set; } 

        public virtual ICollection<ShipStationOrderItemsViewModel> Items { get; set; }
        public virtual ShipStationAddressViewModel BillTo { get; set; }
        public virtual ShipStationAddressViewModel ShipTo { get; set; }

    }
}
