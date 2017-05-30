namespace EIS.Inventory.Shared.ViewModels
{
    public class ProductBigCommerceDto
    {
        public string EisSKU { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Condition { get; set; }
        public string Categories { get; set; }
        public decimal? RetailPrice { get; set; }
        public string PrimaryImage { get; set; }
        public decimal? FixedCostShippingPrice { get; set; }
        public int? Brand { get; set; }
        public int? ProductsType { get; set; }
        public int? InventoryLevel { get; set; }
        public int? InventoryWarningLevel { get; set; }
        public int? InventoryTracking { get; set; }
        public int? OrderQuantityMinimum { get; set; }
        public int? OrderQuantityMaximum { get; set; }
        public string ModifiedBy { get; set; }
        public string ProductURL { get; set; }
        public bool IsEnabled { get; set; }

        public bool HasAnyChanged
        {
            get
            {
                return ProductId != null
                    || CategoryId != null
                    || Price != null
                    || !string.IsNullOrEmpty(Condition)
                    || !string.IsNullOrEmpty(Categories)
                    || RetailPrice != null
                    || !string.IsNullOrEmpty(PrimaryImage)
                    || FixedCostShippingPrice != null
                    || Brand != null
                    || ProductsType != null
                    || InventoryLevel != null
                    || InventoryWarningLevel != null
                    || InventoryTracking != null
                    || OrderQuantityMinimum != null
                    || OrderQuantityMaximum != null
                    || !string.IsNullOrEmpty(ProductURL);
            }
        }
    }
}
