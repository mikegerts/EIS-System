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
    public class BigCommerceMarketplaceOrdersProviderTests
    {
        CredentialService _credentialService;
        CredentialDto _credential;
        Credential _credentialRepo;
        CredentialRepository _credentialRepository;

        public BigCommerceMarketplaceOrdersProviderTests()
        {
            AutoMapperConfig.RegisterAutoMappers();
            _credentialService = new CredentialService();
            _credential = _credentialService.GetCredential("BigCommerce", MarketplaceMode.TEST.ToString());

            _credentialRepository = new CredentialRepository();
            _credentialRepo = _credentialRepository.GetDefaultCredential("BigCommerce", MarketplaceMode.TEST.ToString());
        }

        [Fact]
        public void Should_Get_BigCommerce_OrderStatus()
        {
            // Arrange
            var bcOrderprovider = new BigCommerceMarketplaceOrdersProvider();
            bcOrderprovider.MarketplaceCredential = _credential;

            // Act
            var result = bcOrderprovider.GetBCOrderStatus();


            // Assert
            Assert.True(result != null);
            
        }

        [Fact]
        public void Should_Get_BigCommerce_CarrierCodes()
        {
            // Arrange
            var bcOrderprovider = new BigCommerceMarketplaceOrdersProvider();
            bcOrderprovider.MarketplaceCredential = _credential;

            // Act
            var result = bcOrderprovider.GetShippingCarriers();


            // Assert
            Assert.True(result != null);

        }

        [Fact]
        public void Should_Get_GetMarketplaceOrders()
        {
            // Arrange
            var bcOrderService = new BigCommerceOrdersService();
            bcOrderService.Credential = _credentialRepo;
            var curDateTime = DateTime.Now.AddDays(-120);

            // Act
            var result = bcOrderService.GetMarketplaceOrders(curDateTime);

            // Assert
            Assert.True(result != null);

        }

        [Fact]
        public void Should_Get_GetMarketplaceOrdersByID()
        {
            // Arrange
            var bcOrderService = new BigCommerceMarketplaceOrdersProvider();
            bcOrderService.MarketplaceCredential = _credential;
            var orderid = "1027";

            // Act
            var result = bcOrderService.GetMarketplaceOrder(orderid);

            // Assert
            Assert.True(result != null);

        }

        [Fact]
        public void Should_Get_GetMarketplaceOrdersJson()
        {
            // Arrange
            var bcOrderService = new BigCommerceOrdersService();
            bcOrderService.Credential = _credentialRepo;
            var curDateTime = DateTime.Now.AddDays(-120);
            var filter = new FilterOrders
            {
                MinimumDateCreated = curDateTime.Date
            };

            // Act
            var bigCommerceOrders = bcOrderService.GetBigCommerceOrders(filter);
            var result = JsonConvert.SerializeObject(bigCommerceOrders);

            // Assert
            Assert.True(result != null);

        }

        [Fact]
        public void Shoud_Get_CategoryList()
        {
            // Arrange
            var bcOrderInventoryProvider = new BigCommerceMarketplaceInventoryProvider();
            bcOrderInventoryProvider.Credential = _credential;

            // Act
            var result = bcOrderInventoryProvider.GetCategoryList();

            // Assert
            Assert.True(result.Count > 0);

        }

        [Fact]
        public void Should_Send_ProductListing()
        {
            // Arrange
            EisInventoryContext _context = new EisInventoryContext();
            var bcOrderInventoryProvider = new BigCommerceMarketplaceInventoryProvider();
            bcOrderInventoryProvider.Credential = _credential;

            var eisSKU = "";
            var productListFeed = new List<MarketplaceProductFeedDto>();
            var product = _context.products.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productBC = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productFeed = new MarketplaceProductFeedDto() {
                EisSKU = product.EisSKU,
                Name = product.Name,
                ItemWidth = product.ItemWidth,
                ItemHeight = product.ItemHeight,
                Description = product.Description,
                UPC = product.UPC
            };
            
            productFeed.BigCommerceProductFeed = new BigCommerceProductFeed() {
                EisSKU = productBC.EisSKU,
                Price = productBC.Price,
                ProductsType = productBC.ProductsType.Value,
                Condition = productBC.Condition,
                Categories = productBC.Categories,
                CategoryId = productBC.CategoryId
            };

            productListFeed.Add(productFeed);

            // Act
            bcOrderInventoryProvider.SubmitProductsListingFeed(productListFeed, "Test");


            // Assert
        }

        [Fact]
        public void Should_Send_InventoryListing()
        {
            // Arrange
            EisInventoryContext _context = new EisInventoryContext();
            var bcOrderInventoryProvider = new BigCommerceMarketplaceInventoryProvider();
            bcOrderInventoryProvider.Credential = _credential;

            var eisSKU = "";
            var productListFeed = new List<MarketplaceInventoryFeed>();
            var product = _context.products.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productBC = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productFeed = new MarketplaceInventoryFeed()
            {
                EisSKU = product.EisSKU
            };

            productFeed.BigCommerceInventoryFeed = new BigCommerceInventoryFeed()
            {
                SKU = productBC.EisSKU,
                InventoryLevel = productBC.InventoryLevel,
                InventoryWarningLevel = productBC.InventoryWarningLevel,
                InventoryTracking = productBC.InventoryTracking,
                OrderQuantityMaximum = productBC.OrderQuantityMaximum,
                OrderQuantityMinimum = productBC.OrderQuantityMinimum,
                ProductId = productBC.ProductId
            };

            productListFeed.Add(productFeed);

            // Act
            bcOrderInventoryProvider.SubmitProductInventoryFeeds(productListFeed, "Test");


            // Assert
        }

        [Fact]
        public void Should_Send_ReviseListing()
        {
            // Arrange
            EisInventoryContext _context = new EisInventoryContext();
            var bcOrderInventoryProvider = new BigCommerceMarketplaceInventoryProvider();
            bcOrderInventoryProvider.Credential = _credential;

            var eisSKU = "";
            var productListFeed = new List<MarketplaceProductFeedDto>();
            var product = _context.products.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productBC = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productFeed = new MarketplaceProductFeedDto()
            {
                EisSKU = product.EisSKU,
                Name = product.Name,
                ItemWidth = product.ItemWidth,
                ItemHeight = product.ItemHeight,
                Description = product.Description,
                UPC = product.UPC
            };

            productFeed.BigCommerceProductFeed = new BigCommerceProductFeed()
            {
                EisSKU = productBC.EisSKU,
                Price = productBC.Price,
                ProductsType = productBC.ProductsType.Value,
                Condition = productBC.Condition,
                Categories = productBC.Categories,
                CategoryId = productBC.CategoryId,
                ProductId = productBC.ProductId,
                Brand = productBC.Brand                
            };

            productListFeed.Add(productFeed);

            // Act
            bcOrderInventoryProvider.SubmitProductsReviseFeed(productListFeed, "Test");


            // Assert
        }

        [Fact]
        public void Should_Send_PriceListing()
        {
            // Arrange
            EisInventoryContext _context = new EisInventoryContext();
            var bcOrderInventoryProvider = new BigCommerceMarketplaceInventoryProvider();
            bcOrderInventoryProvider.Credential = _credential;

            var eisSKU = "";
            var productListFeed = new List<MarketplacePriceFeedDto>();
            var product = _context.products.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productBC = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == eisSKU);
            var productFeed = new MarketplacePriceFeedDto()
            {
                EisSKU = product.EisSKU
            };

            productFeed.BigCommerceProductFeed = new BigCommerceProductFeed()
            {
                EisSKU = productBC.EisSKU,
                ProductId = productBC.ProductId,
                FixedCostShippingPrice = productBC.FixedCostShippingPrice,
                Price = productBC.Price,
                RetailPrice = productBC.RetailPrice
            };

            productListFeed.Add(productFeed);

            // Act
            bcOrderInventoryProvider.SubmitProductPriceFeeds(productListFeed, "Test");


            // Assert
        }

    }
}
