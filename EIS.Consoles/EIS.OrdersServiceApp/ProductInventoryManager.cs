using System;
using System.Collections.Generic;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;
using EIS.OrdersServiceApp.Repositories;
using EIS.Inventory.Shared.Helpers;

namespace EIS.OrdersServiceApp
{
    public class ProductInventoryManager
    {
        private readonly ProductOrderRepository _repository;
        private readonly LoggerRepository _logger;
        private readonly OrderService _service;

        public ProductInventoryManager()
        {
            _repository = new ProductOrderRepository();
            _logger = new LoggerRepository();
            _service = new OrderService(new LogService());
        }
                
        public void UpdateOrderVendorProductInventory(List<MarketplaceOrder> orders)
        {
            try
            {
                var results = new List<OrderProductResult>();

                // iterate to each order and also to its order item
                foreach (var order in orders)
                {
                    // then, to its order items
                    foreach (var orderItem in order.OrderItems)
                    {
                        // let's determine if this item has already record in order upate history
                        var hasHistory = _repository.HasOrderItemUpdateHistory(orderItem.OrderItemId, order.OrderStatus, order.PurchaseDate);
                        if (hasHistory)
                            continue;

                        // otherwise let's create order item update history
                        _repository.CreateOrderItemUpdateHistory(orderItem, order.OrderStatus, order.PurchaseDate);

                        // manage the vendor product inventory
                        if (order.OrderStatus != OrderStatus.Canceled)
                        {
                            var result = _service.ManageOrderVendorProduct(orderItem);
                            results.Add(result);
                        }
                        else
                            _repository.ReturnVendorProductInventory(orderItem);
                    }
                }

                // notify Admin if there are insufficient products for the orders
                _service.EvaluateForInsufficientVendorProducts(results);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error Message: " + EisHelper.GetExceptionMessage(ex) + "\n" + ex.StackTrace);
                _logger.LogError(LogEntryType.OrderService,
                   string.Format("Error in updating vendor product inventory <br/>Error message: {0}",
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
            }
        }
    }
}
