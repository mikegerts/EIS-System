using EIS.Inventory.Shared.Models;
using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class MarketplacePriceFeedDto
    {
        public string EisSKU { get; set; }
        public bool IsBlacklisted { get; set; }
        public AmazonPriceFeed AmazonPriceFeed { get; set; }
        public eBayInventoryFeed eBayInventoryFeed { get; set; }
        public BigCommerceProductFeed BigCommerceProductFeed { get; set; }
    }

    public class AmazonPriceFeed
    {
        /// <summary>
        /// The EIS SKU
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// Price of the item (non-sale price)
        /// </summary>
        public decimal StandardPrice { get; set; }

        /// <summary>
        /// Minimum Advertised Price. Use only if dictated by the manufacturer.
        /// Both the standard and sale price (if applicable) must be higher than the MAP value.
        /// </summary>
        public decimal MapPrice { get; set; }

        /// <summary>
        /// Gets or sets the sale price for the item
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// Gets or sets the Sale start date of the item
        /// </summary>
        public DateTime SaleStartDate { get; set; }

        /// <summary>
        /// Gets or sets the Sale end date of the item
        /// </summary>
        public DateTime SaleEndDate { get; set; }

        /// <summary>
        /// Is Price or Inventory for Amazon enabled
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
