
namespace EIS.Inventory.Core.Models
{
    /// <summary>
    /// This class represents the data contract for updating the inventory quantities on marketplace(s)
    /// </summary>
    public class MarketplaceInventoryUpdateItem
    {
        /// <summary>
        /// The unique identifier for the item
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// The quantity of the item
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The FulfillmentLatency - The number of days between the order date 
        /// and the ship date (a whole number between 1 and 30).
        /// </summary>
        public int LeadtimeShip { get; set; }

        /// <summary>
        /// Gets or sets the vendor's Safe Qty
        /// </summary>
        public int? SafeQty { get; set; }

        /// <summary>
        /// Get the inventory quantity for the product
        /// </summary>
        public int InventoryQuantity
        {
            get
            {
                var qty = Quantity - (SafeQty ?? 0);
                return qty > 0 ? qty : 0;
            }
        }
    }
}
