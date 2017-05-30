using System;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Models;
using System.Linq;
using System.Diagnostics;
using Xunit;
using AutoMapper;
using EIS.Inventory.Core.Mappings;

namespace EIS.Inventory.Test {
    //[TestClass]
    public class ProductServiceTests
    {
        public ProductServiceTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
        }

        //[TestMethod]
        public void ShouldReturnProductsSubmitPriceFeed_AmazonIsEnabled () {

            ////Arrange
            //ProductService service = new ProductService(null, null);
            //MarketplaceFeed model = new MarketplaceFeed();
            //model.IsAllProductItems = true;


            ////Act
            //var result = service.GetProductInventoryFeed(model);

            ////Assert
            //Assert.IsTrue(result != null);

        }


        //[TestMethod]
        public void ShouldReturnProductsSubmitPriceFeed_NotBlacklisted () {

            //Arrange
            ProductService service = new ProductService(null, null);
            MarketplaceFeed param = new MarketplaceFeed();
            param.IsAllProductItems = true;
            param.ProductGroupId = -1;

            //Act
            var result = service.GetProductPostFeeds(param).ToList();


            //Assert
            Debug.WriteLine(result.Count);
            //Assert.IsTrue(result.Count > 0, "No Items Found.");

        }


        //[TestMethod]
        public void ShouldFilter_Amazon () {

            //Arrange
            ProductService service = new ProductService(null, null);
            //bool IsAmazonEnabled = true;

            //Act
            //var result = service.GetFilteredProducts("", -1, -1, -1, -1, false, null, null, IsAmazonEnabled).ToList();


            ////Assert
            //Debug.WriteLine(result.Count);
            //Assert.IsTrue(result.Count > 0, "No Items Found.");


        }
        

        //[TestMethod]
        public void ShouldFilter_ASIN () {

            //Arrange
            ProductService service = new ProductService(null, null);
            //var hasASIN = true;


            //Act
            //var result = service.GetFilteredProducts("", -1, -1, -1, false, null, null, null, hasASIN).ToList();


            ////Assert
            //Debug.WriteLine(result.Count);
            //Assert.IsTrue(result.Count > 0, "No Items Found.");

        }


       [Fact]
        public void ShouldFilter_ProductDataFeed_ASIN () {

            //Arrange
            ProductService service = new ProductService(null, null);
            MarketplaceFeed param = new MarketplaceFeed();
            param.HasASIN = true;

            //Act
            var result = service.GetProductPostFeeds(param).ToList();


            //Assert
            Debug.WriteLine(result.Count);
            Assert.True(result.Count > 0, "No Items Found.");

        }

        //[TestMethod]
        public void ShouldFilter_ProductDataFeed_Amazon () {

            //Arrange
            ProductService service = new ProductService(null, null);
            MarketplaceFeed param = new MarketplaceFeed();
            param.IsAmazonEnabled = true;

            //Act
            var result = service.GetProductPostFeeds(param).ToList();


            //Assert
            Debug.WriteLine(result.Count);
            //Assert.IsTrue(result.Count > 0, "No Items Found.");

        }

        //[TestMethod]
        public void ShouldFilter_CustomExport_Amazon () {

            //Arrange
            ExportDataService service = new ExportDataService(null, null);
            ExportProduct param = new ExportProduct();
            param.IsAmazonEnabled = true;
            param.IsAllProductItems = true;

            //Act
            var result = service.CustomExportProducts(param).ToList();


            //Assert
            Debug.WriteLine(result.Count);
            //Assert.IsTrue(result.Count > 0, "No Items Found.");

        }

        //[TestMethod]
        public void ShouldFilter_CustomExport_ASIN () {

            //Arrange
            ExportDataService service = new ExportDataService(null, null);
            ExportProduct param = new ExportProduct();
            param.HasASIN = true;
            param.IsAllProductItems = true;

            //Act
            var result = service.CustomExportProducts(param).ToList();


            //Assert
            Debug.WriteLine(result.Count);
           // Assert.IsTrue(result.Count > 0, "No Items Found.");

        }

    }
}
