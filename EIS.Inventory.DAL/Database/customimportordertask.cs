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
    
    public partial class customimportordertask : scheduledtask
    {
        public bool HasHeader { get; set; }
        public Nullable<bool> IsDropNoOrderFile { get; set; }
        public string FileType { get; set; }
        public string FileHeaders { get; set; }
        public string CustomFields { get; set; }
        public string FtpServer { get; set; }
        public string FtpUser { get; set; }
        public string FtpPassword { get; set; }
        public Nullable<int> FtpPort { get; set; }
        public string RemoteFolder { get; set; }
        public string ConfirmationEmailTos { get; set; }
        public string FileName { get; set; }
    }
}
