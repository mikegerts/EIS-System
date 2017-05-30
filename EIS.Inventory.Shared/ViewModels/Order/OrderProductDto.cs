using System;

namespace EIS.Inventory.Shared.ViewModels
{
    public class OrderProductDto
    {
        public int Id { get; set; }
        public string OrderItemId { get; set; }
        public string EisSupplierSKU { get; set; }
        public string VendorProductName { get; set; }
        public int Quantity { get; set; }
        public int Pack { get; set; }
        public bool IsExported { get; set; }
        public bool IsPoGenerated { get; set; }
        public Nullable<System.DateTime> ExportedDate { get; set; }
        public Nullable<System.DateTime> PoGeneratedDate { get; set; }
    }
}
