using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using EIS.Inventory.Core;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Marketplace.Amazon.Helpers;
using MarketplaceWebService;
using AmazonWebServiceModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Marketplace.Amazon
{
    [Export(typeof(IMarketplaceInventoryProvider))]
    public class AmazonMarketplaceInventoryProvider : IMarketplaceInventoryProvider
    {
        /// <summary>
        /// The local instance of the Amazon service client
        /// </summary>
        private MarketplaceWebServiceClient _amazonClient;
        private AmazonCredentialDto _credential;
        private ILogService _logger;

        public AmazonMarketplaceInventoryProvider()
        {
            _logger = Core.Get<ILogService>();
        }

        public AmazonMarketplaceInventoryProvider(string filePath)
        {
            _logger = new LogService();
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
                config.SetUserAgentHeader("EIS Inventory System", "3.0", "C#");
                _amazonClient = new MarketplaceWebServiceClient(_credential.AccessKeyId, _credential.SecretKey, config);
            }
        }

        public void SubmitProductsListingFeed(List<MarketplaceProductFeedDto> productPostFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has no product type id or no information for Amazon
                var invalidProducts = productPostFeeds
                     .Where(x => x.IsBlacklisted || x.AmazonProductFeed == null || x.ProductTypeId == null)
                    .ToList();
                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.AmazonListing, string.Format("{0}/{1} EIS products which will not be included to Amazon product listing feed due to no product type ID or no Amazon information or black listed.",
                        invalidProducts.Count, productPostFeeds.Count));
                    productPostFeeds.RemoveAll(x => x.IsBlacklisted || x.AmazonProductFeed == null || x.ProductTypeId == null);
                }

                // create the Amazon envelope for the products
                var envelope = RequestHelper.CreateProductsFeedEnvelope(productPostFeeds, AmazonEnvelopeMessageOperationType.Update);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonProductsListingFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient, _logger, _credential.MarketplaceId, _credential.MerchantId, submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Product, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonListing, string.Format("{0} - Successfully posted product listing feed for {1} product items. \nRequested by: {2}",
                    ChannelName, productPostFeeds.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product listing feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex), 
                        submittedBy,
                        productPostFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonListing, description, ex.StackTrace);
            }
        }

        /// <summary>
        /// Update updates every field. That could change some fields that are not blank to blank 
        /// if you did not put a value in that field. Blank values are NOT ignored.
        /// </summary>
        /// <param name="productFeed"></param>
        /// <param name="submittedBy"></param>
        public void SubmitSingleProductListingFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            try
            {
                // create Amazon envelope object for the single eisProduct
                var envelope = RequestHelper.CreateSingleProductFeedEnvelope(productFeed, AmazonEnvelopeMessageOperationType.Update);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonSingleProductListingFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient, 
                    _logger,
                    _credential.MarketplaceId,
                    _credential.MerchantId,
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Product, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonListing, string.Format("{0} - Successfully posted single product feed for EisSKU \'{1}\'.\nRequested by: {2}",
                    ChannelName, productFeed.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonListing, description, ex.StackTrace);

            }
        }

        /// <summary>
        /// Partial Update allows you to only include the fields 
        /// that you want to change/add along with the key fields (like SKU, ASIN/UPC) to identify the items. 
        /// Blank values are ignored.
        /// </summary>
        /// <param name="productPostFeed"></param>
        /// <param name="submittedBy"></param>
        public void SubmitProductsReviseFeed(List<MarketplaceProductFeedDto> productPostFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has no product type id or no information for Amazon
                var invalidProducts = productPostFeeds
                     .Where(x => x.IsBlacklisted || x.AmazonProductFeed == null || x.ProductTypeId == null)
                    .ToList();
                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.AmazonReviseItem, string.Format("{0}/{1} EIS products which will not be included to Amazon product revise feed due to no product type ID or no Amazon information or black listed.",
                        invalidProducts.Count, productPostFeeds.Count));
                    productPostFeeds.RemoveAll(x => x.IsBlacklisted || x.AmazonProductFeed == null || x.ProductTypeId == null);
                }

                // create the Amazon envelope for the products
                var envelope = RequestHelper.CreateProductsFeedEnvelope(productPostFeeds, AmazonEnvelopeMessageOperationType.PartialUpdate);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonProductsReviseFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient, _logger, _credential.MarketplaceId, _credential.MerchantId, submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Product, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonReviseItem, string.Format("{0} - Successfully posted product revise feed for {1} items. \nRequested by: {2}",
                    ChannelName, productPostFeeds.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product revise feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productPostFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonReviseItem, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductReviseFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            try
            {
                // create Amazon envelope object for the single eisProduct
                var envelope = RequestHelper.CreateSingleProductFeedEnvelope(productFeed, AmazonEnvelopeMessageOperationType.PartialUpdate);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonSingleProductReviseFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient,
                    _logger,
                    _credential.MarketplaceId,
                    _credential.MerchantId,
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Product, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonReviseItem, string.Format("{0} - Successfully posted single product revise feed for EisSKU \'{1}\'.\nRequested by: {2}",
                    ChannelName, productFeed.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product revise feed for EisSKU \'{1}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonReviseItem, description, ex.StackTrace);

            }
        }

        public void SubmitProductInventoryFeeds(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy)
        {
            try
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
                    _logger.LogWarning(LogEntryType.AmazonInventoryUpdate, string.Format("{0} - No products for inventory update! Products must be not blacklisted and Amazon enabled. Requested by {1}",
                        ChannelName, submittedBy));
                    return;
                }

                // parse the EIS products and create Amazon envelope
                var envelope = RequestHelper.CreateInventoryFeedEnvelope(inventoryItems);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonInventoryFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient,
                    _logger,
                    _credential.MarketplaceId, 
                    _credential.MerchantId,
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_INVENTORY_AVAILABILITY_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Inventory, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonInventoryUpdate, string.Format("Successfully posted product inventory feed for {0} - {1} items. \nRequested by: {2}",
                    ChannelName, inventoryItems.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting inventory feeds for {0} - {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        inventoryFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductInventoryFeed(MarketplaceInventoryFeed inventoryFeed, string submittedBy)
        {
            try
            {
                // parse the EIS product and create Amazon envelope
                var envelope = RequestHelper.CreateInventoryFeedEnvelope(new List<AmazonInventoryFeed> { inventoryFeed.AmazonInventoryFeed });

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonSingleInventoryFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient,
                    _logger,
                    _credential.MarketplaceId,
                    _credential.MerchantId,
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_INVENTORY_AVAILABILITY_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Inventory, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonInventoryUpdate, string.Format("{0} - Successfully posted single inventory feed for EisSKU \'{1}\'.\nRequested by: {2}",
                    ChannelName, inventoryFeed.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product inventory feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        inventoryFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitProductPriceFeeds(List<MarketplacePriceFeedDto> priceFeeds, string submittedBy)
        {
            try
            {
                // get the Amazon feed and that also allow price update enabled
                var priceItems = priceFeeds
                    .Where(x => !x.IsBlacklisted
                        && x.AmazonPriceFeed != null
                        && x.AmazonPriceFeed.IsEnabled)
                    .Select(x => x.AmazonPriceFeed)
                    .ToList();
                if (!priceItems.Any())
                {
                    _logger.LogWarning(LogEntryType.AmazonInventoryUpdate, string.Format("{0} - No products for price update! Products must be not blacklisted and Amazon enabled. Requested by {1}",
                        ChannelName, submittedBy));
                    return;
                }

                // create the Amazon envelope for the Price feed
                var envelope = RequestHelper.CreatePriceFeedEnvelope(priceItems);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonPriceFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient,
                    _logger,
                    _credential.MarketplaceId,
                    _credential.MerchantId, 
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_PRICING_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Price, submittedBy);
                
                _logger.LogInfo(LogEntryType.AmazonPriceUpdate, string.Format("{0} - Successfully posted price feeds for {1} items. \nRequested by: {2}",
                    ChannelName, priceItems.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting pricing feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        priceFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonPriceUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductPriceFeed(MarketplacePriceFeedDto priceItem, string submittedBy)
        {
            try
            {
                // create the Amazon envelope for the Price feed
                var envelope = RequestHelper.CreatePriceFeedEnvelope(new List<AmazonPriceFeed> { priceItem.AmazonPriceFeed });

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(envelope, "AmazonSinglePriceFeed");

                var submitFeedController = new SubmitFeedController(_amazonClient, 
                    _logger,
                    _credential.MarketplaceId,
                    _credential.MerchantId, 
                    submittedBy);
                var streamResponse = submitFeedController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_PRODUCT_PRICING_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.Price, submittedBy);

                _logger.LogInfo(LogEntryType.AmazonPriceUpdate, string.Format("{0} - Successfully posted single price feed for EisSKU \'{1}\' \nRequested by: {2}",
                    ChannelName, priceItem.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single price feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                         ChannelName,
                         EisHelper.GetExceptionMessage(ex),
                         submittedBy,
                         priceItem.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonPriceUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitProductEndItemFeeds(List<ItemFeed> itemFeeds, string submittedBy)
        {
            return;
        }
        public void SubmitSingleProductEndItem(ItemFeed itemFeed, string submittedBy)
        {
            return;
        }
        
        #region conversion helpers

        /// <summary>
        /// Convert the SubmitFeedResponse stream for the post feed folow into a AmazonExeption if needed
        /// </summary>
        /// <param name="responseStream">The response stream object</param>
        /// <param name="messageType">The type of the message</param>
        /// <returns></returns>
        private void parsedResultStreamAndLogReport(Stream responseStream, AmazonEnvelopeMessageType messageType, string submittedBy)
        {
            try
            {
                using (var stream = responseStream)
                {
                    // the result may not be an XML document. This will be reveled with testing.
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    var report = doc.SelectSingleNode("/AmazonEnvelope/Message/ProcessingReport");
                    var processingSummary = report.SelectSingleNode("ProcessingSummary");
                    var processingReport = new MarketplaceProcessingReport
                    {
                        MerchantId = _credential.MerchantId,
                        MessageType = messageType.ToString(),
                        TransactionId = report.SelectSingleNode("DocumentTransactionID").InnerText,
                        MessagesProcessed = int.Parse(processingSummary.SelectSingleNode("MessagesProcessed").InnerText),
                        MessagesSuccessful = int.Parse(processingSummary.SelectSingleNode("MessagesSuccessful").InnerText),
                        MessagesWithError = int.Parse(processingSummary.SelectSingleNode("MessagesWithError").InnerText),
                        MessagesWithWarning = int.Parse(processingSummary.SelectSingleNode("MessagesWithWarning").InnerText),
                        StatusCode = report.SelectSingleNode("StatusCode").InnerText,
                        SubmittedBy = submittedBy
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

                    // determine the real total number of warning messages
                    var warningCount = reportResults.Count(x => x.Code == "Warning");
                    processingReport.MessagesWithWarning = warningCount;

                    // add it to the report summary
                    processingReport.ReportResults = reportResults;

                    // save it to the database
                    _logger.AddProcessingReport(processingReport);
                }
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error,
                    LogEntryType.AmazonListing,
                    string.Format("Error in parsing {0} result response stream. <br/> Error Message: {1}", messageType.ToString(), EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
            }
        }

        #endregion
    }
}
