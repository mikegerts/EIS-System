namespace EIS.Inventory.Core.ViewModels
{
    public class VendorProductListDto
    {
        public string EisSupplierSKU { get; set; }        
        public string SupplierSKU { get; set; }        
        public string VendorName { get; set; }
        public string CompanyName { get; set; }        
        public string Name { get; set; }
        public decimal SupplierPrice { get; set; }
        public int Quantity { get; set; }
        public int MinPack { get; set; }
    }
}
