using EIS.OrdersServiceApp.Marketplaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Test
{
    [TestClass]
    public class UnitTest_MwsReportsApp
    {
        [TestMethod]
        public void DoGetScheduledReport_Test()
        {
            var amazonReport = new AmazonMarktplaceReport();
            amazonReport.GetScheduledReports();
        }

        [TestMethod]
        public void DoManageScheduledReport_Test()
        {
            var amazonReport = new AmazonMarktplaceReport();
            amazonReport.DoManageScheduledReport("_GET_V2_SETTLEMENT_REPORT_DATA_XML_", DateTime.Now.AddDays(2));
        }
    }
}
