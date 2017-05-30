using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Models;
using EIS.SchedulerTaskApp.Repositories;
using MarketplaceWebService;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.SchedulerTaskApp.Marketplaces
{
    public class AmazonProductInventory : IMarketplaceProductInventory
    {
        private MarketplaceWebServiceClient _amazonClient;
        private SubmitFeedController submitFeedController;
        private AmazonCredentialDto _credential;
        private readonly string _submittedBy;
        private readonly string _filePath;

        public AmazonProductInventory(string submittedBy, string filePath)
        {
            _submittedBy = submittedBy;
            _filePath = filePath;
        }

        public string ChannelName
        {
            get { return "Amazon"; }
        }

        public CredentialDto Credential
        {
            get { return _credential; }
            set
            {
                _credential = value as AmazonCredentialDto;
                RequestHelper.SetCredentials(_credential);

                // create configuratin to use US marketplace
                var config = new MarketplaceWebServiceConfig { ServiceURL = RequestHelper.ServiceUrl };
                config.SetUserAgentHeader("EIS Scheduler Task", "3.0", "C#");
                _amazonClient = new MarketplaceWebServiceClient(_credential.AccessKeyId, _credential.SecretKey, config);
                
                // init the submit controller
                submitFeedController = new SubmitFeedController(_amazonClient, _credential.MarketplaceId, _credential.MerchantId, _submittedBy);
            }
        }

        public void SubmitFlatFileInventoryFeedAsync(string filePath)
        {
            Task.Run(() =>
            {
                Console.WriteLine("{0} is sending inventory update flat file -> {1}", ChannelName, filePath);

                // submit the Inventory XML file to the marketplace
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(filePath, AmazonFeedType._POST_FLAT_FILE_PRICEANDQUANTITYONLY_UPDATE_DATA_);

                // wait and parsed the stream results
                // TODO: NEED TO SAVE THE RESPONSE STREAM TO A FILE
                //parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Inventory); 
            });
        }

        public void SubmitProductsInventoryFeed(List<MarketplaceInventoryFeed> inventoryFeeds)
        {
            // get the AmazonInventorFeed and that is also Enabled
            var inventoryItems = inventoryFeeds
                .Where(x => !x.IsBlacklisted
                    && x.AmazonInventoryFeed != null
                    && x.AmazonInventoryFeed.IsEnabled)
                .Select(x => x.AmazonInventoryFeed)
                .ToList();

            if (!inventoryItems.Any())
            {
                Console.WriteLine(string.Format("{0} - No products for inventory update! Products must be not blacklisted and Amazon enabled.}", ChannelName));
                return;
            }

            // create inventory flat file for inventory update
            var generatedFile = InventoryPriceExcelWriter.CreateAmazonFeedInventoryTextFile(inventoryItems, _filePath);

            // submit it
            SubmitFlatFileInventoryFeedAsync(generatedFile);
        }

        public void SubmitProductsInventoryFeed(List<MarketplaceInventoryUpdateItem> inventoryItems)
        {
            try
            {
                var totalBatch = Math.Ceiling(inventoryItems.Count / 500.0);
                Console.WriteLine("Processing {0} batches for Inventory Feed...", totalBatch);

                // iterate and send the inventory feed by bacthes (100) - https://www.amazon.com/gp/help/customer/display.html?nodeId=200325440
                for (var i = 0; i < totalBatch; i++)
                {
                    var batchedItems = inventoryItems.Skip(i * 500).Take(500).ToList();

                    // parse the EIS products and create Amazon envelope
                    var envelope = RequestHelper.CreateInventoryFeedEnvelope(batchedItems);

                    // parse the envelope into file
                    var xmlFullName = XmlParser.WriteXmlToFile(envelope, string.Format("AmazonInventoryFeed_{0}", i +1));

                    // create task to send the feed asynchronously
                    Task.Run(() =>
                    {
                        Console.WriteLine("Sending batch: {0} for inventory feed...", i + 1);

                        // submit the Inventory XML file to the marketplace
                        var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_INVENTORY_AVAILABILITY_DATA_);

                        // wait and parsed the stream results
                        parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Inventory);
                    });

                    // let's sleep for a minute
                    Thread.Sleep(60000);
                }

                Logger.LogInfo(LogEntryType.AmazonProductInventory, string.Format("Done submitting inventory feed to {0} - products affected: {1} items.",
                    ChannelName, inventoryItems.Count));
                Console.WriteLine(string.Format("Done submitting inventory feed to {0} - products affected: {1} items.", ChannelName, inventoryItems.Count));
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.AmazonProductInventory, string.Format("Error in submitting inventory feed for {0}. <br/>Error Message: {1}", ChannelName,
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                Console.WriteLine(string.Format("Error in submitting inventory feed for {0}. <br/>Error Message: {1}", ChannelName, EisHelper.GetExceptionMessage(ex)));
            }
        }


        #region conversion helpers

        /// <summary>
        /// Convert the SubmitFeedResponse stream for the post feed folow into a AmazonExeption if needed
        /// </summary>
        /// <param name="responseStream">The response stream object</param>
        /// <param name="messageType">The type of the message</param>
        /// <returns></returns>
        private void parsedResultStreamAndLogReport(Stream responseStream, AmazonEnvelopeMessageType messageType)
        {
            try
            {
                using (var stream = responseStream)
                {
                    // the result may not be an XML document. This will be reveled with testing.
                    loadXmlStream(stream, messageType.ToString());

                    using (var fileStream = File.Create(string.Format("D:\\logs\\resultfeed{0:yyyyMMdd_HHmmss}.txt", DateTime.Now)))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.AmazonProductInventory,
                    string.Format("Error in parsing {0} result response stream. <br/> Error Message: {1}", messageType.ToString(),
                    ex.InnerException != null ? string.Format("{0} <br/>Inner Message: {1}", ex.Message, ex.InnerException.Message) : ex.Message),
                    ex.StackTrace);
            }
        }

        private void loadXmlStream(Stream stream, string messageType)
        {
            var doc = new XmlDocument();
            doc.Load(stream);

            var report = doc.SelectSingleNode("/AmazonEnvelope/Message/ProcessingReport");
            var processingSummary = report.SelectSingleNode("ProcessingSummary");
            var processingReport = new MarketplaceProcessingReport
            {
                MerchantId = _credential.MerchantId,
                MessageType = messageType,
                TransactionId = report.SelectSingleNode("DocumentTransactionID").InnerText,
                MessagesProcessed = int.Parse(processingSummary.SelectSingleNode("MessagesProcessed").InnerText),
                MessagesSuccessful = int.Parse(processingSummary.SelectSingleNode("MessagesSuccessful").InnerText),
                MessagesWithError = int.Parse(processingSummary.SelectSingleNode("MessagesWithError").InnerText),
                MessagesWithWarning = int.Parse(processingSummary.SelectSingleNode("MessagesWithWarning").InnerText),
                StatusCode = report.SelectSingleNode("StatusCode").InnerText,
                SubmittedBy = _submittedBy
            };

            // parsed the any processing report results
            var results = report.SelectNodes("Result");
            var reportResults = new List<MarketplaceProcessingReportResult>();
            foreach (XmlNode result in results)
            {
                reportResults.Add(new MarketplaceProcessingReportResult
                {
                    TransactionId = processingReport.TransactionId,
                    MessageId = int.Parse(result.SelectSingleNode("MessageID").InnerText),
                    Code = result.SelectSingleNode("ResultCode").InnerText,
                    MessageCode = result.SelectSingleNode("ResultMessageCode").InnerText,
                    Description = result.SelectSingleNode("ResultDescription").InnerText,
                    AdditionalInfo = result.SelectSingleNode("AdditionalInfo/SKU") == null ? "" : result.SelectSingleNode("AdditionalInfo/SKU").InnerText
                });
            }

            // add it to the report summary
            processingReport.ReportResults = reportResults;

            // save it to the database
            Logger.AddProcessingReport(processingReport);
        }

        #endregion
    }
}
