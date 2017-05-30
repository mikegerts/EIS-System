using System;
using System.Collections.Generic;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class OrderRepository
    {
        public void ManageOrders(Order orderFile)
        {
            if (string.IsNullOrEmpty(orderFile.OrderId))
            {
                return;
            }
            using (var context = new EisInventoryContext())
            {
                // get the information

                var order = context.orders
                   .FirstOrDefault(x => x.OrderId == orderFile.OrderId);

                if (order == null)
                {
                    var eisOrderId = getMaxEisOrderId() + 1;

                    order = new order();
                    order = GetOrderModel(order, orderFile);
                    order.EisOrderId = eisOrderId;
                    context.orders.Add(order);
                    context.SaveChanges();

                    var orderItem = new orderitem();
                    foreach (var item in orderFile.OrderItems)
                    {
                        orderItem = new orderitem();
                        orderItem = GetOrderItem(orderItem, item);
                        context.orderitems.Add(orderItem);
                        context.SaveChanges();
                    }

                }
                //else
                //{
                //    order = GetOrderModel(order, orderFile);
                //    context.SaveChanges();
                //}


            }
        }

        private int getMaxEisOrderId()
        {
            int maxEisOrderId = 1000000;
            using (var context = new EisInventoryContext())
            {
                try
                {
                    var existingMaxEisOrderId = context.orders.Max(x => x.EisOrderId);
                    maxEisOrderId = existingMaxEisOrderId > 0 ? existingMaxEisOrderId : maxEisOrderId;
                }
                catch { }
            }
            return maxEisOrderId;
        }

        private order GetOrderModel(order order, Order orderFile)
        {
            if (!string.IsNullOrEmpty(orderFile.OrderId))
            {
                order.OrderId = orderFile.OrderId;
            }
            if (orderFile.EisOrderId > -1)
            {
                order.EisOrderId = orderFile.EisOrderId;
            }
            if (!string.IsNullOrEmpty(orderFile.Marketplace))
            {
                order.Marketplace = orderFile.Marketplace;
            }
            else
            {
                order.Marketplace = "Amazon";
            }
            if (orderFile.OrderTotal > -1)
            {
                order.OrderTotal = orderFile.OrderTotal;
            }

            if (orderFile.OrderStatus != 0)
                order.OrderStatus = (OrderStatus)orderFile.OrderStatus;
            else
                order.OrderStatus = OrderStatus.Unshipped;

            order.PaymentStatus = orderFile.PaymentStatus;

            if (orderFile.NumOfItemsShipped > 0)
            {
                order.NumOfItemsShipped = (int)orderFile.NumOfItemsShipped;
            }
            if (orderFile.NumOfItemsUnshipped > 0)
            {
                order.NumOfItemsUnshipped = (int)orderFile.NumOfItemsUnshipped;
            }
            if (orderFile.PurchaseDate != null)
            {
                order.PurchaseDate = orderFile.PurchaseDate;
            }
            if (orderFile.LastUpdateDate != null)
            {
                order.PurchaseDate = orderFile.LastUpdateDate;
            }
            if (!string.IsNullOrEmpty(orderFile.PaymentMethod))
            {
                order.PaymentMethod = orderFile.PaymentMethod;
            }
            if (!string.IsNullOrEmpty(orderFile.CompanyName))
            {
                order.CompanyName = orderFile.CompanyName;
            }
            if (!string.IsNullOrEmpty(orderFile.BuyerName))
            {
                order.BuyerName = orderFile.BuyerName;
            }
            if (!string.IsNullOrEmpty(orderFile.BuyerEmail))
            {
                order.BuyerEmail = orderFile.BuyerEmail;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingAddressPhone))
            {
                order.ShippingAddressPhone = orderFile.ShippingAddressPhone;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingAddressName))
            {
                order.ShippingAddressName = orderFile.ShippingAddressName;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingAddressLine1))
            {
                order.ShippingAddressLine1 = orderFile.ShippingAddressLine1;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingAddressLine2))
            {
                order.ShippingAddressLine2 = orderFile.ShippingAddressLine2;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingAddressLine3))
            {
                order.ShippingAddressLine3 = orderFile.ShippingAddressLine3;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingCity))
            {
                order.ShippingCity = orderFile.ShippingCity;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingStateOrRegion))
            {
                order.ShippingStateOrRegion = orderFile.ShippingStateOrRegion;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingPostalCode))
            {
                order.ShippingPostalCode = orderFile.ShippingPostalCode;
            }
            if (!string.IsNullOrEmpty(orderFile.ShipServiceLevel))
            {
                order.ShipServiceLevel = orderFile.ShipServiceLevel;
            }
            if (!string.IsNullOrEmpty(orderFile.ShipmentServiceCategory))
            {
                order.ShipmentServiceCategory = orderFile.ShipmentServiceCategory;
            }

            if (orderFile.ShipmentDate == null)
            {
                order.ShipmentDate = orderFile.ShipmentDate;
            }
            if (!string.IsNullOrEmpty(orderFile.CarrierCode))
            {
                order.CarrierCode = orderFile.CarrierCode;
            }
            if (!string.IsNullOrEmpty(orderFile.ShippingMethod))
            {
                order.ShippingMethod = orderFile.ShippingMethod;
            }
            if (!string.IsNullOrEmpty(orderFile.TrackingNumber))
            {
                order.TrackingNumber = orderFile.TrackingNumber;
            }
            if (orderFile.ShipmentCost > -1)
            {
                order.ShipmentCost = orderFile.ShipmentCost;
            }
            if (!string.IsNullOrEmpty(orderFile.OrderNote))
            {
                order.OrderNote = orderFile.OrderNote;
            }

            return order;
        }

        private orderitem GetOrderItem(orderitem orderItem, OrderItem orderItemFile)
        {

            orderItem.ConditionNote = orderItemFile.ConditionNote;
            orderItem.GiftWrapPrice = orderItemFile.GiftWrapPrice;
            orderItem.GiftWrapTax = orderItemFile.GiftWrapTax;
            orderItem.ItemTax = orderItemFile.ItemTax;
            orderItem.OrderId = orderItemFile.OrderId;
            orderItem.OrderItemId = orderItemFile.OrderItemId;
            orderItem.Price = orderItemFile.Price;
            orderItem.PromotionDiscount = orderItemFile.PromotionDiscount;
            orderItem.QtyOrdered = orderItemFile.QtyOrdered;
            orderItem.QtyShipped = orderItemFile.QtyShipped;
            orderItem.ShippingDiscount = orderItemFile.ShippingDiscount;
            orderItem.ShippingPrice = orderItemFile.ShippingPrice;
            orderItem.ShippingTax = orderItemFile.ShippingTax;
            orderItem.SKU = orderItemFile.SKU;
            orderItem.Title = orderItemFile.Title;

            return orderItem;
        }
    }
}