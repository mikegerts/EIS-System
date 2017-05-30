using EIS.Inventory.Shared.Models;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class KitDto
    {
        public KitDto()
        {
            KitDetails = new List<KitDetailDto>();
        }
        public string ParentKitSKU { get; set; }
        public int Quantity { get; set; }
        public bool IsKit { get; set; }
        public InventoryDependency InventoryDependencyOn { get; set; }
        public IEnumerable<KitDetailDto> KitDetails { get; set; }
    }
}
