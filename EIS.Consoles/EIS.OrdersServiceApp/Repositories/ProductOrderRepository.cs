using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.Services;

namespace EIS.OrdersServiceApp.Repositories
{
    /// <summary>
    /// This class represents for updating the product inventory and checking the order update history data
    /// </summary>
    public class ProductOrderRepository
    {
        private readonly OrderService _service;

        public ProductOrderRepository()
        {
            _service = new OrderService(new LogService());
        }

        public bool HasOrderItemUpdateHistory(string orderItemId, OrderStatus status, DateTime purchaseDate)
        {
            using(var context = new EisInventoryContext())
            {
                var isCheckOrderCanceled = status == OrderStatus.Canceled;
                var result = context.orderupdatehistories
                    .FirstOrDefault(x => x.OrderItemId == orderItemId 
                        && x.PurchaseDate == purchaseDate
                        && (!isCheckOrderCanceled || x.OrderStatus == OrderStatus.Canceled));

                return result != null;
            }
        }

        public void CreateOrderItemUpdateHistory(MarketplaceOrderItem orderItem, OrderStatus orderStatus, DateTime purchaseDate)
        {
            using(var context = new EisInventoryContext())
            {
                context.orderupdatehistories.Add(new orderupdatehistory
                {
                    OrderItemId = orderItem.OrderItemId,
                    QtyOrdered = orderItem.QtyOrdered,
                    OrderStatus = orderStatus,
                    PurchaseDate = purchaseDate,
                    ResultDate = DateTime.UtcNow
                });
                
                // save changes
                context.SaveChanges();
            }
        }

        public bool ReturnVendorProductInventory(MarketplaceOrderItem orderItem)
        {
            var orderProducts = new List<orderproduct>();
            using(var context = new EisInventoryContext())
            {
                // get the order products assigned for this order item
                orderProducts = context.orderproducts
                    .Where(x => x.OrderItemId == orderItem.OrderItemId)
                    .ToList();    
            }

            // return back the vendor product inventory
            return _service.UpdateVendorProductInventory(orderProducts, true);
        }

        
    }
}
