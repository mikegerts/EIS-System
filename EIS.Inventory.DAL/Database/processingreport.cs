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
    
    public partial class processingreport
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string MessageType { get; set; }
        public string StatusCode { get; set; }
        public int MessagesProcessed { get; set; }
        public int MessagesSuccessful { get; set; }
        public int MessagesWithError { get; set; }
        public int MessagesWithWarning { get; set; }
        public string MerchantId { get; set; }
        public Nullable<int> MessageId { get; set; }
        public string SubmittedBy { get; set; }
        public System.DateTime Created { get; set; }
    }
}