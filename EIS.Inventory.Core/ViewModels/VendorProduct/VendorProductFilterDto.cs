using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class VendorProductFilterDto
    {
        public VendorProductFilterDto()
        {
            SelectedModelIds = new List<string>();
            ExcludedModelIds = new List<string>();
        }

        public int? VendorId { get; set; }
        public int? CompanyId { get; set; }
        public string SearchString { get; set; }
        public int? QuantityFrom { get; set; }
        public int? QuantityTo { get; set; }
        public int? WithEisSKULink { get; set; }
        public List<string> SelectedModelIds { get; set; }
        public List<string> ExcludedModelIds { get; set; }
        public bool IsAllSelectedItems { get; set; }
        public int withImages { get; set; }
    }
}
