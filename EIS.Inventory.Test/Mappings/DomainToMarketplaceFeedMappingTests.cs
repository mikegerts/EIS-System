using System.Linq;
using Xunit;
using AutoMapper;
using EIS.Inventory.Core.Mappings;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using Ploeh.AutoFixture;

namespace EIS.Inventory.Test.Mappings
{
    public class DomainToMarketplaceFeedMappingTests
    {
        public DomainToMarketplaceFeedMappingTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
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
            Assert.Equal(domain.productebay.EisSKU, feed.eBayInventoryFeed.EisSKU);
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

        [Fact]
        public void Should_Mapped_Product_To_MarketplaceProductFeedDto()
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
            var productType = domain.producttype;
            var amazonDomain = domain.productamazon;
            var eBayDomain = domain.productebay;
            var bigCommerceDomain = domain.productbigcommerce;

            // act
            var productFeed = Mapper.Map<MarketplaceProductFeedDto>(domain);

            // assert the product type
            Assert.Equal(productType.Id, productFeed.ProductType.Id);
            Assert.Equal(productType.TypeName, productFeed.ProductType.TypeName);
            Assert.Equal(productType.AmazonMainCategoryCode, productFeed.ProductType.AmazonMainCategoryCode);
            Assert.Equal(productType.AmazonSubCategoryCode, productFeed.ProductType.AmazonSubCategoryCode);
            Assert.Equal(productType.EbayMainCategoryCode, productFeed.ProductType.EbayMainCategoryCode);
            Assert.Equal(productType.EbaySubCategoryCode, productFeed.ProductType.EbaySubCategoryCode);

            // assert the EIS/General product
            Assert.Equal(domain.EisSKU, productFeed.EisSKU);
            Assert.Equal(domain.ProductTypeId, productFeed.ProductTypeId);
            Assert.Equal(domain.Name, productFeed.Name);
            Assert.Equal(domain.Description, productFeed.Description);
            Assert.Equal(domain.Brand, productFeed.Brand);
            Assert.Equal(domain.UPC, productFeed.UPC);
            Assert.Equal(domain.PkgLength, productFeed.PkgLength);
            Assert.Equal(domain.PkgHeight, productFeed.PkgHeight);
            Assert.Equal(domain.PkgWidth, productFeed.PkgWidth);
            Assert.Equal(domain.PkgLenghtUnit, productFeed.PkgLenghtUnit);
            Assert.Equal(domain.PkgWeight, productFeed.PkgWeight);
            Assert.Equal(domain.PkgWeightUnit, productFeed.PkgWeightUnit);
            Assert.Equal(domain.ItemLength, productFeed.ItemLength);
            Assert.Equal(domain.ItemWidth, productFeed.ItemWidth);
            Assert.Equal(domain.ItemHeight, productFeed.ItemHeight);
            Assert.Equal(domain.ItemLenghtUnit, productFeed.ItemLenghtUnit);
            Assert.Equal(domain.ItemWeight, productFeed.ItemWeight);
            Assert.Equal(domain.ItemWeightUnit, productFeed.ItemWeightUnit);
            Assert.Equal(domain.IsBlacklisted, productFeed.IsBlacklisted);

            // assert the Product Amazon
            var amazonFeed = productFeed.AmazonProductFeed;
            Assert.Equal(amazonDomain.ASIN, amazonFeed.ASIN);
            Assert.Equal(amazonDomain.PackageQty, amazonFeed.PackageQty);
            Assert.Equal(amazonDomain.SafetyQty, amazonFeed.SafetyQty);
            Assert.Equal(amazonDomain.NumOfItems, amazonFeed.NumOfItems);
            Assert.Equal(amazonDomain.MaxOrderQty, amazonFeed.MaxOrderQty);
            Assert.Equal(amazonDomain.IsAllowGiftWrap, amazonFeed.IsAllowGiftWrap);
            Assert.Equal(amazonDomain.IsAllowGiftMsg, amazonFeed.IsAllowGiftMsg);
            Assert.Equal(amazonDomain.Condition, amazonFeed.Condition);
            Assert.Equal(amazonDomain.ConditionNote, amazonFeed.ConditionNote);
            Assert.Equal(amazonDomain.IsEnabled, amazonFeed.IsEnabled);
            Assert.Equal(amazonDomain.TaxCode, amazonFeed.TaxCode);
            Assert.Equal(amazonDomain.WeightBox1, amazonFeed.WeightBox1);
            Assert.Equal(amazonDomain.WeightBox1Unit, amazonFeed.WeightBox1Unit);
            Assert.Equal(amazonDomain.WeightBox2, amazonFeed.WeightBox2);
            Assert.Equal(amazonDomain.WeightBox2Unit, amazonFeed.WeightBox2Unit);

