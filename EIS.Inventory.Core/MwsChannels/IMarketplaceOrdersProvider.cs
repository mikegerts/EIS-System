using System.Collections.Generic;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.MwsChannels
{
    public interface IMarketplaceOrdersProvider
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }
        
        /// <summary>
        /// Gets or sets the MWS credential to the API
        /// </summary>
        CredentialDto MarketplaceCredential { set; }

        /// <summary>
        /// Get the latest order data with the specified order id
        /// </summary>
        /// <param name="orderId">The id of the marketplace order</param>
        /// <returns></returns>
        MarketplaceOrder GetMarketplaceOrder(string orderId);

        /// <summary>
        /// Get the marketplace orders data with the specified list of order IDs
        /// </summary>
        /// <param name="orderIds">The list of order IDs</param>
        /// <returns></returns>
        List<MarketplaceOrder> GetMarketplaceOrders(List<string> orderIds);

        /// <summary>
        /// Confirm the order shipment by adding tracking number
        /// </summary>
        /// <param name="orderFulfillment">An order to be updated for tracking</param>
        /// <param name="submittedBy">The name of the user who does the confirming the order shipment</param>
        /// <returns></returns>
        bool ConfirmOrderShimpmentDetails(MarketplaceOrderFulfillment orderFulfillment, string submittedBy);

        /// <summary>
        /// Get the list of marketplace shipping carries
        /// </summary>
        /// <returns></returns>
        List<Carrier> GetShippingCarriers();

        /// <summary>
        /// Cancel the order
        /// </summary>
        /// <param name="orderId">The id of order to cancel</param>
        bool CancelOrder(string orderId);

        /// <summary>
        /// Unshipped the Manual Order
        /// </summary>
        /// <param name="orderId">The order id to unshipped</param>
        /// <returns></returns>
        bool UnshippedOrder(string orderId);
    }
}
