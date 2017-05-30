using System;
using Xunit;
using AutoMapper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Models;
using EIS.Inventory.Shared.ViewModels;
using Ploeh.AutoFixture;

namespace EIS.Inventory.Test.Mappings
{
    public class SystemJobServiceAutoMappingTests
    {
        public SystemJobServiceAutoMappingTests()
        {
            SystemJobApp.Helpers.AutoMapperConfig.CreateMappings();
        }

        [Fact]
        public void Should_Mapped_SystemJob_To_Model()
        {
            // arrange
            var domain = new systemjob
            {
                Id = 11,
                Created = DateTime.Now,
                CurrentNumOfItems = 12,
                HasHeader = true,
                HasPostAction_1 = true,
                HasPostAction_2 = true,
                IsAddNewItem = true,
                IsNotified = true,
                JobType = JobType.BulkDeleteProduct,
                Parameters = "PARAMETERS",
                ParametersOut = "PARAMETERSOUT",
                Status = JobStatus.Failed,
                SubmittedBy = "SUBMITTED_BY",
                TotalNumOfItems = 13
            };

            // act
            var model = Mapper.Map<SystemJob>(domain);

            // assert
            Assert.NotNull(model);
            Assert.Equal(model.Id, domain.Id);
            Assert.Equal(model.CurrentNumOfItems, domain.CurrentNumOfItems);
            Assert.Equal(model.HasHeader, domain.HasHeader);
            Assert.Equal(model.HasPostAction_1, domain.HasPostAction_1);
            Assert.Equal(model.HasPostAction_2, domain.HasPostAction_2);
            Assert.Equal(model.IsAddNewItem, domain.IsAddNewItem);
            Assert.Equal(model.IsNotified, domain.IsNotified);
            Assert.Equal(model.JobType, domain.JobType);
            Assert.Equal(model.Parameters, domain.Parameters);
            Assert.Equal(model.ParametersOut, domain.ParametersOut);
            Assert.Equal(model.Status, domain.Status);
            Assert.Equal(model.SubmittedBy, domain.SubmittedBy);
            Assert.Equal(model.TotalNumOfItems, domain.TotalNumOfItems);
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
        public void Should_Mapped_ProducteBayDto_To_ProducteBay()
        {
            // arrange
            var fixture = new Fixture();
            var viewModel = fixture.Create<ProducteBayDto>();

            // act
            var domain = Mapper.Map<productebay>(viewModel);

            // arrange
            Assert.Equal(domain.EisSKU, viewModel.EisSKU);
            Assert.Equal(domain.ItemId, viewModel.ItemId);
            Assert.Equal(domain.CategoryId, viewModel.CategoryId);
            //Assert.Equal(domain.CategoryName, viewModel.CategoryName); - commented since it is just a helper property
            Assert.Equal(domain.Title, viewModel.Title);
            Assert.Equal(domain.SubTitle, viewModel.SubTitle);
            Assert.Equal(domain.Description, viewModel.Description);
            Assert.Equal(domain.ListingQuantity, viewModel.ListingQuantity);
            Assert.Equal(domain.SafetyQty, viewModel.SafetyQty);
            Assert.Equal(domain.StartPrice, viewModel.StartPrice);
            Assert.Equal(domain.ReservePrice, viewModel.ReservePrice);
            Assert.Equal(domain.BinPrice, viewModel.BinPrice);
            Assert.Equal(domain.ListType, viewModel.ListType);
            Assert.Equal(domain.Duration, viewModel.Duration);
            Assert.Equal(domain.Location, viewModel.Location);
            Assert.Equal(domain.Condition_, viewModel.Condition_);
            Assert.Equal(domain.DispatchTimeMax, viewModel.DispatchTimeMax);
            Assert.Equal(domain.IsOutOfStockListing, viewModel.IsOutOfStockListing);
            Assert.Equal(domain.IsBoldTitle, viewModel.IsBoldTitle);
            Assert.Equal(domain.IsRequireAutoPayment, viewModel.IsRequireAutoPayment);
            Assert.Equal(domain.IsEnabled, viewModel.IsEnabled);
        }

        [Fact]
        public void Should_Mapped_ProductAmazon_To_ProductAmazon()
        {
            // arrange
            var fixture = new Fixture();
            var viewModel = fixture.Create<ProductAmazon>();

            // act
            var domain = Mapper.Map<productamazon>(viewModel);

            // arrange
            Assert.Equal(domain.EisSKU, viewModel.EisSKU);
            Assert.Equal(domain.PackageQty, viewModel.PackageQty);
            Assert.Equal(domain.ProductGroup, viewModel.ProductGroup);
            Assert.Equal(domain.ProductTypeName, viewModel.ProductTypeName);
            Assert.Equal(domain.ASIN, viewModel.ASIN);
            Assert.Equal(domain.SafetyQty, viewModel.SafetyQty);
            Assert.Equal(domain.Price, viewModel.Price);
            Assert.Equal(domain.LeadtimeShip, viewModel.LeadtimeShip);
            Assert.Equal(domain.NumOfItems, viewModel.NumOfItems);
            Assert.Equal(domain.MaxOrderQty, viewModel.MaxOrderQty);
            Assert.Equal(domain.MapPrice, viewModel.MapPrice);
            Assert.Equal(domain.IsAllowGiftWrap, viewModel.IsAllowGiftWrap);
            Assert.Equal(domain.IsAllowGiftMsg, viewModel.IsAllowGiftMsg);
            Assert.Equal(domain.Condition, viewModel.Condition);
            Assert.Equal(domain.ConditionNote, viewModel.ConditionNote);
            Assert.Equal(domain.FulfilledBy, viewModel.FulfilledBy);
            Assert.Equal(domain.FbaSKU, viewModel.FbaSKU);
            Assert.Equal(domain.TaxCode, viewModel.TaxCode);
            //Assert.Equal(domain.WeightBox1, viewModel.WeightBox1);
            //Assert.Equal(domain.WeightBox1Unit, viewModel.WeightBox1Unit);
            //Assert.Equal(domain.WeightBox2, viewModel.WeightBox2);
            //Assert.Equal(domain.WeightBox2Unit, viewModel.WeightBox2Unit);
            Assert.Equal(domain.IsEnabled, viewModel.IsEnabled);
        }
    }
}
