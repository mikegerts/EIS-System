using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.DAL.Database;

namespace EIS.Inventory.Core.Services
{
    public interface IOrderService : IDisposable
    {
        /// <summary>
        /// Get the paginated order list
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The number of items in the page</param>
        /// <param name="searchString">The keywords to search</param>
        /// <param name="orderDateFrom">The order date from</param>
        /// <param name="orderDateTo">The order date to</param>
        /// <param name="shipmentDateFrom">The shipment date from</param>
        /// <param name="shipmentDateTo">The shipment date to</param>
        /// <param name="orderStatus">The status of the order</param>
        /// <param name="vendorId">The id of the vendor</param>
        /// <param name="isExported">Boolean to determine whether the order is exported or not</param>
        /// <returns></returns>
        IPagedList<OrderListViewModel> GetPagedOrders(int page,
            int pageSize,
            string searchString,
            string shippingAddress,
            string shippingCity,
            string shippingCountry,
            DateTime? orderDateFrom,
            DateTime? orderDateTo,
            DateTime? shipmentDateFrom,
            DateTime? shipmentDateTo,
            OrderStatus orderStatus,
            //int vendorId,
            int isExported,
            string marketPlace,
            int paymentStatus,
            int orderGroupId);

        /// <summary>
        /// Get the order details with the define order id
        /// </summary>
        /// <param name="orderId">The id of the marketplace order</param>
        /// <returns></returns>
        OrderViewModel GetOrderById(string orderId);

        /// <summary>
        /// Get the unshippped order items of the specified order id
        /// </summary>
        /// <param name="orderId">The id of the order</param>
        /// <returns></returns>
        IEnumerable<OrderItemViewModel> GetOrderUnshippedItems(string orderId);
 
        /// <summary>
        /// Create or update order shipment detail and save it to the database
        /// </summary>
        /// <param name="shipmentModel">The object to save to database</param>
        void LogOrderShipment(OrderShipmentViewModel shipmentModel);

        /// <summary>
        /// Update the order shipment with the updated shipment data
        /// </summary>
        /// <param name="shipmentModel">The updated shipment data</param>
        void UpdateOrderShipment(OrderShipmentViewModel shipmentModel);
        /// <summary>
        /// Update the order notes
        /// </summary>
        /// <param name="shipmentModel">The updated shipment data</param>
        void UpdateOrder(OrderViewModel orderModel);

        /// <summary>
        /// Get the current maximum EIS Order Id
        /// </summary>
        /// <returns></returns>
        int GetNextEisOrderId();

        /// <summary>
        /// Insert the manual order to the database
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        OrderViewModel SaveManualOrder(OrderViewModel orderModel);

        /// <summary>
        /// Save the marketplace orders to the database
        /// </summary>
        /// <param name="orderResults">The list of marketplace orders</param>
        /// <returns></returns>
        int SaveMarketplaceOrders(List<MarketplaceOrder> orderResults);

        /// <summary>
        /// Update the manual order details and order items
        /// </summary>
        /// <param name="orderId">The id of the Order</param>
        /// <param name="orderModel">The updated order</param>
        /// <returns></returns>
        OrderViewModel UpdateManualOrder(string orderId, OrderViewModel orderModel);

        /// <summary>
        /// Update the order products for the specified order id
        /// </summary>
        /// <param name="orderId">The order id</param>
        void UpdateOrderProducts(string orderId);

        /// <summary>
        /// Update the EIS Order data with the marketplace order
        /// </summary>
        /// <param name="marketplaceOrderId">The marketplace id of the Order</param>
        /// <param name="order">The updated marketpalce order data</param>
        /// <returns></returns>
        OrderListViewModel UpdateMarketplaceOrder(string marketplaceOrderId, MarketplaceOrder order);

        /// <summary>
        /// Export orders with the selected criteria define in the model
        /// </summary>
        /// <param name="model">The user's criteria</param>
        /// <returns>Returns the file name</returns>
        string CustomExportOrderAsync(ExportOrder model);

        /// <summary>
        /// Toggle the Order's IsExport field value
        /// </summary>
        /// <param name="isExported"></param>
        /// <param name="isSelectAllPages"></param>
        /// <param name="eisOrderIds"></param>
        void ToggleOrderExportValue(bool isExported, bool isSelectAllPages, List<int> eisOrderIds);

        /// <summary>
        /// Get the pending order deatail with the specified EIS supplier SKU
        /// </summary>
        /// <param name="eisSupplierSKU">The EIS supplier SKU</param>
        /// <returns></returns>
        List<PendingOrderViewModel> GetPendingOrders(string eisSupplierSKU);

        /// <summary>
        /// Update the vendor product inventory
        /// </summary>
        /// <param name="orderProducts"></param>
        /// <param name="isOrderCanceled"></param>
        /// <returns></returns>
        bool UpdateVendorProductInventory(List<orderproduct> orderProducts, bool isOrderCanceled);
    }
}
