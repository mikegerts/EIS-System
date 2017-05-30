
namespace EIS.Inventory.Shared.ViewModels
{
    public class KitDetailDto
    {
        public string ParentKitSKU { get; set; }
        public string ChildKitSKU { get; set; }
        public bool IsMain { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal ProductSupplierPrice { get; set; }
        public decimal ProductSellerPrice { get; set; }
        public int ProductQuantity { get; set; }
    }
}
