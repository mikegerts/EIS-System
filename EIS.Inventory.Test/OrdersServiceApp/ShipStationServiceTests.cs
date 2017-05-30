using EIS.Inventory.Core.Services;
using Xunit;

namespace EIS.Inventory.Test.OrdersServiceApp
{
    public class ShipStationServiceTests
    {
        [Fact]
        public void Test_PostOrderToShipStation()
        {
            // arrange
            var shipstationService = new ShipStationService();

            // act
            var result = shipstationService.PostOrderToShipStation();

        }
    }
}
