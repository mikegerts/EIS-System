using System;
using Xunit;
using EIS.OrdersServiceApp.Marketplaces;
using EIS.OrdersServiceApp.Repositories;

namespace EIS.Inventory.Test
{
    public class eBayOrderManagerTests
    {
        private CredentialRepository _credentialRepository;
        
        public eBayOrderManagerTests()
        {
            _credentialRepository = new CredentialRepository();
        }

        [Fact]
        public void Should_GetMarketplaceOrders ()
        {
            // Arrange
            var eBayOrderManager = new eBayOrdersService();
            var createdAfter = DateTime.Parse("05/14/2017");
            var credential = _credentialRepository
                .GetDefaultCredential("eBay", "LIVE");

            eBayOrderManager.Credential = credential;

            // Act
            var result = eBayOrderManager.GetMarketplaceOrders(createdAfter);


            // Assert
            //Assert.IsTrue(result != null, "There are no data retrieved.");

        }
    }
}
