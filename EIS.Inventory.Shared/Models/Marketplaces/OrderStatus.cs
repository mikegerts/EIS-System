
namespace EIS.Inventory.Shared.Models
{
    /// <summary>
    /// Represents the order status
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Default value for status - custom
        /// </summary>
        None = 0,

        /// <summary>
        /// The order has been placed but payment has not been authorized. 
        /// The order is not ready for shipment. 
        /// </summary>
        Pending = 1,

        /// <summary>
        /// This status is available for pre-orders only. 
        /// The order has been placed, payment has not been authorized,
        /// and the release date of the item is in the future. 
        /// The order is not ready for shipment. 
        /// </summary>
        PendingAvailability = 2,

        /// <summary>
        /// Payment has been authorized and order is ready for shipment,
        /// but no items in the order have been shipped.
        /// </summary>
        Unshipped = 3,

        /// <summary>
        /// One or more (but not all) items in the order have been shipped.
        /// </summary>
        PartiallyShipped = 4,

        /// <summary>
        /// All items in the order have been shipped.
        /// </summary>
        Shipped = 5,

        /// <summary>
        /// All items in the order have been shipped.
        /// The seller has not yet given confirmation to Amazon that the invoice has been shipped to the buyer.
        /// </summary>
        InvoiceUnconfirmed = 6,

        /// <summary>
        /// The order was canceled.
        /// </summary>
        Canceled = 7,

        /// <summary>
        /// The order cannot be fulfilled.
        /// </summary>
        Unfulfillable = 8,
    }
}
