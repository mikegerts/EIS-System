using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class ShipStationOrderItemsViewModel
    {
        public int Id { get; set; }
        public Nullable<int> OrderId { get; set; }
        public string LineItemKey { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public ShipStationWeight Weight { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> TaxAmount { get; set; }
        public Nullable<decimal> ShippingAmount { get; set; }
        public string WarehouseLocation { get; set; }
        public string ProductID { get; set; }
        public Nullable<bool> Adjustment { get; set; }
        public string UPC { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public string FulFillmentSKU { get; set; }

        public virtual ShipStationOrdersViewModel shippingstationorder { get; set; }
    }
}
