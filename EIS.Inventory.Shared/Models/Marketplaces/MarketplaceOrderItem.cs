namespace EIS.Inventory.Shared.Models
{
    /// <summary>
    /// Class that represents an item of an order on a marketplace
    /// </summary>
    public class MarketplaceOrderItem
    {
        /// <summary>
        ///  The id of the order which this item belongs to
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// The unique id of the item on the order.
        /// </summary>
        public string OrderItemId { get; set; }

        /// <summary>
        /// The unique id of the item for the marketplace (i.e. ASIN).
        /// </summary>
        public string MarketplaceItemId { get; set; }

        /// <summary>
        /// The unique identifier for the item
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// The title or the description of the item
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The quantity of the item ordered
        /// </summary>
        public int QtyOrdered { get; set; }

        /// <summary>
        /// The quantity of the item that has been shipped.
        /// </summary>
        public int QtyShipped { get; set; }

        /// <summary>
        /// Get the quantity of the item that hasn't been shipped yet.
        /// </summary>
        public int QtyUnshipped { get { return QtyOrdered - QtyShipped; } }

        /// <summary>
        /// The price of the item.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The shipping price of the item.
        /// </summary>
        public decimal ShippingPrice { get; set; }

        /// <summary>
        /// The gift wrap price of the item
        /// </summary>
        public decimal GiftWrapPrice { get; set; }

        /// <summary>
        /// The tax charged for the item.
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// The shipping tax charged for the item.
        /// </summary>
        public decimal ShippingTax { get; set; }

        /// <summary>
        /// The gift wrap tax charged for the item
        /// </summary>
        public decimal GiftWrapTax { get; set; }

        /// <summary>
        /// The shipping discount for this item
        /// </summary>
        public decimal ShippingDiscount { get; set; }

        /// <summary>
        /// The promotion discount for this item
        /// </summary>
        public decimal PromotionDiscount { get; set; }

        /// <summary>
        /// The condition note for this item
        /// </summary>
        public string ConditionNote { get; set; }
    }
}
