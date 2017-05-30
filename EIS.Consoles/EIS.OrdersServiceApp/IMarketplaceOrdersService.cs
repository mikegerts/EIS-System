using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models;

namespace EIS.OrdersServiceApp
{
    public interface IMarketplaceOrdersService
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Set the marketplace credential for this provider
        /// </summary>
        Models.Credential Credential { set; }

        /// <summary>
        /// Fetches the newly created orders from the marketplaces from a given date.
        /// </summary>
        /// <param name="lastUpdated">The starting created order date for when orders are supposed to be pulled.</param>
        /// <returns>A collection of marketplace orders.</returns>
        IEnumerable<MarketplaceOrder> GetMarketplaceOrders(DateTime createdAfter);

        /// <summary>
        /// Confirm the unshipped orders' shipment
        /// </summary>
        /// <returns></returns>
        bool ConfirmOrdersShipment();
    }
}
