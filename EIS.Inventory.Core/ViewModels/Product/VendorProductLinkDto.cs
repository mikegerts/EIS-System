namespace EIS.Inventory.Core.ViewModels
{
    public class VendorProductLinkDto
    {
        public string EisSKU { get; set; }
        public string EisSupplierSKU { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int MinPack { get; set; }
        public decimal SupplierPrice { get; set; }
        public string Status { get; set; }
    }
}
