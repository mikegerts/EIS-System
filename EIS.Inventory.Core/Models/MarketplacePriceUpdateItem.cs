using System;

namespace EIS.Inventory.Core.Models
{
    /// <summary>
    /// This class represents the data contract for updating the item's price on marketplace(s)
    /// </summary>
    public class MarketplacePriceUpdateItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the item
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
    }
}
