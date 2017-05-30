using System.Linq;
using Xunit;
using AutoMapper;
using EIS.Inventory.Core.Mappings;
using EIS.Inventory.Core.Services;

namespace EIS.Inventory.Test.Services
{
    public class ShippingServiceTests
    {
        public ShippingServiceTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
        }

        [Fact]
        public void Should_Get_Awaiting_Shipments()
        {
            // arrange
            var logger = new LogService();
            var service = new ShippingService(logger);

            // act
            var results = service.GetAwaitingShipments(1, 100);

            // assert
            Assert.NotNull(results);
            Assert.True(results.Any());

            foreach (var item in results)
                Assert.True(item.Quantity > 0);
        }
    }
}
