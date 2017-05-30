
namespace EIS.Inventory.Shared.ViewModels
{
    public class ProductSearchDto
    {
        public string EisSKU { get; set; }
        public string ASIN { get; set; }
        public string EisSupplierSKU { get; set; }
        public string VendorName { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
    }
}
