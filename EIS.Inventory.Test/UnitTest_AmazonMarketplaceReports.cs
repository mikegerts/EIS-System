using EIS.Marketplace.Amazon;
using EIS.OrdersServiceApp;
using EIS.OrdersServiceApp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EIS.Inventory.Test
{
    [TestClass]
    public class UnitTest_AmazonMarketplaceReports
    {
        [TestMethod]
        public void GetAmazonReports_ShouldReturnReports()
        {
            var amazonReport = new AmazonMarketplaceReportProvider();
            var startDate = new DateTime(2016, 05, 01);
            var todayDate = DateTime.Now;
            amazonReport.GetReportList(startDate, todayDate);
        }

        [TestMethod]
        public void RequestReport_Test()
        {
            var reportType = "_GET_V2_SETTLEMENT_REPORT_DATA_XML_";
            var startDate = new DateTime(2015, 01, 01);
            var amazonReport = new AmazonMarketplaceReportProvider();
            var requestReportId = amazonReport.DoRequestReport(reportType, startDate);
        }

        [TestMethod]
        public void DoRequestReportList_Test()
        {
            var requestReportIds = new List<string> {
            "71267016970",
            "71268016970"
            };
            var amazonReport = new AmazonMarketplaceReportProvider();
            amazonReport.DoRequestReportList(requestReportIds);
        }

        [TestMethod]
        public void XmlFIleParser_Test()
        {
            var xmlFiles = Directory.GetFiles(@"D:\logs\reports");
            var filePaser = new SettlementXmlFileParser();
            var settlementList = filePaser.ParseXmlFiles(xmlFiles.ToList());

            //SettlementDataManager.InsertSettlementReportToDb(settlementList);
        }
    }
}
