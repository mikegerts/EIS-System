
namespace EIS.Inventory.Core.Models
{
    /// <summary>
    /// Represents the order status
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// This status is available for pre-orders only. 
        /// The order has been placed, payment has not been authorized,
        /// and the release date of the item is in the future. 
        /// The order is not ready for shipment. 
        /// </summary>
        PendingAvailability,

        /// <summary>
        /// The order has been placed but payment has not been authorized. 
        /// The order is not ready for shipment. 
        /// </summary>
        Pending,

        /// <summary>
        /// Payment has been authorized and order is ready for shipment,
        /// but no items in the order have been shipped.
        /// </summary>
        Unshipped,
        
        /// <summary>
        /// One or more (but not all) items in the order have been shipped.
        /// </summary>
        PartiallyShipped,

        /// <summary>
        /// All items in the order have been shipped.
        /// </summary>
        Shipped,

        /// <summary>
        /// All items in the order have been shipped.
        /// The seller has not yet given confirmation to Amazon that the invoice has been shipped to the buyer.
        /// </summary>
        InvoiceUnconfirmed,

        /// <summary>
        /// The order was canceled.
        /// </summary>
        Canceled,

        /// <summary>
        /// The order cannot be fulfilled.
        /// </summary>
        Unfulfillable,
    }
}