            // assert the Product eBay
            var eBayFeed = productFeed.eBayProductFeed;
            Assert.Equal(eBayDomain.ItemId, eBayFeed.ItemId);
            Assert.Equal(eBayDomain.CategoryId, eBayFeed.CategoryId);
            Assert.Equal(eBayDomain.Title, eBayFeed.Title);
            Assert.Equal(eBayDomain.SubTitle, eBayFeed.SubTitle);
            Assert.Equal(eBayDomain.Description, eBayFeed.Description);
            Assert.Equal(eBayDomain.ListType, eBayFeed.ListType);
            Assert.Equal(eBayDomain.Duration, eBayFeed.Duration);
            Assert.Equal(eBayDomain.Location, eBayFeed.Location);
            Assert.Equal(eBayDomain.Condition_, eBayFeed.Condition_);
            Assert.Equal(eBayDomain.DispatchTimeMax, eBayFeed.DispatchTimeMax);
            Assert.Equal(eBayDomain.IsOutOfStockListing, eBayFeed.IsOutOfStockListing);
            Assert.Equal(eBayDomain.IsBoldTitle, eBayFeed.IsBoldTitle);
            Assert.Equal(eBayDomain.IsRequireAutoPayment, eBayFeed.IsRequireAutoPayment);
            Assert.Equal(eBayDomain.IsEnabled, eBayFeed.IsEnabled);
            //Assert.Equal(eBayDomaiin.ReturnsAcceptedOption, eBayFeed.ReturnsAcceptedOption);
            //Assert.Equal(eBayDomaiin.ShippingCostPaidByOption, eBayFeed.ShippingCostPaidByOption);
            //Assert.Equal(eBayDomaiin.RefundOption, eBayFeed.RefundOption);
            //Assert.Equal(eBayDomaiin.ReturnsWithinOption, eBayFeed.ReturnsWithinOption);
            //Assert.Equal(eBayDomaiin.ReturnPolicyDescription, eBayFeed.ReturnPolicyDescription);
            //Assert.Equal(eBayDomaiin.ShippingType, eBayFeed.ShippingType);
            //Assert.Equal(eBayDomaiin.ShippingService, eBayFeed.ShippingService);
            //Assert.Equal(eBayDomaiin.ShippingServiceCost, eBayFeed.ShippingServiceCost);

