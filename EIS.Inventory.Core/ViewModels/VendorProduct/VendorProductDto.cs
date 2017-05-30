using System.ComponentModel.DataAnnotations;

namespace EIS.Inventory.Core.ViewModels
{
    public class VendorProductDto
    {
        public string EisSupplierSKU { get; set; }        
        [Required]
        public string SupplierSKU { get; set; }
        [Required]
        public int VendorId { get; set; }              
        public string VendorName { get; set; }
        public string CompanyName { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal SupplierPrice { get; set; }
        public int Quantity { get; set; }
        [Range(1, int.MaxValue)]
        public int MinPack { get; set; }
        public string UPC { get; set; }
        public string Category { get; set; }
        public string Weight { get; set; }
        public string WeightUnit { get; set; }
        public string Shipping { get; set; }
        public int? VendorMOQ { get; set; }
        public string VendorMOQType { get; set; }
        public bool IsAutoLinkToEisSKU { get; set; }
        public bool IsCreateEisSKUAndLink { get; set; }
        public string ModifiedBy { get; set; }
    }
}
