using AutoMapper;
using Xunit;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Test.Mappings
{
    public class OrdersServiceAutoMappingTests
    {
        public OrdersServiceAutoMappingTests()
        {
            EIS.OrdersServiceApp.Helpers.AutoMapperConfig.CreateMappings();
        }


        [Fact]
        public void Should_Mapped_OrderProduct_To_Model()
        {
            // arrange
            var domain = new orderproduct
            {
                EisSupplierSKU = "EIS_123_XXX",
                Quantity = 10,
                Pack = 12,
            };

            // act
            var model = Mapper.Map<OrderProduct>(domain);

            // assert
            Assert.NotNull(model);
            Assert.Equal(model.EisSupplierSKU, domain.EisSupplierSKU);
            Assert.Equal(model.Quantity, domain.Quantity);
            Assert.Equal(model.Pack, domain.Pack);
        }
    }
}
