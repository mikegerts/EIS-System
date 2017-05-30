
namespace EIS.OrdersServiceApp.Models
{
    /// <summary>
    /// Class that represents the items for a fullfillment order.
    /// </summary>
    public class MarketplaceOrderFulfillmentItem
    {
        /// <summary>
        /// Gets or sets the quantity of the item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unique id for the item in an order.
        /// </summary>
        public string OrderItemId { get; set; }
    }
}