            // asssert product BigCommerce
            var bigCommerceFeed = productFeed.BigCommerceProductFeed;
            Assert.Equal(bigCommerceDomain.EisSKU, bigCommerceFeed.EisSKU);
            Assert.Equal(bigCommerceDomain.ProductId, bigCommerceFeed.ProductId);
            Assert.Equal(bigCommerceDomain.Price, bigCommerceFeed.Price);
            Assert.Equal(bigCommerceDomain.RetailPrice, bigCommerceFeed.RetailPrice);
            Assert.Equal(bigCommerceDomain.FixedCostShippingPrice, bigCommerceFeed.FixedCostShippingPrice);
            Assert.Equal(bigCommerceDomain.Condition, bigCommerceFeed.Condition);
            Assert.Equal(bigCommerceDomain.Categories, bigCommerceFeed.Categories);
            Assert.Equal(bigCommerceDomain.Brand, bigCommerceFeed.Brand);
            Assert.Equal(bigCommerceDomain.ProductsType, bigCommerceFeed.ProductsType);
            Assert.Equal(bigCommerceDomain.CategoryId, bigCommerceFeed.CategoryId);
            Assert.Equal(bigCommerceDomain.InventoryLevel, bigCommerceFeed.InventoryLevel);
            Assert.Equal(bigCommerceDomain.InventoryWarningLevel, bigCommerceFeed.InventoryWarningLevel);
            Assert.Equal(bigCommerceDomain.InventoryTracking, bigCommerceFeed.InventoryTracking);
            Assert.Equal(bigCommerceDomain.OrderQuantityMinimum, bigCommerceFeed.OrderQuantityMinimum);
            Assert.Equal(bigCommerceDomain.OrderQuantityMaximum, bigCommerceFeed.OrderQuantityMaximum);
            Assert.Equal(domain.Quantity, bigCommerceFeed.ProductQuantity);

        }

        [Fact]
        public void Should_Mapped_VendorProduct_To_VendorProductDto()
        {
            // arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<vendorproduct>();

            // act
            var viewModel = Mapper.Map<VendorProductDto>(domain);

            // arrange
            Assert.Equal(viewModel.EisSupplierSKU, domain.EisSupplierSKU);
            Assert.Equal(viewModel.SupplierSKU, domain.SupplierSKU);
            Assert.Equal(viewModel.VendorId, domain.VendorId);
            Assert.Equal(viewModel.VendorName, domain.VendorName);
            Assert.Equal(viewModel.CompanyName, domain.CompanyName);
            Assert.Equal(viewModel.Name, domain.Name);
            Assert.Equal(viewModel.Description, domain.Description);
            Assert.Equal(viewModel.SupplierPrice, domain.SupplierPrice);
            Assert.Equal(viewModel.Quantity, domain.Quantity);
            Assert.Equal(viewModel.MinPack, domain.MinPack);
            Assert.Equal(viewModel.UPC, domain.UPC);
            Assert.Equal(viewModel.Category, domain.Category);
            Assert.Equal(viewModel.Weight, domain.Weight);
            Assert.Equal(viewModel.WeightUnit, domain.WeightUnit);
            Assert.Equal(viewModel.Shipping, domain.Shipping);
            Assert.Equal(viewModel.VendorMOQ, domain.VendorMOQ);
            Assert.Equal(viewModel.VendorMOQType, domain.VendorMOQType);
            Assert.Equal(viewModel.ModifiedBy, domain.ModifiedBy);
        }

        [Fact]
        public void Should_Mapped_orderitems_MarketplaceOrderItem()
        {
            // arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<orderitem>();

            // act
            var viewModel = Mapper.Map<MarketplaceOrderItem>(domain);

            // assert
            Assert.Equal(viewModel.OrderId, domain.OrderId);
            Assert.Equal(viewModel.OrderItemId, domain.OrderItemId);
            Assert.Equal(viewModel.MarketplaceItemId, domain.ItemId);
            Assert.Equal(viewModel.SKU, domain.SKU);
            Assert.Equal(viewModel.Title, domain.Title);
            Assert.Equal(viewModel.QtyOrdered, domain.QtyOrdered);
            Assert.Equal(viewModel.QtyShipped, domain.QtyShipped);
            Assert.Equal(viewModel.Price, domain.Price);
            Assert.Equal(viewModel.ShippingPrice, domain.ShippingPrice);
            Assert.Equal(viewModel.GiftWrapPrice, domain.GiftWrapPrice);
            Assert.Equal(viewModel.Tax, domain.ItemTax);
            Assert.Equal(viewModel.ShippingTax, domain.ShippingTax);
            Assert.Equal(viewModel.GiftWrapTax, domain.GiftWrapTax);
            Assert.Equal(viewModel.ShippingDiscount, domain.ShippingDiscount);
            Assert.Equal(viewModel.PromotionDiscount, domain.PromotionDiscount);
            Assert.Equal(viewModel.ConditionNote, domain.ConditionNote);
        }

