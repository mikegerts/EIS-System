using System.Linq;
using Xunit;
using AutoMapper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using Ploeh.AutoFixture;
using System;

namespace EIS.Inventory.Test.Mappings
{
    public class SchedulerTaskServiceAutoMappingTests
    {
        public SchedulerTaskServiceAutoMappingTests()
        {
            SchedulerTaskApp.Helpers.AutoMapperConfig.CreateMappings();
        }

        [Fact]
        public void Should_Map_Product_To_MarketplaceInventoryFeed()
        {
            // arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var domain = fixture.Create<product>();
            domain.productebay.product = domain;
            domain.productamazon.product = domain;
            domain.productbigcommerce.product = domain;

            var vendor = domain.AvailableVendorProduct.vendor;
            vendor.InventoryUpdateFrequency = "AlwaysInStock";
            vendor.AlwaysQuantity = 1;
            var availableVendorProduct = domain.AvailableVendorProduct;
            var remainingQty = domain.Quantity - availableVendorProduct.SafetyQty;

            //act
            var feed = Mapper.Map<MarketplaceInventoryFeed>(domain);

            // assert the vendor product properties
            Assert.Equal(availableVendorProduct.Quantity, feed.AmazonInventoryFeed.ProductQuantity);
            Assert.Equal(availableVendorProduct.Quantity, feed.eBayInventoryFeed.ProductQuantity);
            Assert.Equal(availableVendorProduct.Quantity, feed.BigCommerceInventoryFeed.ProductQuantity);
            Assert.Equal(availableVendorProduct.IsAlwaysInStock, feed.AmazonInventoryFeed.IsAlwaysInStock);
            Assert.Equal(availableVendorProduct.IsAlwaysInStock, feed.eBayInventoryFeed.IsAlwaysInStock);
            Assert.Equal(availableVendorProduct.AlwaysQuantity, feed.AmazonInventoryFeed.AlwaysQuantity);
            Assert.Equal(availableVendorProduct.AlwaysQuantity, feed.eBayInventoryFeed.AlwaysQuantity);
            Assert.Equal(domain.productamazon.SafetyQty, feed.AmazonInventoryFeed.SafetyQty);
            Assert.Equal(domain.productebay.SafetyQty, feed.eBayInventoryFeed.SafetyQty);
            
            // assert the inventory quantity
            Assert.Equal(feed.AmazonInventoryFeed.InventoryQuantity, 1);
            Assert.Equal(feed.eBayInventoryFeed.InventoryQuantity, 1);

            // assert the marketplace inventory feed
            Assert.Equal(domain.EisSKU, feed.EisSKU);
            Assert.Equal(domain.IsBlacklisted, feed.IsBlacklisted);
            Assert.NotNull(feed.AmazonInventoryFeed);
            Assert.NotNull(feed.eBayInventoryFeed);
            Assert.NotNull(feed.BigCommerceInventoryFeed);

            // assert the amazon inventory feed
            Assert.Equal(domain.productamazon.EisSKU, feed.AmazonInventoryFeed.SKU);
            Assert.Equal(domain.Quantity, feed.AmazonInventoryFeed.ProductQuantity);
            Assert.Equal(domain.productamazon.SafetyQty, feed.AmazonInventoryFeed.SafetyQty);
            Assert.Equal(domain.productamazon.LeadtimeShip, feed.AmazonInventoryFeed.LeadtimeShip);
            Assert.Equal(domain.productamazon.IsEnabled, feed.AmazonInventoryFeed.IsEnabled);

            // assert the eBay inventory feed
            Assert.Equal(domain.productebay.ItemId, feed.eBayInventoryFeed.ItemId);
            Assert.Equal(domain.Quantity, feed.eBayInventoryFeed.ProductQuantity);
            Assert.Equal(domain.productebay.ListingQuantity, feed.eBayInventoryFeed.ListingQuantity);
            Assert.Equal(domain.productebay.SafetyQty, feed.eBayInventoryFeed.SafetyQty);
            Assert.Equal(domain.productebay.StartPrice, feed.eBayInventoryFeed.StartPrice);
            //Assert.Equal(domain.productebay.ListingQuantity, feed.eBayInventoryFeed.InventoryQuantity);

            // assert the big commerce inventory feed
            Assert.Equal(domain.productbigcommerce.EisSKU, feed.BigCommerceInventoryFeed.SKU);
            Assert.Equal(domain.productbigcommerce.ProductId, feed.BigCommerceInventoryFeed.ProductId);
            Assert.Equal(domain.productbigcommerce.InventoryLevel, feed.BigCommerceInventoryFeed.InventoryLevel);
            Assert.Equal(domain.productbigcommerce.InventoryWarningLevel, feed.BigCommerceInventoryFeed.InventoryWarningLevel);
            Assert.Equal(domain.productbigcommerce.InventoryTracking, feed.BigCommerceInventoryFeed.InventoryTracking);
            Assert.Equal(domain.productbigcommerce.OrderQuantityMinimum, feed.BigCommerceInventoryFeed.OrderQuantityMinimum);
            Assert.Equal(domain.productbigcommerce.OrderQuantityMaximum, feed.BigCommerceInventoryFeed.OrderQuantityMaximum);
        }

    }
}
