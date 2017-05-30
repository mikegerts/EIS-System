using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models
{
    public class MarketplaceInventoryFeed
    {
        public string EisSKU { get; set; }
        public bool IsBlacklisted { get; set; }
        public AmazonInventoryFeed AmazonInventoryFeed { get; set; }
        public eBayInventoryFeed eBayInventoryFeed { get; set; }
        public BigCommerceInventoryFeed BigCommerceInventoryFeed { get; set; }
    }

    public class AmazonInventoryFeed
    {
        /// <summary>
        /// The unique identifier for the item
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// The quantity of the product
        /// </summary>
        public int ProductQuantity { get; set; }

        public bool IsAlwaysInStock { get; set; }
        public int AlwaysQuantity { get; set; }

        /// <summary>
        /// The FulfillmentLatency - The number of days between the order date 
        /// and the ship date (a whole number between 1 and 30).
        /// </summary>
        public int? LeadtimeShip { get; set; }

        /// <summary>
        /// Gets or sets the vendor's Safe Qty
        /// </summary>
        public int? SafetyQty { get; set; }

        public bool IsEnabled { get; set; }

        /// <summary>
        /// Get the inventory quantity for the product
        /// </summary>
        public int InventoryQuantity
        {
            get
            {
                if (IsAlwaysInStock)
                    return AlwaysQuantity;

                var qty = ProductQuantity - (SafetyQty ?? 0);
                return qty > 0 ? qty : 0;
            }
        }
    }

    public class eBayInventoryFeed
    {
        public string EisSKU { get; set; }
        public string ItemId { get; set; }
        public int ProductQuantity { get; set; }
        public int ListingQuantity { get; set; }
        public int? SafetyQty { get; set; }
        public decimal StartPrice { get; set; }
        public decimal BinPrice { get; set; }
        public decimal ReservePrice { get; set; }
        public bool IsAlwaysInStock { get; set; }
        public int AlwaysQuantity { get; set; }
        public int InventoryQuantity
        {
            get
            {
                if (IsAlwaysInStock)
                    return AlwaysQuantity;

                var remainingQty = ProductQuantity - (SafetyQty ?? 0);
                remainingQty = remainingQty < 0 ? 0 : remainingQty;
                return remainingQty > ListingQuantity ? ListingQuantity : remainingQty;
            }
        }
    }

    public class BigCommerceInventoryFeed
    {
        public string SKU { get; set; }
        public int? ProductId { get; set; }
        public int? InventoryLevel { get; set; }
        public int? InventoryWarningLevel { get; set; }
        public int? InventoryTracking { get; set; }
        public int? OrderQuantityMinimum { get; set; }
        public int? OrderQuantityMaximum { get; set; }
        public int ProductQuantity { get; set; }
        public bool IsEnabled { get; set; }
    }
}