        [Fact]
        public void Should_Mapped_Order_To_OrderProductListDto()
        {
            // arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<order>();

            // act
            var viewModel = Mapper.Map<OrderProductListDto>(domain);

            // assert
            Assert.Equal(viewModel.EisOrderId, domain.EisOrderId);
            Assert.Equal(viewModel.OrderId, domain.OrderId);
            Assert.Equal(viewModel.ItemSKU, domain.OrderProductItemSKU);
            Assert.Equal(viewModel.ItemName, domain.OrderProductItemName);
            Assert.Equal(viewModel.Quantity, domain.OrderProductQuantity);
            Assert.Equal(viewModel.BuyerName, domain.ShippingAddressName);
        }

        [Fact]
        public void Should_Mapped_AddressDetails_To_Address()
        {
            // var arrange 
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<addressdetail>();

            // act
            var viewModel = Mapper.Map<Address>(domain);

            // assert
            Assert.Equal(viewModel.Line1, domain.Line1);
            Assert.Equal(viewModel.Line2, domain.Line2);
            Assert.Equal(viewModel.CountryCode, domain.CountryCode);
            Assert.Equal(viewModel.City, domain.City);
            Assert.Equal(viewModel.StateOrRegion, domain.StateOrRegion);
            Assert.Equal(viewModel.PostalCode, domain.PostalCode);
            Assert.Equal(viewModel.IsResidential, domain.IsResidential);
        }

        [Fact]
        public void Should_Mapped_ShippingLocations_To_ShipFromLocationDto()
        {
            // var arrange 
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<shippinglocation>();

            // act
            var viewModel = Mapper.Map<ShippingLocationDto>(domain);

            // assert
            Assert.Equal(viewModel.Id, domain.Id);
            Assert.Equal(viewModel.Name, domain.Name);
            Assert.Equal(viewModel.FromCompanyName, domain.FromCompanyName);
            Assert.Equal(viewModel.FromPhone, domain.FromPhone);
            Assert.Equal(viewModel.ReturnCompanyName, domain.ReturnCompanyName);
            Assert.Equal(viewModel.ReturnPhone, domain.ReturnPhone);
            Assert.Equal(viewModel.IsReturnSame, domain.IsReturnSame);
            Assert.Equal(viewModel.IsDefault, domain.IsDefault);

            Assert.NotNull(viewModel.FromAddressDetails);
            Assert.Equal(viewModel.FromAddressDetails.Line1, domain.FromAddressDetails.Line1);
            Assert.Equal(viewModel.FromAddressDetails.Line2, domain.FromAddressDetails.Line2);
            Assert.Equal(viewModel.FromAddressDetails.CountryCode, domain.FromAddressDetails.CountryCode);
            Assert.Equal(viewModel.FromAddressDetails.City, domain.FromAddressDetails.City);
            Assert.Equal(viewModel.FromAddressDetails.StateOrRegion, domain.FromAddressDetails.StateOrRegion);
            Assert.Equal(viewModel.FromAddressDetails.PostalCode, domain.FromAddressDetails.PostalCode);
            Assert.Equal(viewModel.FromAddressDetails.IsResidential, domain.FromAddressDetails.IsResidential);
            
            Assert.NotNull(viewModel.ReturnAddressDetails);
            Assert.Equal(viewModel.ReturnAddressDetails.Line1, domain.ReturnAddressDetails.Line1);
            Assert.Equal(viewModel.ReturnAddressDetails.Line2, domain.ReturnAddressDetails.Line2);
            Assert.Equal(viewModel.ReturnAddressDetails.CountryCode, domain.ReturnAddressDetails.CountryCode);
            Assert.Equal(viewModel.ReturnAddressDetails.City, domain.ReturnAddressDetails.City);
            Assert.Equal(viewModel.ReturnAddressDetails.StateOrRegion, domain.ReturnAddressDetails.StateOrRegion);
            Assert.Equal(viewModel.ReturnAddressDetails.PostalCode, domain.ReturnAddressDetails.PostalCode);
            Assert.Equal(viewModel.ReturnAddressDetails.IsResidential, domain.ReturnAddressDetails.IsResidential);
        }

