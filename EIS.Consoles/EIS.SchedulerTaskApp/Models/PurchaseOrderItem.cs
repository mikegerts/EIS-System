using System;

namespace EIS.SchedulerTaskApp.Models
{
    public class PurchaseOrderItem
    {
        public string Id { get; set; }
        
        public int EisOrderId { get; set; }

        public string ItemName { get; set; }

        public string SupplierSKU { get; set; }

        public string EisSupplierSKU { get; set; }

        public int SupplierQtyOrdered { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal ShippingPrices { get; set; }

        public decimal Taxes { get; set; }

        public decimal Discounts { get; set; }

        public bool IsPaid { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal Total
        {
            get { return UnitPrice * SupplierQtyOrdered; }
        }

        public decimal VendorFees
        {
            get { return Total + Profit; }
        }

        public decimal Profit { get; set; }
    }
}
