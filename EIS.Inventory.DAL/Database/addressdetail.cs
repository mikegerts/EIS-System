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
    
    public partial class addressdetail
    {
        public addressdetail()
        {
            this.shippinglocations = new HashSet<shippinglocation>();
            this.shippinglocations1 = new HashSet<shippinglocation>();
        }
    
        public int Id { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string StateOrRegion { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public bool IsResidential { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
    
        public virtual ICollection<shippinglocation> shippinglocations { get; set; }
        public virtual ICollection<shippinglocation> shippinglocations1 { get; set; }
    }
}
