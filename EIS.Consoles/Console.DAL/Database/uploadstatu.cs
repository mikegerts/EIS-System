//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EIS.Console.DAL.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class uploadstatu
    {
        public long Id { get; set; }
        public int VendorId { get; set; }
        public System.DateTime StartUploadDate { get; set; }
        public Nullable<System.DateTime> EndUploadDate { get; set; }
        public bool Status { get; set; }
        public Nullable<int> Attempt { get; set; }
    
        public virtual vendor vendor { get; set; }
    }
}
