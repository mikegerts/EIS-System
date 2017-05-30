using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    /// <summary>
    /// A object representation for vendor product that are fetched from vendor product file
    /// </summary>
    public class VendorProduct
    {
        public VendorProduct()
        {
            HasInvalidData = false;
        }

        public string EisSupplierSKU { get; set; }
        public string SupplierSKU { get; set; }
        public int VendorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public decimal SupplierPrice { get; set; }
        public int Quantity { get; set; }
        public int MinPack { get; set; }
        public string UPC { get; set; }
        public string Category { get; set; }
        public string Weight { get; set; }
        public string WeightUnit { get; set; }
        public string Shipping { get; set; }
        public int? VendorMOQ { get; set; }
        public string VendorMOQType { get; set; }
        public bool IsAutoLinkToEisSKU { get; set; }

        // helper properties
        public bool IsSupplierPriceSet { get; set; }
        public bool IsQuantitySet { get; set; }
        public bool IsMinPackSet { get; set; }
        public bool IsAutoLinkToEisSKUSet { get; set; }
        public bool HasInvalidData { get; set; }
        public string SubmittedBy { get; set; }
        public List<string> Images { get; set; }
    }
}
