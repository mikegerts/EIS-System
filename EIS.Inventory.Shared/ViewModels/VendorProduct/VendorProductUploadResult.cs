using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class VendorProductUploadResult
    {
        public VendorProductUploadResult()
        {
            NewCreatedEisSKUs = new Dictionary<string, string>();
            UpdatedEisSupplierSKUs = new List<string>();
            DeletedEisSupplierSKULinks = new List<string>();
            NoChangedEisSupplierSKUs = new List<string>();
        }
        public Dictionary<string, string> NewCreatedEisSKUs { get; set; }
        public List<string> UpdatedEisSupplierSKUs { get; set; }
        public List<string> DeletedEisSupplierSKULinks { get; set; }
        public List<string> NoChangedEisSupplierSKUs { get; set; }

    }
}
