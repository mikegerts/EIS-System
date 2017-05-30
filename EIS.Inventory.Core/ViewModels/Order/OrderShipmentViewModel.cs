using System;
using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class OrderShipmentViewModel
    {
        public string Marketplace { get; set; }
        
        public string OrderId { get; set; }

        public IEnumerable<OrderItemViewModel> OrderItems { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public string CarrierCode { get; set; }

        public string ShippingMethod { get; set; }

        public string ShipperTrackingNumber { get; set; }

        public bool? IsSucceed { get; set; }

        public string SubmittedBy { get; set; }

        public DateTime? Modified { get; set; }
    }
}
