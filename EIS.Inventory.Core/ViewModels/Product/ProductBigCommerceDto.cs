using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductBigCommerceDto {

        public string EisSKU { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Condition { get; set; }
        public string Categories { get; set; }
        public Nullable<decimal> RetailPrice { get; set; }
        public string PrimaryImage { get; set; }
        public Nullable<decimal> FixedCostShippingPrice { get; set; }
        public string Brand { get; set; }
        public Nullable<int> ProductsType { get; set; }
        public Nullable<int> InventoryLevel { get; set; }
        public Nullable<int> InventoryWarningLevel { get; set; }
        public Nullable<int> InventoryTracking { get; set; }
        public Nullable<int> OrderQuantityMinimum { get; set; }
        public Nullable<int> OrderQuantityMaximum { get; set; }
        public string ModifiedBy { get; set; }
        public ProductDto Product { get; set; }
        public Nullable<int> CategoryId { get; set; }
    }
}
