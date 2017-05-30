using AutoMapper;
using EIS.Inventory.Core.Mappings;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using Ploeh.AutoFixture;
using System.Linq;
using Xunit;

namespace EIS.Inventory.Test.Mappings
{
    public class ServiceModelToDomainMappingTests
    {
        public ServiceModelToDomainMappingTests()
        {

            Mapper.Initialize(x =>
            {
                x.AddProfile<ViewModelToDomainMappingProfile>();
            });
        }
        
        [Fact]
        public void Should_Mapped_ProductBigCommerceDto_To_ProductBigCommerce()
        {
            // arrange
            var fixture = new Fixture();
            var viewModel = fixture.Create<ProductBigCommerceDto>();

            // act
            var domain = Mapper.Map<productbigcommerce>(viewModel);

            // assert
            Assert.Equal(domain.EisSKU, viewModel.EisSKU);
            Assert.Equal(domain.ProductId, viewModel.ProductId);
            Assert.Equal(domain.CategoryId, viewModel.CategoryId);
            Assert.Equal(domain.Condition, viewModel.Condition);
            Assert.Equal(domain.Categories, viewModel.Categories);
            Assert.Equal(domain.RetailPrice, viewModel.RetailPrice);
            Assert.Equal(domain.PrimaryImage, viewModel.PrimaryImage);
            Assert.Equal(domain.FixedCostShippingPrice, viewModel.FixedCostShippingPrice);
            Assert.Equal(domain.Brand, viewModel.Brand);
            Assert.Equal(domain.ProductsType, viewModel.ProductsType);
            Assert.Equal(domain.InventoryLevel, viewModel.InventoryLevel);
            Assert.Equal(domain.InventoryWarningLevel, viewModel.InventoryWarningLevel);
            Assert.Equal(domain.InventoryTracking, viewModel.InventoryTracking);
            Assert.Equal(domain.OrderQuantityMinimum, viewModel.OrderQuantityMinimum);
            Assert.Equal(domain.OrderQuantityMaximum, viewModel.OrderQuantityMaximum);
            Assert.Equal(domain.ModifiedBy, viewModel.ModifiedBy);
        }

        [Fact]
        public void Should_Mapped_Address_To_AddressDetails()
        {
            // arrange
            var fixture = new Fixture();
            var model = fixture.Create<Address>();

            // act
            var domain = Mapper.Map<addressdetail>(model);
            
            // assert
            Assert.Equal(domain.Line1, model.Line1);
            Assert.Equal(domain.Line2, model.Line2);
            Assert.Equal(domain.CountryCode, model.CountryCode);
            Assert.Equal(domain.City, model.City);
            Assert.Equal(domain.StateOrRegion, model.StateOrRegion);
            Assert.Equal(domain.PostalCode, model.PostalCode);
            Assert.Equal(domain.IsResidential, model.IsResidential);

        }

        [Fact]
        public void Should_Mapped_ShipFromLocationDto_To_ShippingLocation()
        {
            // var arrange 
            var fixture = new Fixture();
            var model = fixture.Create<ShippingLocationDto>();

            // act
            var domain = Mapper.Map<shippinglocation>(model);

            // assert
            Assert.Equal(domain.Id, model.Id);
            Assert.Equal(domain.Name, model.Name);
            Assert.Equal(domain.FromCompanyName, model.FromCompanyName);
            Assert.Equal(domain.FromPhone, model.FromPhone);
            Assert.Equal(domain.ReturnCompanyName, model.ReturnCompanyName);
            Assert.Equal(domain.ReturnPhone, model.ReturnPhone);
            Assert.Equal(domain.IsReturnSame, model.IsReturnSame);
            Assert.Equal(domain.IsDefault, model.IsDefault);

            Assert.NotNull(domain.FromAddressDetails);
            Assert.Equal(domain.FromAddressDetails.Line1, model.FromAddressDetails.Line1);
            Assert.Equal(domain.FromAddressDetails.Line2, model.FromAddressDetails.Line2);
            Assert.Equal(domain.FromAddressDetails.CountryCode, model.FromAddressDetails.CountryCode);
            Assert.Equal(domain.FromAddressDetails.City, model.FromAddressDetails.City);
            Assert.Equal(domain.FromAddressDetails.StateOrRegion, model.FromAddressDetails.StateOrRegion);
            Assert.Equal(domain.FromAddressDetails.PostalCode, model.FromAddressDetails.PostalCode);
            Assert.Equal(domain.FromAddressDetails.IsResidential, model.FromAddressDetails.IsResidential);

            Assert.NotNull(domain.ReturnAddressDetails);
            Assert.Equal(domain.ReturnAddressDetails.Line1, model.ReturnAddressDetails.Line1);
            Assert.Equal(domain.ReturnAddressDetails.Line2, model.ReturnAddressDetails.Line2);
            Assert.Equal(domain.ReturnAddressDetails.CountryCode, model.ReturnAddressDetails.CountryCode);
            Assert.Equal(domain.ReturnAddressDetails.City, model.ReturnAddressDetails.City);
            Assert.Equal(domain.ReturnAddressDetails.StateOrRegion, model.ReturnAddressDetails.StateOrRegion);
            Assert.Equal(domain.ReturnAddressDetails.PostalCode, model.ReturnAddressDetails.PostalCode);
            Assert.Equal(domain.ReturnAddressDetails.IsResidential, model.ReturnAddressDetails.IsResidential);

        }
    }
}
