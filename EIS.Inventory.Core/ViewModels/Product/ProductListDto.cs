namespace EIS.Inventory.Core.ViewModels
{
    public class ProductListDto
    {
        public string EisSKU { get; set; }
        public string EisSupplierSKU { get; set; }
        public string VendorName { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public decimal? SupplierPrice { get; set; }
        public decimal? SellerPrice { get; set; }
    }
}
