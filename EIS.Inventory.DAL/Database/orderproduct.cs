//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EIS.Inventory.DAL.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class orderproduct
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
    
        public virtual orderitem orderitem { get; set; }
        public virtual vendorproduct vendorproduct { get; set; }
    }
}