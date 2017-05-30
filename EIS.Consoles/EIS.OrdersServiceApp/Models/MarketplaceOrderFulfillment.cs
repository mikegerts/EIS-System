using System;
using System.Collections.Generic;

namespace EIS.OrdersServiceApp.Models
{
    /// <summary>
    /// Class for updating an order on the marketplace with fullfillment data.
    /// </summary>
    public class MarketplaceOrderFulfillment
    {
        public MarketplaceOrderFulfillment()
        {
            OrderItems = new List<MarketplaceOrderFulfillmentItem>();
        }

        /// <summary>
        /// Gets or sets the identification for what marketplace it came from
        /// </summary>
        public string Marketplace { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the order in the marketplace
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the date the fulfillment was made.
        /// </summary>
        public DateTime FulfillmentDate { get; set; }

        /// <summary>
        /// Gets or sets the code of the carrier
        /// </summary>
        public string CarrierCode { get; set; }

        /// <summary>
        /// Get the carrier object based on the carrier code given
        /// </summary>
        public Carrier Carrier
        {
            get
            {
                return new Carrier { Code = CarrierCode };
            }
        }

        /// <summary>
        /// Gets or sets the shipping service method of the order.
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the tracking number for the shipper.
        /// </summary>
        public string ShipperTrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the items on the order.
        /// </summary>
        public List<MarketplaceOrderFulfillmentItem> OrderItems { get; set; }
    }
}
