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
    
    public partial class orderitem
    {
        public orderitem()
        {
            this.orderproducts = new HashSet<orderproduct>();
        }
    
        public string OrderItemId { get; set; }
        public string OrderId { get; set; }
        public string ItemId { get; set; }
        public string SKU { get; set; }
        public string Title { get; set; }
        public int QtyOrdered { get; set; }
        public Nullable<int> QtyShipped { get; set; }
        public decimal Price { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal GiftWrapPrice { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ShippingTax { get; set; }
        public decimal GiftWrapTax { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal PromotionDiscount { get; set; }
        public string ConditionNote { get; set; }
    
        public virtual order order { get; set; }
        public virtual ICollection<orderproduct> orderproducts { get; set; }
    }
}
