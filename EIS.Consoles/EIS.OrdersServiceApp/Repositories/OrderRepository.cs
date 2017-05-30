using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.OrdersServiceApp.Models;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.OrdersServiceApp.Repositories
{
    /// <summary>
    /// This class represents for database CRUD operations for Order and OrderItem records
    /// </summary>
    public class OrderRepository
    {
        private readonly LoggerRepository _logger;

        public OrderRepository()
        {
            _logger = new LoggerRepository();
        }

        public void DoInsertOrupdateOrder(MarketplaceOrder marketplaceOrder)
        {
            try
            {
                using (var context = new EisInventoryContext())
                {
                    // check if this order alreaady exist
                    var order = context.orders.FirstOrDefault(x => x.OrderId == marketplaceOrder.OrderId);
                    if (order == null)
                    {
                        var eisOrderId = getMaxEisOrderId() + 1;
                        context.orders.Add(new order
                        {
                            EisOrderId = eisOrderId,
                            OrderId = marketplaceOrder.OrderId,
                            Marketplace = marketplaceOrder.Marketplace,
                            OrderTotal = marketplaceOrder.OrderTotal,
                            NumOfItemsShipped = (int)marketplaceOrder.NumOfItemsShipped,
                            NumOfItemsUnshipped = (int)marketplaceOrder.NumOfItemsUnshipped,
                            OrderStatus = marketplaceOrder.OrderStatus,
                            PurchaseDate = marketplaceOrder.PurchaseDate,
                            LastUpdateDate = marketplaceOrder.LastUpdateDate,
                            PaymentMethod = marketplaceOrder.PaymentMethod,
                            BuyerName = marketplaceOrder.BuyerName,
                            BuyerEmail = marketplaceOrder.BuyerEmail,
                            ShippingAddressPhone = marketplaceOrder.ShippingAddressPhone,
                            ShippingAddressName = marketplaceOrder.ShippingAddressName,
                            ShippingAddressLine1 = marketplaceOrder.ShippingAddressLine1,
                            ShippingAddressLine2 = marketplaceOrder.ShippingAddressLine2,
                            ShippingAddressLine3 = marketplaceOrder.ShippingAddressLine3,
                            ShippingCity = marketplaceOrder.ShippingCity,
                            ShippingStateOrRegion = marketplaceOrder.ShippingStateOrRegion,
                            ShippingPostalCode = marketplaceOrder.ShippingPostalCode,
                            ShipServiceLevel = marketplaceOrder.ShipServiceLevel,
                            ShipmentServiceCategory = marketplaceOrder.ShipmentServiceCategory,
                            EarliestShipDate = marketplaceOrder.EarliestShipDate,
                            LatestShipDate = marketplaceOrder.LatestShipDate,
                            EarliestDeliveryDate = marketplaceOrder.EarliestDeliveryDate,
                            LatestDeliveryDate = marketplaceOrder.LatestDeliveryDate,
                            OrderType = marketplaceOrder.OrderType,
                            SellerOrderId = marketplaceOrder.SellerOrderId,
                            MarketplaceId = marketplaceOrder.MarketplaceId,
                            PurchaseOrderNumber = marketplaceOrder.PurchaseOrderNumber,
                            SalesChannel = marketplaceOrder.SalesChannel,
                            AdjustmentAmount = marketplaceOrder.AdjustmentAmount,
                            AmountPaid = marketplaceOrder.AmountPaid,
                            PaymentOrRefundAmount = marketplaceOrder.PaymentOrRefundAmount,
                            PaymentStatus = 1,
                            OrderNote = null,
                            Created = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        order.Marketplace = marketplaceOrder.Marketplace;
                        order.OrderTotal = marketplaceOrder.OrderTotal;
                        order.NumOfItemsShipped = (int)marketplaceOrder.NumOfItemsShipped;
                        order.NumOfItemsUnshipped = (int)marketplaceOrder.NumOfItemsUnshipped;
                        order.OrderStatus = marketplaceOrder.OrderStatus;
                        order.PurchaseDate = marketplaceOrder.PurchaseDate;
                        order.LastUpdateDate = marketplaceOrder.LastUpdateDate;
                        order.PaymentMethod = marketplaceOrder.PaymentMethod;
                        order.BuyerName = marketplaceOrder.BuyerName;
                        order.BuyerEmail = marketplaceOrder.BuyerEmail;
                        order.ShippingAddressPhone = marketplaceOrder.ShippingAddressPhone;
                        order.ShippingAddressName = marketplaceOrder.ShippingAddressName;
                        order.ShippingAddressLine1 = marketplaceOrder.ShippingAddressLine1;
                        order.ShippingAddressLine2 = marketplaceOrder.ShippingAddressLine2;
                        order.ShippingAddressLine3 = marketplaceOrder.ShippingAddressLine3;
                        order.ShippingCity = marketplaceOrder.ShippingCity;
                        order.ShippingStateOrRegion = marketplaceOrder.ShippingStateOrRegion;
                        order.ShippingPostalCode = marketplaceOrder.ShippingPostalCode;
                        order.ShipServiceLevel = marketplaceOrder.ShipServiceLevel;
                        order.ShipmentServiceCategory = marketplaceOrder.ShipmentServiceCategory;
                        order.EarliestShipDate = marketplaceOrder.EarliestShipDate;
                        order.LatestShipDate = marketplaceOrder.LatestShipDate;
                        order.EarliestDeliveryDate = marketplaceOrder.EarliestDeliveryDate;
                        order.LatestDeliveryDate = marketplaceOrder.LatestDeliveryDate;
                        order.OrderType = marketplaceOrder.OrderType;
                        order.SellerOrderId = marketplaceOrder.SellerOrderId;
                        order.MarketplaceId = marketplaceOrder.MarketplaceId;
                        order.PurchaseOrderNumber = marketplaceOrder.PurchaseOrderNumber;
                        order.SalesChannel = marketplaceOrder.SalesChannel;
                        order.AdjustmentAmount = marketplaceOrder.AdjustmentAmount;
                        order.AmountPaid = marketplaceOrder.AmountPaid;
                        order.PaymentOrRefundAmount = marketplaceOrder.PaymentOrRefundAmount;
                        order.PaymentStatus = 1;
                        order.OrderNote = null;
                        order.Created = DateTime.UtcNow;
                    }

                    // save the chagnes
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error occured on order id: {0} \nError Message: {1} \n{2} ", marketplaceOrder.OrderId, ex.Message, ex.StackTrace);
                _logger.LogError(LogEntryType.OrderService,
                   string.Format("Error occured in creating or updating order id {0} to DB <br/>Error message: {1}",
                    marketplaceOrder.OrderId,
                    EisHelper.GetExceptionMessage(ex)), ex.StackTrace);
            }
        }


        /// <summary>
        /// Insert new record or update for order item into database
        /// </summary>
        /// <param name="item">The record of order item to save to database</param>
        public void DoInsertOrUpdateOrderItem(MarketplaceOrderItem item)
        {
            try
            {
                using (var context = new EisInventoryContext())
                {
                    // check if there is existing order item
                    var orderItem = context.orderitems
                        .FirstOrDefault(x => x.OrderItemId == item.OrderItemId && x.OrderId == item.OrderId);

                    if (orderItem == null)
                    {
                        context.orderitems.Add(new orderitem
                        {
                            OrderItemId = item.OrderItemId,
                            OrderId = item.OrderId,
                            ItemId = item.MarketplaceItemId,
                            SKU = item.SKU,
                            Title = item.Title,
                            QtyOrdered = item.QtyOrdered,
                            QtyShipped = item.QtyShipped,
                            Price = item.Price,
                            ShippingPrice = item.ShippingPrice,
                            GiftWrapPrice = item.GiftWrapPrice,
                            ItemTax = item.Tax,
                            ShippingTax = item.ShippingTax,
                            GiftWrapTax = item.GiftWrapTax,
                            ShippingDiscount = item.ShippingDiscount,
                            PromotionDiscount = item.PromotionDiscount,
                            ConditionNote = item.ConditionNote,
                        });
                    }
                    else
                    {
                        orderItem.ItemId = item.MarketplaceItemId;
                        orderItem.SKU = item.SKU;
                        orderItem.Title = item.Title;
                        orderItem.QtyOrdered = item.QtyOrdered;
                        orderItem.QtyShipped = item.QtyShipped;
                        orderItem.Price = item.Price;
                        orderItem.ShippingPrice = item.ShippingPrice;
                        orderItem.GiftWrapPrice = item.GiftWrapPrice;
                        orderItem.ItemTax = item.Tax;
                        orderItem.ShippingTax = item.ShippingTax;
                        orderItem.GiftWrapTax = item.GiftWrapTax;
                        orderItem.ShippingDiscount = item.ShippingDiscount;
                        orderItem.PromotionDiscount = item.PromotionDiscount;
                        orderItem.ConditionNote = item.ConditionNote;
                    }

                    // save the changes
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error occured on order item id: {0} \nError Message: {1} \n{2} ", item.OrderItemId, ex.Message, ex.StackTrace);
                _logger.LogError(LogEntryType.OrderService,
                  string.Format("Error occured in inserting or creating order item {0} and order id {1} to DB <br/>Error message: {2}",
                    item.OrderItemId,
                    item.OrderId,
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
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
        

        public List<MarketplaceOrderFulfillment> GetUnshippedOrdersForShipment(string marketplaceType)
        {
            var orderFulfillements = new List<MarketplaceOrderFulfillment>();
            using (var context = new EisInventoryContext())
            {
                // get the list of orders that are already have tracking number and no shipment history yet
                var unConfirmOrders = context.orders
                    .Include("ordershipmenthistory")
                    .Where(x => x.OrderStatus == OrderStatus.Unshipped
                        && !string.IsNullOrEmpty(x.TrackingNumber)
                        && x.Marketplace == marketplaceType
                        && x.ordershipmenthistory == null)
                    .ToList();

                // parse it into orde fulfillment
                foreach (var order in unConfirmOrders)
                {
                    var orderFulfillment = new MarketplaceOrderFulfillment
                    {
                        Marketplace = order.Marketplace,
                        OrderId = order.OrderId,
                        FulfillmentDate = order.LastUpdateDate.Value.Date, // use the LastUpdateDate instead of the Shipment date
                        CarrierCode = order.CarrierCode,
                        ShippingMethod = order.ShippingMethod,
                        ShipperTrackingNumber = order.TrackingNumber
                    };

                    // parse and add the order items
                    foreach (var item in order.orderitems)
                    {
                        orderFulfillment.OrderItems.Add(new MarketplaceOrderFulfillmentItem
                        {
                            Quantity = item.QtyOrdered,
                            OrderItemId = item.OrderItemId
                        });
                    }

                    orderFulfillements.Add(orderFulfillment);
                }
            }

            return orderFulfillements;
        }

        public int InsertOrderShipmentHistory(List<MarketplaceOrderFulfillment> orderFulfillments)
        {
            using (var context = new EisInventoryContext())
            {
                var resultDate = DateTime.Now;
                foreach (var item in orderFulfillments)
                {
                    var history = new ordershipmenthistory
                    {
                        OrderId = item.OrderId,
                        ResultDate = resultDate
                    };

                    context.ordershipmenthistories.Add(history);
                }

                return context.SaveChanges();
            }
        }

        public int DeleteOrderShipmentHistory(List<MarketplaceOrderFulfillment> orderFeeds)
        {
            using (var context = new EisInventoryContext())
            {
                var orderIds = orderFeeds.Select(x => x.OrderId);
                var histories = context.ordershipmenthistories.Where(x => orderIds.Contains(x.OrderId));

                // delete it
                context.ordershipmenthistories.RemoveRange(histories);
                return context.SaveChanges();
            }
        }
    }
}
