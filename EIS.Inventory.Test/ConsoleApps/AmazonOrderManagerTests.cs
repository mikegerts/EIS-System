using System;
using EIS.OrdersServiceApp.Marketplaces;
using EIS.OrdersServiceApp.Repositories;

namespace EIS.Inventory.Test
{
    public class UnitTest_AmazonOrderManager
    {
        private CredentialRepository _credentialRepository;
        
        //[TestInitialize]
        public void InitializeVariables ()
        {
            _credentialRepository = new CredentialRepository();
        }

        //[TestMethod]
        public void Should_GetMarketplaceOrders ()
        {
            // Arrange
            var amazonOrderManager = new AmazonOrdersService();
            var createdAfter = DateTime.Parse("10/30/2016");
            var credential = _credentialRepository
                .GetDefaultCredential("Amazon", "TEST");

            amazonOrderManager.Credential = credential;

            // Act
            var result = amazonOrderManager.GetMarketplaceOrders(createdAfter);


            // Assert
            //Assert.IsTrue(result != null, "There are no data retrieved.");

        }
    }
}
