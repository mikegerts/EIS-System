using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class OrderProductDetailDto
    {
        public int EisOrderId { get; set; }
        public string OrderId { get; set; }
        public string Store { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string ShippingAddressPhone { get; set; }
        public string ShippingAddressName { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingStateOrRegion { get; set; }
        public string ShippingPostalCode { get; set; }
        public string OrderNote { get; set; }
        public List<OrderProductDto> OrderProducts { get; set; }
    }
}
