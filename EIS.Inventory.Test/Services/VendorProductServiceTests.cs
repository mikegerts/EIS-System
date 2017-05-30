using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.ViewModels;
using Xunit;

namespace EIS.Inventory.Test.Services
{
    public class VendorProductServiceTests
    {
        [Fact]
        public void Should_Update_Vendor_Product_Inventory()
        {
            // arrange
            var service = new VendorProductService(new ImageHelper(new PersistenceHelper()),new LogService());
            var model = new VendorProduct
            {
                EisSupplierSKU = "CJ10800000000464",
                Description = "Test Description",
                Quantity = 100,
                IsQuantitySet = true,
            };

            // act
            var result = service.DoUpadateOrInsertVendorProduct(model, true, "Denden Mushi");

            // 
            Assert.NotEqual(0, result);
            Assert.Equal(1, result);
        }
    }
}
