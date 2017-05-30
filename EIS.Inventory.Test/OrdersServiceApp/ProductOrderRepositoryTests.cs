using System;
using System.Collections.Generic;
using Xunit;
using EIS.Inventory.Shared.Models;
using EIS.OrdersServiceApp;

namespace EIS.Inventory.Test.OrdersServiceApp
{
    public class ProductOrderRepositoryTests
    {
        [Fact]
        public void Should_Update_Vendor_Product_Inventory()
        {
            // prepare
            var orderItem = new MarketplaceOrderItem
            {
                SKU = "MI65155370_4",
                OrderItemId = "00226200885258",
                QtyOrdered = 2
            };
            var order = new MarketplaceOrder {
                OrderItems = new List<MarketplaceOrderItem> { orderItem },
                OrderStatus = OrderStatus.Canceled,
                PurchaseDate = new DateTime(2016, 12, 26, 5, 48, 36) // 2016-12-26 05:48:36
            };
            var manager = new ProductInventoryManager();

            // act
            manager.UpdateOrderVendorProductInventory(new List<MarketplaceOrder> { order });
        }

        [Fact]
        public void Should_Get_Desired_Result()
        {
            // arrange
            var itemsRequired = 5;
            var minPack = 3;
            var factorQty = 6;

            // act
            var result = getAllowedQuantity(itemsRequired, minPack, factorQty);

            // assert
            Assert.NotEqual(result, itemsRequired);
            Assert.Equal(result, 4);
        }

        [Fact]
        public void Should_Get_Desired_Result_2()
        {
            // arrange
            var itemsRequired = 7;
            var minPack = 3;
            var factorQty = 6;

            // act
            var result = getAllowedQuantity(itemsRequired, minPack, factorQty);

            // assert
            Assert.NotEqual(result, itemsRequired);
            Assert.Equal(result, 6);
        }


        private int getAllowedQuantity(int itemsRequired, int minPack, int factorQty)
        {
            var result = itemsRequired * minPack;
            if (result % factorQty == 0)
                return itemsRequired;

            return getAllowedQuantity(itemsRequired - 1, minPack, factorQty);
        }
    }
}
