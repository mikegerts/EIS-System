using System;
using EIS.Inventory.Shared.Models;

namespace EIS.OrdersServiceApp.Models
{
    /// <summary>
    /// Class represents the history to add or update the product inventory
    /// </summary>
    public class OrderItemDetail
    {
        /// <summary>
        /// Gets or sets the marketplace order id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order item ID
        /// </summary>
        public string OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the EIS SKU
        /// </summary>
        public string EisSKU { get; set; }

        /// <summary>
        /// Gets or sets the quantity ordered or cancelled
        /// </summary>
        public int QtyOrdered { get; set; }

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the date where the order took place
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        public override string ToString()
        {
            return string.Format("OrderItemId: {0} EisSKU: {1} Qty: {2} Status: {3}",
                OrderItemId, EisSKU, QtyOrdered, OrderStatus);
        }
    }
}
