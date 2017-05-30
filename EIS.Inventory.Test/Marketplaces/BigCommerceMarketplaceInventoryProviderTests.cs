using System;
using Xunit;
using EIS.Marketplace.BigCommerce;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.ViewModels;
using EIS.OrdersServiceApp.Marketplaces;
using EIS.OrdersServiceApp.Repositories;
using EIS.OrdersServiceApp.Models;
using EIS.Inventory.Core.ViewModels;
using System.Collections.Generic;
using EIS.Inventory.DAL.Database;
using System.Linq;
using BigCommerce4Net.Api;
using Newtonsoft.Json;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Test.Marketplaces
{
    public class BigCommerceMarketplaceInventoryProviderTests
    {
        CredentialService _credentialService;
        CredentialDto _credential;
        Credential _credentialRepo;
        CredentialRepository _credentialRepository;

        public BigCommerceMarketplaceInventoryProviderTests()
        {
            AutoMapperConfig.RegisterAutoMappers();
            _credentialService = new CredentialService();
            _credential = _credentialService.GetCredential("BigCommerce", MarketplaceMode.TEST.ToString());

            _credentialRepository = new CredentialRepository();
            _credentialRepo = _credentialRepository.GetDefaultCredential("BigCommerce", MarketplaceMode.TEST.ToString());
        }

        [Fact]
        public void Should_Delete_Single_BigCommerce_Product()
        {
            // Arrange
            var bcInventoryprovider = new BigCommerceMarketplaceInventoryProvider();
            bcInventoryprovider.Credential = _credential;
            var marketPlaceEndItemFeed = new ItemFeed();

            // Act
            marketPlaceEndItemFeed.EisSKU = "";
            marketPlaceEndItemFeed.ItemId = "";

            bcInventoryprovider.SubmitSingleProductEndItem(marketPlaceEndItemFeed, "UnitTest");


            // Assert


        }
    }
}
