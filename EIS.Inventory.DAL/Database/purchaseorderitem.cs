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
    
    public partial class purchaseorderitem
    {
        public long Id { get; set; }
        public string PurchaseOrderId { get; set; }
        public int EisOrderId { get; set; }
        public string SKU { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ShippingPrices { get; set; }
        public decimal Taxes { get; set; }
        public decimal Discounts { get; set; }
        public bool IsPaid { get; set; }
        public decimal Profit { get; set; }
    
        public virtual purchaseorder purchaseorder { get; set; }
    }
}
