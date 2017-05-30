using Xunit;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Test
{
    public class CopyObjectTests
    {
        [Fact]
        public void Should_Copy_Fields_For_OrderProduct()
        {
            // arrange
            var result = new OrderProduct();
            var destination = new orderproduct
            {
                EisSupplierSKU = "SKU_1",
                Quantity = 1,
                Pack = 1
            };


            // act
            CopyObject.CopyFields(destination, result);

            // assert
            Assert.Equal(destination.EisSupplierSKU, result.EisSupplierSKU);
            Assert.Equal(destination.Quantity, result.Quantity);
            Assert.Equal(destination.Pack, result.Pack);
        }
    }
}