        [Fact]
        public void Should_Mapped_OrderProduct_To_OrderProductDto()
        {
            // var arrange 
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<orderproduct>();

            // act
            var viewModel = Mapper.Map<OrderProductDto>(domain);

            // assert
            Assert.Equal(viewModel.Id, domain.Id);
            Assert.Equal(viewModel.OrderItemId, domain.OrderItemId);
            Assert.Equal(viewModel.EisSupplierSKU, domain.EisSupplierSKU);
            Assert.Equal(viewModel.VendorProductName, domain.vendorproduct.Name);
            Assert.Equal(viewModel.Quantity, domain.Quantity);
            Assert.Equal(viewModel.Pack, domain.Pack);
            Assert.Equal(viewModel.IsExported, domain.IsExported);
            Assert.Equal(viewModel.IsPoGenerated, domain.IsPoGenerated);
            Assert.Equal(viewModel.ExportedDate, domain.ExportedDate);
            Assert.Equal(viewModel.PoGeneratedDate, domain.PoGeneratedDate);

        }

        [Fact]
        public void Should_Mapped_Order_To_OrderProductDetailDto()
        {
            // var arrange 
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var domain = fixture.Create<order>();

            // act
            var viewModel = Mapper.Map<OrderProductDetailDto>(domain);

            // assert
            Assert.Equal(viewModel.EisOrderId, domain.EisOrderId);
            Assert.Equal(viewModel.OrderId, domain.OrderId);
            Assert.Equal(viewModel.Store, domain.Marketplace);
            Assert.Equal(viewModel.PurchaseDate, domain.PurchaseDate);
            Assert.Equal(viewModel.BuyerName, domain.BuyerName);
            Assert.Equal(viewModel.BuyerEmail, domain.BuyerEmail);
            Assert.Equal(viewModel.ShippingAddressPhone, domain.ShippingAddressPhone);
            Assert.Equal(viewModel.ShippingAddressName, domain.ShippingAddressName);
            Assert.Equal(viewModel.ShippingAddressLine1, domain.ShippingAddressLine1);
            Assert.Equal(viewModel.ShippingAddressLine2, domain.ShippingAddressLine2);
            Assert.Equal(viewModel.ShippingCity, domain.ShippingCity);
            Assert.Equal(viewModel.ShippingStateOrRegion, domain.ShippingStateOrRegion);
            Assert.Equal(viewModel.ShippingPostalCode, domain.ShippingPostalCode);
            Assert.Equal(viewModel.OrderNote, domain.OrderNote);

            var domainOrderProducts = domain.OrderProducts.ToList();
            var orderProductModels = viewModel.OrderProducts;

            Assert.Equal(orderProductModels[0].Id, domainOrderProducts[0].Id);
            Assert.Equal(orderProductModels[0].OrderItemId, domainOrderProducts[0].OrderItemId);
            Assert.Equal(orderProductModels[0].EisSupplierSKU, domainOrderProducts[0].EisSupplierSKU);
            Assert.Equal(orderProductModels[0].VendorProductName, domainOrderProducts[0].vendorproduct.Name);
            Assert.Equal(orderProductModels[0].Quantity, domainOrderProducts[0].Quantity);
            Assert.Equal(orderProductModels[0].Pack, domainOrderProducts[0].Pack);
            Assert.Equal(orderProductModels[0].IsExported, domainOrderProducts[0].IsExported);
            Assert.Equal(orderProductModels[0].IsPoGenerated, domainOrderProducts[0].IsPoGenerated);
            Assert.Equal(orderProductModels[0].ExportedDate, domainOrderProducts[0].ExportedDate);
            Assert.Equal(orderProductModels[0].PoGeneratedDate, domainOrderProducts[0].PoGeneratedDate);
        }
    }
}
