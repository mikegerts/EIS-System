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
    
    public partial class vendorproductlink
    {
        public string EisSKU { get; set; }
        public string EisSupplierSKU { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
    
        public virtual vendorproduct vendorproduct { get; set; }
        public virtual product product { get; set; }
    }
}