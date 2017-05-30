using System.Collections.Generic;
using System.Threading.Tasks;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Managers
{
    public interface IMarketplaceOrdersManager
    {
        /// <summary>
        /// Get the list of shipping carriers for the specified marketplace
        /// </summary>
        /// <param name="marketplace">The name of marketplace</param>
        /// <returns></returns>
        List<Carrier> GetShippingCarriers(string marketplace);

        /// <summary>
        /// Get the list of orders with the specified order IDs to the marketplace
        /// </summary>
        /// <param name="marketplace">The name of marketplace</param>
        /// <param name="marketplaceOrderIds">The list of order IDs</param>
        /// <returns></returns>
        int ImportMarketplaceOrdersData(string marketplace, List<string> marketplaceOrderIds);

        /// <summary>
        /// Update the marketplace order data with the specified marketplace order id
        /// </summary>
        /// <param name="marketplace">The type of marketplace to where the get the order data</param>
        /// <param name="marketplaceOrderId">The id of the marketplace order</param>
        /// <returns></returns>
        OrderListViewModel UpdateMarketplaceOrderData(string marketplace, string marketplaceOrderId);

        /// <summary>
        /// Confirm the order shipment details
        /// </summary>
        /// <param name="shipmentModel"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task ConfirmOrderShipmentAsync(OrderShipmentViewModel shipmentModel, string userName);

        /// <summary>
        /// Cancel the manual order crearted
        /// </summary>
        /// <param name="orderId">The id of order to cancel</param>
        /// <param name="marketplace">The name of marketplace where the order was placed</param>
        bool CancelOrder(string orderId, string marketplace);

        /// <summary>
        /// Unshipped the manual order
        /// </summary>
        /// <param name="orderId">The order id to unshipped</param>
        /// <param name="marketplace">The name of the marketplace of the order</param>
        /// <returns></returns>
        bool UnshippedOrder(string orderId, string marketplace);
    }
}
