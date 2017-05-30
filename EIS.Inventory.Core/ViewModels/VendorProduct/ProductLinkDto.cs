using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductLinkDto
    {
        public string EisSKU { get; set; }
        public string EisSupplierSKU { get; set; }
        public string Name { get; set; }
        public decimal SellingPrice { get; set; }
        public string SkuType { get; set; }
    }
}
