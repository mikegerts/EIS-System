using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.OrdersServiceApp.ShippingServices;
using EIS.Inventory.Core.Services;
using System.Text;
using AutoMapper;
using EIS.Inventory.Core.Mappings;
using Newtonsoft.Json.Linq;
using Xunit;
using System.Data;

namespace EIS.Inventory.Test
{
    public class ShipStationServiceTests
    {
        
        //[TestInitialize]
        public void InitializeVariables()
        {
            AutoMapperConfig.RegisterAutoMappers();

        }

        [Fact]
        public void ShouldPostOrdersToShipStation()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<int> result_task;


            //Act
            result_task = shippingStationOrder.PostOrderToShippingStation();
            result_task.Wait();
            var result = result_task.Result;

            //Assert
            //Assert.IsTrue(result > 0);

        }


        [Fact]
        public void ShouldGetUnshippedOrders()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();


            //Act
            var result = shippingStationOrder.GetUnshippedOrders();

            //Assert
            Assert.True(result.Count > 0);

        }


        [Fact]
        public void ShouldDeleteOrders()
        {
            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            var orderNumber = "test";


            //Act
            var resultTask = shippingStationOrder.DeleteOrderByOrderNumber(orderNumber);
            resultTask.Wait();
            var result = resultTask.Result;

            //Assert
            Assert.True(result);

        }

        public List<order> GetUnshippedOrders()
        {

            EisInventoryContext _context;
            _context = new EisInventoryContext();

            List<order> unshippedOrderList = _context.orders.Where(o => o.OrderStatus == EIS.Inventory.Shared.Models.OrderStatus.Unshipped).ToList();

            List<int> existingshippingorders = _context.shippingstationtrackings.Select(o => o.EisOrderId).ToList();

            // filter unshipped orders that were not yet posted to the shipstation system
            unshippedOrderList = unshippedOrderList.Where(o => !existingshippingorders.Contains(o.EisOrderId)).ToList();

            return unshippedOrderList;
        }



        //[TestMethod]
        public void ShouldGetOrderFromShipStation()
        {
            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<List<ShipStationOrdersViewModel>> result_task;
            List<ShipStationOrdersViewModel> result = null;

            //Act
            result_task = shippingStationOrder.GetOrders();

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result.Count > 0);

        }


        [Fact]
        public void ShouldGetOrderFromShipStationByOrderNumber()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            var orderNumber = "1005028";
            Task<ShipStationOrdersViewModel> result_task;
            ShipStationOrdersViewModel result = null;

            //Act
            result_task = shippingStationOrder.GetOrderByNumber(orderNumber);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result != null);

        }


        //[TestMethod]
        public void ShouldGetShipmentFromShipStationByOrderNumber()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            var orderNumber = "	4872157";
            Task<ShipStationShipmentViewModel> result_task;
            ShipStationShipmentViewModel result = null;

            //Act
            result_task = shippingStationOrder.GetShipmentsByOrderNumber(orderNumber);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result != null);

        }


        [Fact]
        public void ShouldGetShipmentFromShipStation()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<List<ShipStationShipmentViewModel>> result_task;
            List<ShipStationShipmentViewModel> result = new List<ShipStationShipmentViewModel>();

            //Act
            result_task = shippingStationOrder.GetShipments();

            result_task.Wait();

            result = result_task.Result;

            //Assert
            Assert.True(result.Count > 0);

        }


        [Fact]
        public void ShouldPostShipmentFromShipStationByOrderNumber()
        {
            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<int> result_task;
            int result = 0;
            var orderNumber = "114-9161886-9469832";

            //Act
            result_task = shippingStationOrder.PostTrackingNumberAndCostByOrderNumber(orderNumber);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            Assert.True(result > 0);

        }


        //[TestMethod]
        public void ShouldSetShadowSKUQuantity()
        {
            //_context = new EisInventoryContext();

            ////Arrange
            //var shippingStationOrder = new ShippingStationOrders();
            //var orderObject = _context.orders.Where(o => o.OrderId == "114-1243411-9896225").DefaultIfEmpty(null).FirstOrDefault();


            //var orderItems = orderObject.orderitems;

            ////Act
            //shippingStationOrder.SetSpecialQuantity(ref orderItems); //change this to public in ShipStationOrders.cs to test


            ////Assert
            //Assert.IsTrue(orderItems == orderObject.orderitems);

        }


        [Fact]
        public void ShouldGetTrackingNumberandCost()
        {
            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<int> resultTask;

            //Act
            resultTask = shippingStationOrder.PostTrackingNumberAndCost();
            resultTask.Wait();

            var result = resultTask.Result;


            //Assert
            Assert.True(result > 0);

        }


        //Change GetImageURl method to public to test
        //[TestMethod]
        public void ShouldGetImageURL()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            var EISKU = "MI7657226";

            //Act
            var result = shippingStationOrder.GetImageURLPublic(EISKU);


            //Assert
            //Assert.IsTrue(result != "");

        }


        //[TestMethod]
        public void ShouldGetShipmentFromShipStationByDate()
        {

            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<List<ShipStationShipmentViewModel>> result_task;
            List<ShipStationShipmentViewModel> result = new List<ShipStationShipmentViewModel>();
            var lastDate = Convert.ToDateTime("09/01/2016");

            //Act
            result_task = shippingStationOrder.GetShipmentsByDate(lastDate);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result.Count > 0);

        }

        //[TestMethod]
        public void ShouldWriteToFile()
        {
            //Arrange
            var filePath = ConfigurationManager.AppSettings["ServiceLogPath"];
            var fileName = "ShipStation_Logs";
            var serviceMethod = "TestServiceMethod";
            var logText = "Texgdf  fsdsdsdf s fsdf";

            //Act
            LogToFile.CreateLog(serviceMethod, logText, filePath, fileName);

            //Asert
            //Assert.IsTrue(File.Exists(filePath + @"\" + fileName + ".txt"), "File was not created.");
        }

        //[TestMethod]
        public void ShouldCreateShipmentLabelByOrderNumber()
        {
            // Arrange
            var shippingStationOrder = new ShipStationService();
            Task<string> result_task;
            string result = "";
            var orderNumber = "112-7461785-3785061";

            //Act
            result_task = shippingStationOrder.CreateShipmentLabel(orderNumber);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result != "");
        }


        [Fact]
        public void ShouldPostOrderToShipStationByOrderNumber()
        {
            //Arrange
            var shippingStationOrder = new ShipStationOrders();
            Task<int> result_task;
            int result = 0;
            var orderNumber = "1006539";

            //Act
            result_task = shippingStationOrder.PostOrderToShippingStationByOrderNumber(orderNumber);

            result_task.Wait();

            result = result_task.Result;

            //Assert
            //Assert.IsTrue(result > 0);

        }

        //[TestMethod]
        public void ShouldDecodePDFString()
        {
            // Arrange
            string encodedString = @"";

            // Act
            var result = StringUtility.Base64DecodeString(encodedString);

            //File.WriteAllBytes("D:/test.pdf", StringUtility.Base64Decode(encodedString));

            // Assert
            //Assert.IsTrue(result != null, "String cannot be decoded.");

        }


        [Fact]
        public void ShouldFilterErroneousData()
        {
            //// Arrange
            //var shippingStationOrder = new ShipStationService();
            //var responseData = @"{'hasErrors':true,
            //                      'results': [{'orderId':0,'orderNumber':'113-9707985-4732217','orderKey':null,'success':false,'errorMessage':'Invalid shipTo address'},
            //                                  {'orderId':262566361,'orderNumber':'113-9713381-7000225','orderKey':null,'success':true,'errorMessage':null}]}";
            //JObject json = JObject.Parse(responseData);

            //// Act
            //var result = shippingStationOrder.FilterErrorDataAndLogEmail(json, "PostOrderToShipStation Erroneous Data");

            //// Assert
            //Assert.True(result, "Erroneous Data not filtered.");
        }


        [Fact]
        public void ShouldUpdateParentMeanWeight()
        {
            //// Arrange
            //EisInventoryContext _context;
            //_context = new EisInventoryContext();

            //var shipStationService = new ShipStationService();
            //var productObject = _context.products.FirstOrDefault(o => o.EisSKU == "BS1003936");
            //decimal accurateWeight = 9;
            //var unit = "ounces";

            //// Act
            //shipStationService.UpdateShadowProductsWeight(true, productObject, accurateWeight, unit);


            //// Assert

        }


        [Fact]
        public void ShouldUpdateShadowWeight()
        {
            //// Arrange
            //EisInventoryContext _context;
            //_context = new EisInventoryContext();

            //var shipStationService = new ShipStationService();
            //var productObject = _context.products.FirstOrDefault(o => o.EisSKU == "BS1003936_9");
            //decimal accurateWeight = 5;
            //var unit = "ounces";

            //// Act
            //shipStationService.UpdateShadowProductsWeight(false, productObject, accurateWeight, unit);


            //// Assert

        }
    }
}
