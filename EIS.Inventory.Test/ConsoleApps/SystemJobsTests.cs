using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Workers;
using System.Collections.Generic;

namespace EIS.Inventory.Test
{
    //[TestClass]
    public class UnitTest_SystemJobs {

        private EisInventoryContext _context;

       // [TestInitialize]
        public void InitializeVariables() {
            _context = new EisInventoryContext();
        }


       // [TestMethod]
        public void ShouldUploadBlacklistedSKU () {

            //Arrange
            var systemjobObject = new systemjob() {
                HasHeader = true,
                JobType = JobType.BlacklistedSkuUpload,
                Status = JobStatus.Pending,
                Parameters = "D:\\logs\\Supplier Files\\08092016-220541_CustomExportProducts.csv"
            };

//            var jobWorker = new BlacklistedSKUFileUploadWorker(systemjobObject);


            //Act
//            jobWorker.StartJob();


            //Assert


        }



        //[TestMethod]
        public void ShouldUploadShadowsSKU () {

            //Arrange
            var systemjobObject = new systemjob() {
                HasHeader = true,
                JobType = JobType.ShadowFileUpload,
                Status = JobStatus.Pending,
                Parameters = "D:\\logs\\Supplier Files\\Shadow_Upload_Template.csv"
            };

//            var jobWorker = new ShadowFileUploadWorker(systemjobObject);


            //Act
//            jobWorker.StartJob();


            //Assert


        }


        //[TestMethod]
        public void ShouldParseShadowsSKU () {

            //Arrange
            var filePath = "D:\\logs\\Supplier Files\\Shadow_Upload_Template.csv";
            var shadows = new List<Shadow>();


            //Act
            var result = CsvFileDataParser.ParsedShadowFile(filePath, shadows, true);


            //Assert
            //Assert.IsTrue(result.IsSucess);
            //Assert.IsTrue(!shadows.Select(o => o.FactorQuantity).Contains(0));
        }
    }
}
