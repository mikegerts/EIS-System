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
    
    public partial class orderupdatehistory
    {
        public long Id { get; set; }
        public string OrderItemId { get; set; }
        public int QtyOrdered { get; set; }
        public EIS.Inventory.Shared.Models.OrderStatus OrderStatus { get; set; }
        public System.DateTime PurchaseDate { get; set; }
        public System.DateTime ResultDate { get; set; }
    }
}
