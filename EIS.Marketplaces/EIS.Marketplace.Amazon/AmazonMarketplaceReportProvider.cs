using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using EIS.Inventory.Core.MwsChannels;
using EIS.Marketplace.Amazon.Helpers;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Marketplace.Amazon
{
    [Export(typeof(IMarketplaceReportProvider))]
    public class AmazonMarketplaceReportProvider : IMarketplaceReportProvider
    {
        /// <summary>
        /// The local instance of the Amazon service client
        /// </summary>
        private MarketplaceWebServiceClient _amazonClient;
        private string[] _REPORT_TYPES = new string[] {
            "_GET_FBA_REIMBURSEMENTS_DATA_",
            "_GET_V2_SETTLEMENT_REPORT_DATA_XML_",
            "_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_REPLACEMENT_DATA_",
            "_GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_",
            "_GET_DATE_RANGE_FINANCIAL_TRANSACTION_DATA_",
            "_GET_FLAT_FILE_OFFAMAZONPAYMENTS_SETTLEMENT_DATA_",
            "_GET_FLAT_FILE_OFFAMAZONPAYMENTS_ORDER_REFERENCE_DATA_",
            "_GET_FLAT_FILE_OFFAMAZONPAYMENTS_AUTHORIZATION_DATA_",
            "_GET_FLAT_FILE_OFFAMAZONPAYMENTS_CAPTURE_DATA_",
            "_GET_FLAT_FILE_OFFAMAZONPAYMENTS_REFUND_DATA_"
        };

        public AmazonMarketplaceReportProvider()
        {
            // create configuratin to use US marketplace
            var config = new MarketplaceWebServiceConfig { ServiceURL = RequestHelper.ServiceUrl };
            config.SetUserAgentHeader("EIS Inventory System", "3.0", "C#");

            _amazonClient = new MarketplaceWebServiceClient("AKIAJDQNAJIEJ2XZWVQA",
                "iRJplr+w2vZ1felGmV/OuUqOSreEyAx6c7o8nF3J",
                config);
        }

        public string ChannelName
        {
            get { return "Amazon"; }
        }
        
        public CredentialDto MarketplaceCredential
        {
            set
            {
                var marketplaceCredential = value as AmazonCredentialDto;
                RequestHelper.SetCredentials(marketplaceCredential);
            }
        }

        public IEnumerable<object> GetReportList(DateTime startDate, DateTime endDate)
        {
            var request = new GetReportListRequest
            {
                Merchant = "A12HYFEDED6DEW",
                AvailableFromDate = startDate,
                AvailableToDate = endDate,
                ReportTypeList = new TypeList { Type = new List<string> { "_GET_V2_SETTLEMENT_REPORT_DATA_XML_"} }
            };
            
            var response = _amazonClient.GetReportList(request);
            var reportInfoList = response.GetReportListResult.ReportInfo;
            var nextToken = response.GetReportListResult.NextToken;
            var hasNext = response.GetReportListResult.HasNext;
            GetReportListByNextTokenResponse nextResponse = null;
            var reportCounter = 0;

            // pause for 2 seconds to give the Amazon MWS a little bit time to process the others
            Thread.Sleep(2000);

            // let's find the report we are interested in
            var reports = findReportTypes(reportInfoList);

            while (hasNext)
            {
                var nextRequest = new GetReportListByNextTokenRequest { Merchant = "A12HYFEDED6DEW", NextToken = nextToken };
                nextResponse = _amazonClient.GetReportListByNextToken(nextRequest);

                reportInfoList = nextResponse.GetReportListByNextTokenResult.ReportInfo;
                nextToken = nextResponse.GetReportListByNextTokenResult.NextToken;
                hasNext = nextResponse.GetReportListByNextTokenResult.HasNext;

                // pause for 2 seconds, this is the restore rate for GetReportListByNextToken
                Thread.Sleep(2000);

                // find and add it to the list
                reports.AddRange(findReportTypes(reportInfoList));
                foreach (var reportInfo in reports)
                {
                    // check if the report request quota reach to 15
                    if (reportCounter % 15 == 0)
                        // if so, pause for 1 minute, this is the restore rate for GetReport
                        Thread.Sleep(60000);

                    var stream = downloadReportStream(reportInfo.ReportId);

                    // save the stream into a file
                    saveStreamToFile(stream, string.Format("{0}_{1}", reportInfo.ReportType, reportInfo.ReportId));
                    reportCounter++;
                }

                // clear the reports lisst
                reports.Clear();
            }

            return null;
        }

        public string DoRequestReport(string reportType, DateTime startDate)
        {
            var request = new RequestReportRequest
            {
                Merchant = "A12HYFEDED6DEW",
                StartDate = startDate,
                EndDate = DateTime.Now.AddDays(-2),
                ReportType = reportType
            };

            try
            {
                var response = _amazonClient.RequestReport(request);
                var reportRequestInfo = response.RequestReportResult.ReportRequestInfo;

                return reportRequestInfo.ReportRequestId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DoRequestReportList(List<string> requestReportIds)
        {
            var request = new GetReportListRequest
            {
                Merchant = "A12HYFEDED6DEW",
                ReportRequestIdList = new IdList { Id = requestReportIds }
            };

            var response = _amazonClient.GetReportList(request);
            var reportInfoList = response.GetReportListResult.ReportInfo;

            foreach (var reportInfo in reportInfoList)
            {
                var stream = downloadReportStream(reportInfo.ReportId);

                // save the stream into a file
                saveStreamToFile(stream, string.Format("{0}_{1}", reportInfo.ReportType, reportInfo.ReportId));
            }
        }

        private List<ReportInfo> findReportTypes(List<ReportInfo> reportInfoList)
        {
            var reports = new List<ReportInfo>();
            foreach (var report in reportInfoList)
                if (_REPORT_TYPES.Contains(report.ReportType))
                    reports.Add(report);

            return reports;
        }

        private List<ReportInfo> getReportInfoList(List<string> reportRequestIds)
        {
            var request = new GetReportListRequest
            {
                Merchant = "A12HYFEDED6DEW",
                ReportRequestIdList = new IdList { Id = reportRequestIds }
            };

            var response = _amazonClient.GetReportList(request);

            return response.GetReportListResult.ReportInfo;
        }

        private Stream downloadReportStream(string repordId)
        {
            try
            {
                var reportStream = new MemoryStream();
                var request = new GetReportRequest
                {
                    Merchant = "A12HYFEDED6DEW",
                    ReportId = repordId,
                    
                    Report = reportStream
                };

                var response = _amazonClient.GetReport(request);
                return reportStream;
            }
            catch (MarketplaceWebServiceException)
            {
                return null;
            }
        }

        private void saveStreamToFile(Stream stream, string fileName)
        {
            using (var fileStream = File.Create(string.Format("D:\\logs\\{0}.xml", fileName)))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }
    }
}
