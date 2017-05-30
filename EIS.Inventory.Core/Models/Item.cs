
namespace EIS.Inventory.Core.Models
{
    public class Item
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int QtyAvailable { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal SupplierPrice { get; set; }
    }
}
