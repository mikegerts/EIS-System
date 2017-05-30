using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using EIS.Inventory.Shared.Models;

namespace EIS.MwsReportsApp.Helpers
{
    public class AmazonReportController
    {
        private const string _SETTLEMENT_REPORT_TYPE = "_GET_V2_SETTLEMENT_REPORT_DATA_XML_";
        private MarketplaceWebServiceClient _amazonClient;
        private readonly string _reportsDirectory;
        private readonly string _merchantId;
        private readonly string _accessKeyId;
        private readonly string _secretAccessKey;

        public AmazonReportController()
        {
            // init the reports directory
            _reportsDirectory = ConfigurationManager.AppSettings["ReportsPath"].ToString();
            _merchantId = ConfigurationManager.AppSettings["MerchantId"].ToString();
            _accessKeyId = ConfigurationManager.AppSettings["AccessKeyId"].ToString();
            _secretAccessKey = ConfigurationManager.AppSettings["SecretAccessKey"].ToString();

            // create configuratin to use US marketplace
            var config = new MarketplaceWebServiceConfig { ServiceURL = "https://mws.amazonservices.com" };
            config.SetUserAgentHeader("EIS Reports Service", "3.0", "C#");
            _amazonClient = new MarketplaceWebServiceClient(_accessKeyId, _secretAccessKey, config);
        }

        public List<string> SubmitSettlementReportRequest(DateTime createdAfter, bool includeAcknowledged)
        {
            var request = new GetReportListRequest
            {
                Merchant = _merchantId,
                AvailableFromDate = createdAfter,
                AvailableToDate = DateTime.Now,
                ReportTypeList = new TypeList { Type = new List<string> { _SETTLEMENT_REPORT_TYPE } }
            };

            var response = _amazonClient.GetReportList(request);
            var reportInfoList = response.GetReportListResult.ReportInfo;
            var nextToken = response.GetReportListResult.NextToken;
            var hasNext = response.GetReportListResult.HasNext;
            var reportCounter = 1;
            bool hasNextReport;
            var reportFilePaths = new List<string>();
            var reportIds = new List<string>();

            // pause for 2 seconds to give the Amazon MWS a little bit time to process the others
            Thread.Sleep(2000);

            do
            {
                hasNextReport = false;

                // download and parse the report info
                foreach (var reportInfo in reportInfoList)
                {
                    if (!includeAcknowledged && reportInfo.Acknowledged)
                        continue;

                    // check if the report request quota reach to 15
                    if (reportCounter % 15 == 0)
                        // if so, pause for 1 minute, this is the restore rate for GetReport
                        Thread.Sleep(60000);

                    var stream = downloadReportStream(reportInfo.ReportId);

                    // save the stream into a file
                    var filePath = saveStreamToFile(stream, string.Format("{0}{1}", reportInfo.ReportId, reportInfo.ReportType));

                    // add the filepath and report id to the lists
                    reportFilePaths.Add(filePath);
                    reportIds.Add(reportInfo.ReportId);

                    reportCounter++;
                }

                // send another request for the next report list
                if (hasNext)
                {
                    var nextRequest = new GetReportListByNextTokenRequest { Merchant = _merchantId, NextToken = nextToken };
                    var nextResponse = _amazonClient.GetReportListByNextToken(nextRequest);

                    reportInfoList = nextResponse.GetReportListByNextTokenResult.ReportInfo;
                    nextToken = nextResponse.GetReportListByNextTokenResult.NextToken;
                    hasNext = nextResponse.GetReportListByNextTokenResult.HasNext;
                    hasNextReport = reportInfoList.Any();
                }

            } while (hasNextReport);

            // then let's send the acknowledgment to the Reports Ids
            submitReportsAcknowledgement(reportIds);

            return reportFilePaths;
        }

        public bool ManageReportSchedule(string reportType, DateTime scheduledDate)
        {
            // create the manage report request
            var request = new ManageReportScheduleRequest
            {   
                Merchant = _merchantId,
                ReportType = reportType,
                ScheduleDate = scheduledDate,
                Schedule = ScheduleType._1_DAY_.ToString(),
            };

            // submit the request
            var response = _amazonClient.ManageReportSchedule(request);

            return true;
        }

        public void GetReportScheduledList()
        {
            // create the scheduled list request
            var request = new GetReportScheduleListRequest
            {
                Merchant = _merchantId,
                //ReportTypeList 
            };

            // submit the request
            var response = _amazonClient.GetReportScheduleList(request);
            var reportScheduleList = response.GetReportScheduleListResult.ReportSchedule;
            var nextToken = response.GetReportScheduleListResult.NextToken;
            var hasNext = response.GetReportScheduleListResult.HasNext;
            var reportCounter = 1;
            bool hasNextReport;

            do
            {
                hasNextReport = false;

                // download and parse the scheduled report
                foreach (var scheduledReport in reportScheduleList)
                {
                    // check if the report request quota reach to 15
                    if (reportCounter % 15 == 0)
                        // if so, pause for 1 minute, this is the restore rate for GetReport
                        Thread.Sleep(60000);

                    //var stream = downloadReportStream(scheduledReport.);

                    //// save the stream into a file
                    //var filePath = saveStreamToFile(stream, string.Format("{0}{1}", scheduledReport.ReportId, scheduledReport.ReportType));

                    //// add the filepath and report id to the lists
                    //reportFilePaths.Add(filePath);
                    //reportIds.Add(scheduledReport.ReportId);

                    reportCounter++;
                }

            } while (hasNextReport);
        }

        private void submitReportsAcknowledgement(List<string> reportIds)
        {
            // the max reports to be send is 10
            var totalBatches = Math.Ceiling(reportIds.Count / 10.0);
            for (var i = 0; i < totalBatches; i++)
            {
                // let's sleep for a while if there any batches left,
                // 45s is the restore rate for Reports for every 10 request
                if (i != 0)
                    Thread.Sleep(46000);

                var batchedIds = reportIds.Skip(i * 10).Take(10).ToList();
                
                // create the request
                var request = new UpdateReportAcknowledgementsRequest
                {
                    Merchant = _merchantId,
                    Acknowledged = true,
                    ReportIdList = new IdList { Id = batchedIds }
                };

                var response = _amazonClient.UpdateReportAcknowledgements(request);
            }
        }

        private Stream downloadReportStream(string repordId)
        {
            var errorCode = string.Empty;
            var reportStream = new MemoryStream();
            do
            {
                try
                {
                    // if there is an error, let's sleep for a while
                    if (!string.IsNullOrEmpty(errorCode))
                        Thread.Sleep(46000);

                    errorCode = string.Empty;
                    var request = new GetReportRequest
                    {
                        Merchant = _merchantId,
                        ReportId = repordId,

                        Report = reportStream
                    };

                    var response = _amazonClient.GetReport(request);
                }
                catch (MarketplaceWebServiceException ex)
                {
                    errorCode = ex.ErrorCode;
                    Console.WriteLine("Error encountered in downloading the Amazon report. \nError: {0} - Msg:{1}", ex.ErrorCode, ex.Message);
                }

            } while (!string.IsNullOrEmpty(errorCode));

            return reportStream;
        }

        private string saveStreamToFile(Stream stream, string fileName)
        {
            var filePath = string.Format("{0}\\{1}.xml", _reportsDirectory, fileName);
            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return filePath;
        }
    }
}
