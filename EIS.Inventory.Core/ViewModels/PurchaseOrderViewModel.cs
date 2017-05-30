using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class PurchaseOrderViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the purchase order
        /// </summary>
        public string Id { get; set; }

        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor or company name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets the vendor or company address
        /// </summary>
        public string VendorAddress { get; set; }

        public string ContactPerson { get; set; }

        public string VendorEmail { get; set; }

        public string VendorCity { get; set; }

        public string VendorZipCode { get; set; }

        public string PhoneNumber { get; set; }
        public decimal Total { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public DateTime Modified { get; set; }

        public DateTime Created { get; set; }

        public bool IsManual { get; set; }

        public IEnumerable<PurchaseOrderItem> Items { get; set; }
    }

    public class PurchaseOrderItem
    {
        public string Id { get; set; }

        public string PurchaseOrderId { get; set; }
        
        public long EisOrderId { get; set; }

        public string ItemName { get; set; }

        public string SKU { get; set; }

        public int Qty { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal ShippingPrices { get; set; }

        public decimal Taxes { get; set; }

        public decimal Discounts { get; set; }

        public bool IsPaid { get; set; }

        public decimal Total
        {
            get { return UnitPrice * Qty; }
        }
    }
}
