
using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class OrderItemViewModel
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
        /// Get the quantity of item not shipped
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
        public decimal ItemTax { get; set; }

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

        /// <summary>
        /// A helper property for marketplace name
        /// </summary>
        public string Marketplace { get; set; }
        
        public List<OrderProductDto> OrderProducts { get; set; }

        public string MarketPlaceUrl  
        {
            get
            {
                var marketPlaceUrl = "";
                if(Marketplace == "Amazon")
                {
                    marketPlaceUrl = "https://amazon.com/dp/" + MarketplaceItemId;
                }
                else if (Marketplace == "eBay")
                {
                    marketPlaceUrl = "http://www.ebay.com/itm/" + MarketplaceItemId;
                }
                else
                {
                    marketPlaceUrl = "/product/edit/" + SKU;
                }
                return marketPlaceUrl;
            }
        }

     
    }
}
