using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BigCommerce4Net.Api;
using BigCommerce4Net.Domain;
using EIS.MwsOrdersManagerApp.Models;
using EIS.MwsOrdersManagerApp.Marketplaces;
using EIS.Inventory.DAL.Database;
using System.Linq;
using EIS.MwsOrdersManagerApp.Repositories;

namespace UnitTest_EShopo
{
    [TestClass]
    public class UnitTest_BigCommerceOrderManager
    {
        private Credential _credential;
        private CredentialRepository _credentialRepository;

        [TestInitialize]
        public void InitializeVariables ()
        {
            _credentialRepository = new CredentialRepository();

            _credential = _credentialRepository
                        .GetDefaultMarketplaceCredential("BigCommerce", "TEST");

        }

        [TestMethod]
        public void ShouldGetOrdersFromAPI ()
        {
            //Arrange
            var BigCommerceManager = new BigCommerceOrderManager();
            BigCommerceManager.MarketplaceCredential = _credential;
            var createdAfter = DateTime.Now;

            //Act
            var response = BigCommerceManager.GetMarketplaceOrders(createdAfter);
            
            //Assert
            Assert.IsTrue(response != null);
        }



        [TestMethod]
        public void ShouldConfirmOrderstoAPI ()
        {
            //Arrange
            var BigCommerceManager = new BigCommerceOrderManager();
            BigCommerceManager.MarketplaceCredential = _credential;
            var createdAfter = DateTime.Now;

            //Act
            var response = BigCommerceManager.ConfirmOrdersShipment();

            //Assert
            Assert.IsTrue(response);
        }
    }
}
