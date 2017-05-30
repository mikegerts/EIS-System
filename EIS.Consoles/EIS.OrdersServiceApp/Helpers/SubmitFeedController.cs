using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AmazonWebServiceModels;
using EIS.OrdersServiceApp.Models;
using EIS.OrdersServiceApp.Repositories;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using EIS.Inventory.Shared.Models;

namespace EIS.OrdersServiceApp.Helpers
{
    /// <summary>
    /// Class that contains all of the logic for making submit calls to the Amazon market web service
    /// </summary>
    public class SubmitFeedController
    {
        private MarketplaceWebServiceClient _amazonCient;
        private string _marketplaceId;
        private string _merchantId;
        private string _submittedBy;
        private LoggerRepository _logger;

        public SubmitFeedController(MarketplaceWebServiceClient amazonClient,
            LoggerRepository logger,
            string marketplaceId,
            string merchantId,
            string submittedBy)
        {
            _amazonCient = amazonClient;
            _logger = logger;
            _marketplaceId = marketplaceId;
            _merchantId = merchantId;
            _submittedBy = submittedBy;
            SleepFirst = true;
        }

        /// <summary>
        /// Flag to sleep first the feed controller at first
        /// </summary>
        public bool SleepFirst { get; set; }

        /// <summary>
        /// Submit the feed and get the stream response with the specified XML file path and feed type
        /// </summary>
        /// <param name="xmlFileName">The full file path of the XML</param>
        /// <param name="feedType">The type of the feed</param>
        /// <returns>Returns the stream for XML</returns>
        public Stream SubmitFeedAndGetResponse(string xmlFileName, AmazonFeedType feedType)
        {
            var submitFeedRequest = createSubmitFeedRequest(xmlFileName, feedType);

            // send the feed request to the mws
            var submitFeedResponse = _amazonCient.SubmitFeed(submitFeedRequest);

            WaitForGetFeedSubmissionListToCompleted(submitFeedResponse.SubmitFeedResult.FeedSubmissionInfo.FeedSubmissionId);

            return GetFeedSubmissionResult(submitFeedResponse.SubmitFeedResult.FeedSubmissionInfo.FeedSubmissionId);
        }

        /// <summary>
        /// Create SubmitFeedRequest given an XML file path and feed type
        /// </summary>
        /// <param name="xmlFilePath">The xml full file path</param>
        /// <param name="feedType">The type of feed to be submitted</param>
        /// <returns></returns>
        private SubmitFeedRequest createSubmitFeedRequest(string xmlFilePath, AmazonFeedType feedType)
        {
            // create submit feed request
            var feedRequest = new SubmitFeedRequest
            {
                Merchant = _merchantId,
                MarketplaceIdList = new IdList { Id = new List<string> { _marketplaceId } },
                FeedType = feedType.ToString(),
                ContentType = new ContentType(MediaType.OctetStream),
                FeedContent = File.Open(xmlFilePath, FileMode.Open, FileAccess.ReadWrite)
            };

            feedRequest.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(feedRequest.FeedContent);

            return feedRequest;
        }

        /// <summary>
        /// Will continue to call GetFeedSubmissionList unitl it's completed or cancelled
        /// </summary>
        /// <param name="feedSubmissionId">The feed submission id</param>
        private void WaitForGetFeedSubmissionListToCompleted(string feedSubmissionId)
        {
            // create submission feed list request
            GetFeedSubmissionListResponse submissionListResponse = null;
            var isUpdate = false;
            var submissionListRequest = new GetFeedSubmissionListRequest
            {
                Merchant = _merchantId,
                FeedSubmissionIdList = new IdList { Id = { feedSubmissionId } }
            };

            do
            {
                Console.WriteLine("Checking the submitted feed status...");

                // check to see if it's the first time it's been called
                if (submissionListResponse != null)
                    // if not yet finished yet, sleep for 45 seconds. This is the restore rate for GetFeedSubmissionList
                    Thread.Sleep(45000);

                submissionListResponse = _amazonCient.GetFeedSubmissionList(submissionListRequest);
                parsedAndPersistSubmissionListResponse(submissionListResponse, ref isUpdate);

            } while (!submissionListResponse.GetFeedSubmissionListResult.FeedSubmissionInfo.First().FeedProcessingStatus.Equals("_CANCELED_")
                && !submissionListResponse.GetFeedSubmissionListResult.FeedSubmissionInfo.First().FeedProcessingStatus.Equals("_DONE_"));

            Console.WriteLine("Submitted feed status is DONE!");
        }

        /// <summary>
        /// Handles calling the GetFeedSubmissionResult method on Amazon's web service and returns the stream
        /// </summary>
        /// <param name="feedSubmissionId">The id of the feed submission</param>
        /// <returns>A stream of the XML result</returns>
        private Stream GetFeedSubmissionResult(string feedSubmissionId)
        {
            var stream = new MemoryStream();
            var feedSubmissionResultRequest = new GetFeedSubmissionResultRequest
            {
                Merchant = _merchantId,
                FeedSubmissionId = feedSubmissionId,
                FeedSubmissionResult = stream
            };

            // send the feed
            _amazonCient.GetFeedSubmissionResult(feedSubmissionResultRequest);

            return feedSubmissionResultRequest.FeedSubmissionResult;
        }

        /// <summary>
        /// Parsed and persist the submission list response to the database
        /// </summary>
        /// <param name="response">The submission list response to parsed and to save</param>
        /// <param name="isUpdate">Flag if it is to update or add the response to database</param>
        private void parsedAndPersistSubmissionListResponse(GetFeedSubmissionListResponse response, ref bool isUpdate)
        {
            if (!response.IsSetGetFeedSubmissionListResult())
                return;

            var submissionInfo = response.GetFeedSubmissionListResult.FeedSubmissionInfo.First();

            var requestReport = new MarketplaceRequestReport
            {
                RequestId = response.ResponseMetadata.RequestId,
                ReportRequestId = submissionInfo.FeedSubmissionId,
                FeedType = submissionInfo.FeedType,
                ProcessingStatus = submissionInfo.FeedProcessingStatus,
                StartDate = submissionInfo.StartedProcessingDate,
                EndDate = submissionInfo.CompletedProcessingDate,
                SubmittedDate = submissionInfo.SubmittedDate,
                SubmittedBy = _submittedBy
            };

            if (isUpdate)
                _logger.UpdateRequestReport(requestReport);
            else
            {
                _logger.AddRequestReport(requestReport);
                isUpdate = true;
            }
        }
    }
}
