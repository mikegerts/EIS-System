using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using EIS.Marketplace.Amazon.Services;
using EIS.Marketplace.Amazon.Services.Core;
using EIS.Marketplace.Amazon.Services.Models;

namespace EIS.Marketplace.Amazon.Helpers
{
    public class MarketplaceHelper
    {
        public static string ParseObjectToXML(AmazonEnvelope envelope)
        {
            string xmlBody;
            var objectType = envelope.GetType();

            var xmlSerializer = new XmlSerializer(objectType);
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, envelope);
                ms.Position = 0;

                var sr = new StreamReader(ms, Encoding.UTF8);
                xmlBody = sr.ReadToEnd();

                //xmlBody = getInnerXmlBody(xmlResult);
            }

            return xmlBody;
        }

        public static List<string> SendSubmitFeedRequest(SubmitFeedRequest request)
        {
            var results = new List<string>();
            var client = _getWebServiceClient();

            var uploadSuccess = false;
            var retryCount = 0;
            while (!uploadSuccess)
            {
                try
                {
                    var feedResponse = client.SubmitFeed(request);
                    var submissionId = feedResponse.SubmitFeedResult.FeedSubmissionInfo.FeedSubmissionId;
                    results.Add(submissionId);
                    uploadSuccess = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount == 3) 
                        break;

                    Thread.Sleep(18000);

                    if (ex.ToString().ToLowerInvariant().Contains("request is throttled")) 
                        continue;

                    results.Add(string.Format("ERROR: {0}", ex));
                }
            }

            return results;
        }

        public static GetFeedSubmissionListResponse GetFeedSubmissionListResult(List<string> resultIds)
        {
            // create the submit feed list request
            var request = RequestHelper.CreateFeedSubmissionListRequest(resultIds);

            var client = _getWebServiceClient();

            return client.GetFeedSubmissionList(request);
        }

        public static GetFeedSubmissionResultResponse GetFeedSubmissionResultReport(string feedSubmissionId)
        {
            // create reqeust for feed submission result report
            var request = RequestHelper.CreateFeedSubmissionResultRequest(feedSubmissionId);

            var client = _getWebServiceClient();

            return client.GetFeedSubmissionResult(request);
        }

        private static MarketplaceWebServiceClient _getWebServiceClient()
        {
            var feedConfig = new MarketplaceWebServiceConfig
            {
                ServiceURL = RequestHelper.ServiceUrl
            };

            return new MarketplaceWebServiceClient(RequestHelper.AccessKeyId,
                RequestHelper.SecretKey,
                "EIS System",
                "1.01",
                feedConfig);
        }

    }
}
