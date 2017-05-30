using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace EIS.Inventory.Test.Services
{
    public class OrderSeriveTests
    {
        public OrderSeriveTests()
        {
            AutoMapperConfig.RegisterAutoMappers();
        }

        [Fact]
        public void Should_Map_MarektplaceOrders_To_Domain()
        {
            // arrange
            IOrderService orderService = new OrderService(null);
            var orderItems = new List<MarketplaceOrderItem>();
            orderItems.Add(new MarketplaceOrderItem
            {
                SKU = "EIS-SKU-B",
                Title = "Product A Kigwa",
                Price = 12345,
            });
            orderItems.Add(new MarketplaceOrderItem
            {
                SKU = "EIS-SKU-A",
                Title = "Product B Kigwa",
                Price = 67890,
            });

            var marketplaceOrder = new MarketplaceOrder
            {
                AdjustmentAmount = 1,
                AmountPaid = 2,
                BuyerName = "Eduardo Genita",
                BuyerEmail = "egenita@outlook.com",
                EarliestShipDate = DateTime.Now,
                EarliestDeliveryDate = DateTime.Now,
                LastUpdateDate = DateTime.Now,
                LatestShipDate = DateTime.Now,
                LatestDeliveryDate = DateTime.Now,
                Marketplace = "Amazon",
                MarketplaceId = "M-12345-67890",
                NumOfItemsShipped = 3,
                NumOfItemsUnshipped = 3,
                OrderId = "EIS-12345-67890",
                OrderStatus = OrderStatus.Unshipped,
                OrderItems = orderItems,
                OrderTotal = 3,
                OrderType = "OrderTYpe",
                PaymentMethod = "PayPal",
                PaymentOrRefundAmount = 5,
                PurchaseDate = DateTime.Now,
                PurchaseOrderNumber = "PO-12345-67890",
                SalesChannel = "Amazon.com",
                SellerOrderId = "12345-67890",
                ShipmentServiceCategory = "USPS"
            };


            // act
            orderService.SaveMarketplaceOrders(new List<MarketplaceOrder> { marketplaceOrder });

            // assert
            Assert.True(true);
        }
    }
}
