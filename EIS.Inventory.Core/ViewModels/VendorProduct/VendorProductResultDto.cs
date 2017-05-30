using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.ViewModels
{
    public class VendorProductResultDto
    {
        public string EisSupplierSKU { get; set; }
        public string Name { get; set; }
        public int MinPack { get; set; }
        public string DisplayName
        {
            get { return string.Format("{0} | {1} | {2}", MinPack, EisSupplierSKU, Name.Truncate(50)); }
        }
    }
}
