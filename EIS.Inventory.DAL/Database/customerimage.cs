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
    
    public partial class customerimage
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public string FileName { get; set; }
        public string Caption { get; set; }
        public int Order_ { get; set; }
    
        public virtual customer customer { get; set; }
    }
}
