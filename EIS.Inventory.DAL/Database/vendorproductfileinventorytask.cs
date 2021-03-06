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
    
    public partial class vendorproductfileinventorytask : scheduledtask
    {
        public int VendorId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public bool HasHeader { get; set; }
        public bool IsDeleteFile { get; set; }
        public string FtpServer { get; set; }
        public string FtpUser { get; set; }
        public string FtpPassword { get; set; }
        public Nullable<int> FtpPort { get; set; }
        public string RemoteFolder { get; set; }
        public bool IsZeroOutQty { get; set; }
        public bool IsAddNewItem { get; set; }
        public bool IsLinkEisSKU { get; set; }
        public bool IsCreateEisSKUAndLink { get; set; }
        public Nullable<int> SKU { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> SupplierPrice { get; set; }
        public Nullable<int> ProductName { get; set; }
        public Nullable<int> Description { get; set; }
        public Nullable<int> Category { get; set; }
        public Nullable<int> UPC { get; set; }
        public Nullable<int> MinPack { get; set; }
    
        public virtual vendor vendor { get; set; }
    }
}
