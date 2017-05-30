using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EIS.MwsReportsApp.Helpers;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp.Marketplaces
{
    [Export(typeof(IMarketplaceReport))]
    public class AmazonMarktplaceReport : IMarketplaceReport
    {
        private readonly AmazonReportController _reportController;
        private readonly SettlementXmlFileParser _fileParser;

        public AmazonMarktplaceReport()
        {
            _reportController = new AmazonReportController();
            _fileParser = new SettlementXmlFileParser();
        }

        public string ChannelName
        {
            get { return "Amazon"; }
        }

        public SettlementReportDto GetSettlementReport(DateTime createdAfter, bool includeAcknowledged)
        {
            // create the Amazon report controller and get the file path
            var filePaths = _reportController.SubmitSettlementReportRequest(createdAfter, includeAcknowledged);
            if(!filePaths.Any())
            {
                Console.WriteLine("NO settlement report found from Amazon.");
                return null;
            }
            var settlementData = _fileParser.ParseXmlFiles(filePaths);

            // then let's delete the files
            _fileParser.DeleteXmlFiles(filePaths);

            return settlementData;
        }

        public void GetScheduledReports()
        {
            _reportController.GetReportScheduledList();
        }

        public void DoManageScheduledReport(string reportType, DateTime scheduledDate)
        {
            _reportController.ManageReportSchedule(reportType, scheduledDate);
        }
    }
}
