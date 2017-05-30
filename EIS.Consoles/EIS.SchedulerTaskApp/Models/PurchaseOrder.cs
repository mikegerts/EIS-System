using System.Collections.Generic;
using System.Linq;

namespace EIS.SchedulerTaskApp.Models
{
    public class PurchaseOrder
    {
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// Gets or sets the id of the vendor
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor or company name
        /// </summary>
        public string VendorName { get; set; }

        public string ContactPerson { get; set; }
        
        public string ContactEmail { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string PhoneNumber { get; set; }

        public List<PurchaseOrderItem> Items { get; set; }

        /// <summary>
        /// Get the total discounts of the order items
        /// </summary>
        public decimal Discounts
        {
            get { return Items.Sum(x => x.Discounts); }
        }

        /// <summary>
        /// Get the subtotal of all items
        /// </summary>
        public decimal Subtotal
        {
            get { return Items.Sum(x => x.Total); }
        }

        /// <summary>
        /// Get the total of items' taxes
        /// </summary>
        public decimal Taxes
        {
            get { return Items.Sum(x => x.Taxes); }
        }

        /// <summary>
        /// Get the total shipping price for the items' shipping cost
        /// </summary>
        public decimal ShippingPrices
        {
            get { return Items.Sum(x => x.ShippingPrices); }
        }

        /// <summary>
        /// Get the grand total of the purchase order
        /// </summary>
        public decimal GrandTotal
        {
            get
            {
                var subTotal = Items.Sum(x => x.Total);
                var discounts = Items.Sum(x => x.Discounts);
                var shipping = Items.Sum(x => x.ShippingPrices);

                return subTotal + shipping - discounts;
            }
        }
    }
}
