using System.Collections.Generic;

namespace EIS.Inventory.Core.Models
{
    public class ItemContainer
    {
        public ItemContainer(string eisSKU, string itemId)
        {
            EisSKU = eisSKU;
            ItemId = itemId;
        }
        public ItemContainer()
        {
            Fees = new List<Fee>();
        }
        public string EisSKU { get; set; }
        public string ItemId { get; set; }
        public List<Fee> Fees { get; set; }
        public string Message { get; set; }
    }
}
