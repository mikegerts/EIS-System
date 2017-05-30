using System.Linq;
using Xunit;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Test.Services
{
    public class OrderServiceTests
    {
        public OrderServiceTests()
        {
            EIS.OrdersServiceApp.Helpers.AutoMapperConfig.CreateMappings();
        }

        [Fact]
        public void Should_Get_ShadowProduct_In_ManageOrderVendorProduct()
        {
            // arrange
            var orderItem = new MarketplaceOrderItem
            {
                SKU = "BS1001221_6",
                OrderItemId = "55981003740466",
                OrderId = "01000012",
                Title = "Scotchgard Scotch Gard Fabric Protector 14 Oz",
                QtyOrdered = 1
            };
            var factorQty = 6;
            var totalItemsRequired = orderItem.QtyOrdered * factorQty;
            var service = new OrderService(new LogService());

            // act
            var result = service.ManageOrderVendorProduct(orderItem);

            // assert
            Assert.NotNull(result);
            Assert.Equal(totalItemsRequired, result.TotalAvailableItems);
        }
    }
}
