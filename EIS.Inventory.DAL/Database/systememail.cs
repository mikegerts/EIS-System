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
    
    public partial class systememail
    {
        public systememail()
        {
            this.messagetemplates = new HashSet<messagetemplate>();
        }
    
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
    
        public virtual ICollection<messagetemplate> messagetemplates { get; set; }
    }
}
