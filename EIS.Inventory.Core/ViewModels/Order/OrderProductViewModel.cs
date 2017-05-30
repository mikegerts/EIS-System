using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels.Order
{
    public class OrderProductViewModel
    {
        public int Id { get; set; }
        public string OrderItemId { get; set; }
        public string EisSupplierSKU { get; set; }
        public int Quantity { get; set; }
        public int Pack { get; set; }
        public bool IsExported { get; set; }
        public bool IsPoGenerated { get; set; }
        public Nullable<System.DateTime> ExportedDate { get; set; }
        public Nullable<System.DateTime> PoGeneratedDate { get; set; }
        public Nullable<System.DateTime> Created { get; set; }


    }
}
