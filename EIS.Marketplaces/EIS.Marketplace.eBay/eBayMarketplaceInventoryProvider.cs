using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Reflection;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.Util;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.eBay.Helpers;

namespace EIS.Marketplace.eBay
{
    [Export(typeof(IMarketplaceInventoryProvider))]
    public class eBayMarketplaceInventoryProvider : IMarketplaceInventoryProvider
    {
        private ApiContext _context;
        private eBayCredentialDto _credential;
        private ILogService _logger;
        private readonly string _logDirectory;
        private readonly bool _isWriteToFile;

        public eBayMarketplaceInventoryProvider()
        {
            _logger = Core.Get<ILogService>();
            _logDirectory = string.Format("{0}\\{1:yyyy}{1:MM}{1:dd}-{1:HH}{1:mm}{1:ss}_{2}", ConfigurationManager.AppSettings["MarketplaceFeedRoot"], DateTime.Now, "{0}.txt");
            _isWriteToFile = Convert.ToBoolean(ConfigurationManager.AppSettings["IsWriteToFile"]);

        }

        public eBayMarketplaceInventoryProvider(ILogService logger)
        {
            _logger = logger;
        }

        public string ChannelName
        {
            get { return "eBay"; }
        }

        public CredentialDto Credential
        {
            get { return _credential; }
            set {
                _credential = value as eBayCredentialDto;
                RequestHelper.SetCredentials(_credential);

                // init the context
                _context = new ApiContext();
                _context.ApiCredential = RequestHelper.ApiCredential;
                _context.SoapApiServerUrl = RequestHelper.ServiceUrl;
                
                // set the logging
                _context.ApiLogManager = new ApiLogManager();
                _context.ApiLogManager.EnableLogging = _isWriteToFile;
            }
        }

        public void SubmitProductsListingFeed(List<MarketplaceProductFeedDto> productFeeds, string submittedBy)
        {
            // take out the products which has no information for eBay
            var invalidProducts = productFeeds
                .Where(x => x.IsBlacklisted || x.eBayProductFeed == null || x.eBayProductFeed.CategoryId == null)
                .ToList();
            if (invalidProducts.Any())
            {
                _logger.LogWarning(LogEntryType.eBayProductListing, string.Format("{0}/{1} EIS products which will not be included to eBay product listing feed due to no eBay category id or no eBay information or blacklisted.",
                    invalidProducts.Count, productFeeds.Count));
                productFeeds.RemoveAll(x => x.IsBlacklisted || x.eBayProductFeed == null || x.eBayProductFeed.CategoryId == null);
            }

            // let's do not include the eBay products which have already item id
            var alreadyAddedProducts = productFeeds
                .Where(x => !string.IsNullOrEmpty(x.eBayProductFeed.ItemId))
                .ToList();
            if (alreadyAddedProducts.Any())
            {
                _logger.LogWarning(LogEntryType.eBayProductListing, string.Format("{0}/{1} EIS products which will not be included to eBay product listing feed since they are already added. Please do the product revise feed instead!",
                    alreadyAddedProducts.Count, productFeeds.Count));
                productFeeds.RemoveAll(x => !string.IsNullOrEmpty(x.eBayProductFeed.ItemId));
            }

            // determine if there's product feed to post
            if (!productFeeds.Any())
            {
                _logger.LogWarning(LogEntryType.eBayProductListing, "No eBay products for product listing.");
                return;
            }

            // set the log file name
            _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));
            var totalBatches = Math.Ceiling(productFeeds.Count / 5.0);
            var failedBatches = new List<MarketplaceProductFeedDto>();

            for (var i = 0; i < totalBatches; i++)
            {
                // send the product listing feed by 5 items as required for eBay Trading API
                var batchedProducts = productFeeds.Skip(i * 5).Take(5).ToList();
                var itemRequestList = new List<AddItemRequestContainerType>();

                try
                {
                    foreach (var product in batchedProducts)
                    {
                        var itemRequest = new AddItemRequestContainerType();

                        // create the item request object for product
                        itemRequest.Item = RequestHelper.CreateItemType(product, _credential.eBayDescriptionTemplate);
                        itemRequest.MessageID = product.EisSKU;

                        itemRequestList.Add(itemRequest);
                    }

                    // add it to the item collection object
                    var apiCall = new AddItemsCall(_context);
                    var response = apiCall.AddItems(new AddItemRequestContainerTypeCollection(itemRequestList.ToArray()));

                    // parse the product's item id and fees from response object
                    var itemContainers = getParsedResponseItemContainers(response.ToArray());

                    // update the product eBay ItemId and log the posting fees
                    logAndUpdateProductItemId(itemContainers, LogEntryType.eBayProductListing);
                }
                catch (Exception ex)
                {
                    // add it to the faulty list
                    failedBatches.AddRange(batchedProducts);
                                         
                    var description = string.Format("Batch: {0}/{1} - Error in submitting PRODUCT_LISTING feed. \nError Message: {2} \nRequested by: {3}",
                       i + 1,
                       totalBatches,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.eBayProductListing, description, ex.StackTrace);
                }
            }

            // resend the faulty product listing feed if there's any
            if (failedBatches.Any())
            {
                _logger.LogInfo(LogEntryType.eBayProductListing, string.Format("Starting sending PRODUCT_LISTING feed for failed batches. Count: {0}", failedBatches.Count));

                // iterate and send each item
                foreach (var item in failedBatches)
                    SubmitSingleProductListingFeed(item, submittedBy);

                _logger.LogInfo(LogEntryType.eBayProductListing, string.Format("Resubmission for {0} failed items for PRODUCT_LISTING feed has been completed.", failedBatches.Count()));
            }
            else
            {
                _logger.LogInfo(LogEntryType.eBayProductListing, string.Format("Successfully posted product listing feed for {0} - {1} product items. \nRequested by: {2}",
                    ChannelName, productFeeds.Count, submittedBy));
            }
        }

        public void SubmitSingleProductListingFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            if (productFeed.eBayProductFeed == null || productFeed.eBayProductFeed.CategoryId == null)
            {
                _logger.LogWarning(LogEntryType.eBayProductListing, string.Format("Unable to create listing for this product - {0} due to no eBay details or category id.",
                    productFeed.EisSKU));
                return;
            }
            if (!string.IsNullOrEmpty(productFeed.eBayProductFeed.ItemId))
            {
                _logger.LogWarning(LogEntryType.eBayProductListing, string.Format("The EIS product {0} is already listed in eBay. Please do the product revise feed instead!",
                    productFeed.EisSKU));
                return;
            }

            try
            {
                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));
                var apiCall = new AddItemCall(_context);

                // parse the product feed for eBay item
                var itemType = RequestHelper.CreateItemType(productFeed, _credential.eBayDescriptionTemplate);

                // send the request and get the fee collection
                 var fees = apiCall.AddItem(itemType);

                var container = new ItemContainer
                {
                    EisSKU = productFeed.EisSKU,
                    ItemId = itemType.ItemID
                };
                foreach (FeeType fee in fees)
                {
                    if (fee.Fee.Value <= 0)
                        continue;

                    container.Fees.Add(new Fee
                    {
                        Name = fee.Name,
                        Amount = fee.Fee.Value,
                        CurrencyCode = fee.Fee.currencyID.ToString(),
                        ActionFeedType = eBayConstants.ITEM_ADDED
                    });
                }

                logAndUpdateProductItemId(new List<ItemContainer> { container }, LogEntryType.eBayProductListing);
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting single product feed for {0} - {3}. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayProductListing, description, ex.StackTrace);
            }
        }

        public void SubmitProductsReviseFeed(List<MarketplaceProductFeedDto> productFeeds, string submittedBy)
        {
            // let's take out products which has no eBay information or eBayCategory Id
            var invalidProducts = productFeeds
                .Where(x => x.eBayProductFeed == null || string.IsNullOrEmpty(x.eBayProductFeed.ItemId))
                .ToList();
            if (invalidProducts.Any())
            {
                _logger.LogWarning(LogEntryType.eBayProductRevise, string.Format("{0}/{1} EIS products which will not be included to eBay revise feed due to no no eBay information or not listed yet to eBay.",
                    invalidProducts.Count, productFeeds.Count));
                productFeeds.RemoveAll(x => x.eBayProductFeed == null || string.IsNullOrEmpty(x.eBayProductFeed.ItemId));
            }

            // determine if there's product feed to revise
            if (!productFeeds.Any())
            {
                _logger.LogWarning(LogEntryType.eBayProductRevise, "No eBay products for revise.");
                return;
            }

            _logger.LogInfo(LogEntryType.eBayProductRevise, string.Format("Starting posting PRODUCT_REVISE feed. Count: {0}", productFeeds.Count));

            // iterate and submit the products revise feed 1 by one
            foreach (var item in productFeeds)
                SubmitSingleProductReviseFeed(item, submittedBy);

            _logger.LogInfo(LogEntryType.eBayProductRevise, string.Format("Submitting for {0} product items for PRODUCT_REVISE feed has been completed.", productFeeds.Count()));
        }

        public void SubmitSingleProductReviseFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            if (string.IsNullOrEmpty(productFeed.eBayProductFeed.ItemId))
            {
                _logger.LogWarning(LogEntryType.eBayProductRevise, string.Format("Unable to sent product revise feed due no item id found for this product: {0}",
                    productFeed.EisSKU));
                return;
            }

            try
            {
                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));
                var apiCall = new ReviseFixedPriceItemCall(_context);

                // build item type 
                var itemType = RequestHelper.CreateItemType(productFeed, _credential.eBayDescriptionTemplate);
                itemType.ItemID = productFeed.eBayProductFeed.ItemId;

                // send the request and get the fee collection
                var fees = apiCall.ReviseFixedPriceItem(itemType, new StringCollection());
                var feeContainers = new List<ItemContainer>();

                // parse the fees for revising the item
                if (fees != null)
                {
                    foreach (FeeType fee in fees)
                    {
                        if (fee.Fee.Value <= 0)
                            continue;

                        var container = new ItemContainer
                        {
                            EisSKU = productFeed.EisSKU,
                            ItemId = productFeed.eBayProductFeed.ItemId
                        };

                        container.Fees.Add(new Fee
                        {
                            Name = fee.Name,
                            Amount = fee.Fee.Value,
                            CurrencyCode = fee.Fee.currencyID.ToString(),
                            ActionFeedType = eBayConstants.ITEM_REVISED
                        });

                        feeContainers.Add(container);
                    }
                    
                    // insert the fees to database and don't update the product eBay item id
                    logAndUpdateProductItemId(feeContainers, LogEntryType.eBayProductRevise, true);
                }
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting single product revise feed for {0} - {3}. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayProductRevise, description, ex.StackTrace);
            }
        }

        public void SubmitProductPriceFeeds(List<MarketplacePriceFeedDto> priceFeeds, string submittedBy)
        {
            try
            {
                var invalidItemFeeds = priceFeeds.Where(x => x.eBayInventoryFeed == null || string.IsNullOrEmpty(x.eBayInventoryFeed.ItemId)).ToList();
                if (invalidItemFeeds.Any())
                {
                    _logger.LogWarning(LogEntryType.eBayPriceUpdate, string.Format("Unable to send price update feed for {0} items due to no ItemId. Requested by {1}", invalidItemFeeds.Count, submittedBy));
                    priceFeeds.RemoveAll(x => x.eBayInventoryFeed == null || string.IsNullOrEmpty(x.eBayInventoryFeed.ItemId));
                }

                if (!priceFeeds.Any())
                {
                    _logger.LogWarning(LogEntryType.eBayPriceUpdate, "No eBay products for price update.");
                    return;
                }

                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));

                var faultyInventoryFeeds = sendInventoryStatusFeeds(priceFeeds.Select(x => x.eBayInventoryFeed).ToList(), submittedBy, isPriceUpdate: true);

                if (faultyInventoryFeeds.Any())
                    resubmitInventoryStatusBySingleFeeds(faultyInventoryFeeds, submittedBy, isPriceUpdate: true);

                _logger.LogInfo(LogEntryType.eBayPriceUpdate, string.Format("Successfully posted price feeds for {0} items. \nRequested by: {1}", priceFeeds.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting product price feeds for {0} items. \nError Message: {1} \nRequested by: {2}",
                        priceFeeds.Count,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayPriceUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductPriceFeed(MarketplacePriceFeedDto priceFeed, string submittedBy)
        {
            if (string.IsNullOrEmpty(priceFeed.eBayInventoryFeed.ItemId))
            {
                _logger.LogInfo(LogEntryType.eBayPriceUpdate, string.Format("Unable to send price update feed for \'{0}\' since it doesn't have ItemId. \nRequested by: {1}", priceFeed.EisSKU, submittedBy));
                return;
            }

            try
            {
                // create inventory item feed
                var inventoryStatus = new InventoryStatusType
                {
                    ItemID = priceFeed.eBayInventoryFeed.ItemId,
                    StartPrice = new AmountType
                    {
                        currencyID = CurrencyCodeType.USD,
                        Value = (double)priceFeed.eBayInventoryFeed.StartPrice
                    }
                };

                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));

                var inventoryStatusCollection = new InventoryStatusTypeCollection();
                inventoryStatusCollection.Add(inventoryStatus);

                // send the inventory collection request
                var apiCall = new ReviseInventoryStatusCall(_context);
                var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                _logger.LogInfo(LogEntryType.eBayPriceUpdate, string.Format("Successfully posted single price feed for \'{0}\'. \nRequested by: {1}", priceFeed.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting single product price feed for \'{0}\'. \nError Message: {1} \nRequested by: {2}",
                        priceFeed.EisSKU,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayPriceUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitProductInventoryFeeds(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy)
        {
            try
            {
                var invalidItemFeeds = inventoryFeeds.Where(x => x.eBayInventoryFeed == null || string.IsNullOrEmpty(x.eBayInventoryFeed.ItemId)).ToList();
                if (invalidItemFeeds.Any())
                {
                    _logger.LogWarning(LogEntryType.eBayInventoryUpdate, string.Format("Unable to send price inventory feed for {0} items due to no ItemId. Requested by {1}", invalidItemFeeds.Count, submittedBy));
                    inventoryFeeds.RemoveAll(x => x.eBayInventoryFeed == null || string.IsNullOrEmpty(x.eBayInventoryFeed.ItemId));
                }

                if (!inventoryFeeds.Any())
                {
                    _logger.LogWarning(LogEntryType.eBayInventoryUpdate, "No eBay products for inventory update.");
                    return;
                }

                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));

                var faultyInventoryFeeds = sendInventoryStatusFeeds(inventoryFeeds.Select(x=> x.eBayInventoryFeed).ToList(), submittedBy, isQuantityUpdate: true);

                if (faultyInventoryFeeds.Any())
                    resubmitInventoryStatusBySingleFeeds(faultyInventoryFeeds, submittedBy, isQuantityUpdate: true);

                _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Successfully posted inventory feeds for {0} items. \nRequested by: {1}", inventoryFeeds.Count, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting product inventory feeds for {0} items. \nError Message: {1} \nRequested by: {2}",
                        inventoryFeeds.Count,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductInventoryFeed(MarketplaceInventoryFeed inventoryFeed, string submittedBy)
        {
            try
            {
                if(string.IsNullOrEmpty(inventoryFeed.eBayInventoryFeed.ItemId))
                {
                    _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Unable to send inventory update feed for \'{0}\' since it doesn't have ItemId. \nRequested by: {1}", inventoryFeed.EisSKU, submittedBy));
                    return;
                }

                // create inventory item feed
                var inventoryStatus = new InventoryStatusType
                {
                    ItemID = inventoryFeed.eBayInventoryFeed.ItemId,
                    Quantity = inventoryFeed.eBayInventoryFeed.InventoryQuantity
                };

                var inventoryStatusCollection = new InventoryStatusTypeCollection();
                inventoryStatusCollection.Add(inventoryStatus);

                // set the log file name
                _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));

                // send the inventory collection request
                var apiCall = new ReviseInventoryStatusCall(_context);
                var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Successfully posted single inventory feed for {0} - {1} item. \nRequested by: {2}",
                    ChannelName, inventoryFeed.EisSKU, submittedBy));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting single inventory price feed for \'{0}\'. \nError Message: {1} \nRequested by: {2}",
                        inventoryFeed.EisSKU,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitProductEndItemFeeds(List<ItemFeed> itemFeeds, string submittedBy)
        {
            // let's take out the items which have no item id
            itemFeeds.RemoveAll(x => string.IsNullOrEmpty(x.ItemId));

            // determine if there's product feed to post
            if (!itemFeeds.Any())
            {
                _logger.LogWarning(LogEntryType.eBayEndListing, "No eBay products for end listing.");
                return;
            }

            // set the log file name
            _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_logDirectory, MethodBase.GetCurrentMethod().Name), false, true, true));

            // we can specify up to 10 listing to end in single request
            var totalBatches = Math.Ceiling(itemFeeds.Count / 10.0);
            var failedBatches = new List<ItemFeed>();

            for (var i = 0; i < totalBatches; i++)
            {
                // send product end listing by batches
                var batchedProducts = itemFeeds.Skip(i * 10).Take(10).ToList();
                var itemRequestList = new EndItemRequestContainerTypeCollection();

                try
                {
                    foreach (var product in batchedProducts)
                    {
                        var itemRequest = new EndItemRequestContainerType
                        {
                            MessageID = product.EisSKU,
                            ItemID = product.ItemId,
                            EndingReason = EndReasonCodeType.NotAvailable,
                            EndingReasonSpecified = true,
                        };

                        itemRequestList.Add(itemRequest);
                    }

                    // create the api call and send the request
                    var apiCall = new EndItemsCall(_context);
                    var response = apiCall.EndItems(itemRequestList);

                    // update the product eBay' ItemId to null
                    _logger.SeteBayItemIdToNull(batchedProducts);

                }
                catch (Exception ex)
                {
                    var description = string.Format("Batch: {0}/{1} - Error in submitting END_ITEM feed. \nError Message: {2} \nRequested by: {3}",
                       i + 1,
                       totalBatches,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.eBayEndListing, description, ex.StackTrace);
                    Console.WriteLine(description);

                    // add the batched items to the list for resubmission 1 by 1
                    failedBatches.AddRange(batchedProducts);
                }
            }

            if (failedBatches.Any())
            {
                _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("Starting sending END_ITEM feed for failed batches. Count: {0}", failedBatches.Count));

                // iterate and send each item
                foreach (var item in failedBatches)
                    SubmitSingleProductEndItem(item, submittedBy);

                _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("Resubmission for {0} failed items for END_ITEM feed has been completed.", failedBatches.Count()));
            }
            else
            {
                _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("Successfully posted product END_ITEM feed for {0} - {1} product items. \nRequested by: {2}",
                    ChannelName, itemFeeds.Count, submittedBy));
            }
        }

        public void SubmitSingleProductEndItem(ItemFeed itemFeed, string submittedBy)
        {
            try
            {
                if (string.IsNullOrEmpty(itemFeed.ItemId))
                {
                    _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("Unable to send END_ITEM feed for \'{0}\' since it doesn't have ItemId. \nRequested by: {1}", itemFeed.EisSKU, submittedBy));
                    return;
                }

                // create the end item request
                var endItemCall = new EndItemCall(_context);
                endItemCall.EndItem(itemFeed.ItemId, EndReasonCodeType.NotAvailable);

                // let's set the eBay product ItemId to null
                _logger.SeteBayItemIdToNull(new List<ItemFeed> { itemFeed });

                _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("Successfully posted single END_ITEM feed for {0} - {1} item. \nRequested by: {2} \nAPI Message: {3}",
                    ChannelName, itemFeed.EisSKU, submittedBy, endItemCall.ApiResponse.Message));
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting END_ITEM feed for {0}. \nError Message: {2} \nRequested by: {3}",
                      itemFeed.EisSKU,
                      EisHelper.GetExceptionMessage(ex),
                      submittedBy);
                _logger.LogError(LogEntryType.eBayEndListing, description, ex.StackTrace);
            }
        }

        private List<eBayInventoryFeed> sendInventoryStatusFeeds(List<eBayInventoryFeed> inventoryItems, string submittedBy, bool isPriceUpdate = false, bool isQuantityUpdate = false)
        {
            var failedBatches = new List<eBayInventoryFeed>();
            var totalBatches = Math.Ceiling(inventoryItems.Count() / 4.0);

            for (var i = 0; i < totalBatches; i++)
            {
                // only 4 items are allowed in single request for ReviseInventoryStatus
                var batchedInventories = inventoryItems.Skip(i * 4).Take(4).ToList();
                var inventoryStatusCollection = new InventoryStatusTypeCollection();

                try
                {
                    foreach (var item in batchedInventories)
                    {
                        // create the item for inventory feed request
                        var inventoryStatus = new InventoryStatusType
                        {
                            ItemID = item.ItemId
                        };

                        // set the new price if its price update
                        if (isPriceUpdate)
                        {
                            inventoryStatus.StartPrice = new AmountType
                            {
                                currencyID = CurrencyCodeType.USD,
                                Value = (double)item.BinPrice
                            };
                        }

                        // set the new quantity if its for inventory update
                        if (isQuantityUpdate)
                        {
                            inventoryStatus.Quantity = item.InventoryQuantity;
                            inventoryStatus.QuantitySpecified = true;
                        }

                        inventoryStatusCollection.Add(inventoryStatus);
                    }

                    // send the inventory collection request
                    var apiCall = new ReviseInventoryStatusCall(_context);
                    var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                }
                catch (Exception ex)
                {
                    var description = string.Format("Batch: {0}/{1} - Error in submitting {2} feed. \nError Message: {3} \nRequested by: {4}",
                       i + 1,
                       totalBatches,
                       (isPriceUpdate ? "PRICE" : "INVENTORY"),
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.eBayInventoryUpdate, description, ex.StackTrace);
                    Console.WriteLine(description);

                    // add the batched items to the list for resubmission 1 by 1
                    failedBatches.AddRange(batchedInventories);
                }
            }

            if (!failedBatches.Any())
            {
                _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Successfully posted inventory/price feeds for {0} - {1} items. \nRequested by: {2}",
                    ChannelName, inventoryItems.Count(), submittedBy));
            }

            return failedBatches;
        }
        
        private void resubmitInventoryStatusBySingleFeeds(List<eBayInventoryFeed> inventoryFeeds, string submittedBy, bool isPriceUpdate = false, bool isQuantityUpdate = false)
        {
            for (var i = 0; i < inventoryFeeds.Count; i++)
            {
                try
                {
                    // create inventory item feed
                    var inventoryStatus = new InventoryStatusType
                    {
                        ItemID = inventoryFeeds[i].ItemId
                    };

                    // set the new price if its price update
                    if (isPriceUpdate)
                    {
                        inventoryStatus.StartPrice = new AmountType
                        {
                            currencyID = CurrencyCodeType.USD,
                            Value = (double)inventoryFeeds[i].BinPrice,
                        };
                    }

                    // set the new quantity if its for inventory update
                    if (isQuantityUpdate)
                    {
                        inventoryStatus.Quantity = inventoryFeeds[i].InventoryQuantity;
                        inventoryStatus.QuantitySpecified = true;
                    }

                    var inventoryStatusCollection = new InventoryStatusTypeCollection();
                    inventoryStatusCollection.Add(inventoryStatus);

                    // send the inventory collection request
                    var apiCall = new ReviseInventoryStatusCall(_context);
                    var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                    var description = string.Format("Successfully posting {0} feed for {1}.  \nRequested by: {2}",
                       (isPriceUpdate ? "PRICE" : "INVENTORY"),
                       inventoryFeeds[i].EisSKU,
                       submittedBy);
                    _logger.LogInfo(LogEntryType.eBayInventoryUpdate, description);
                    Console.WriteLine(description);
                }
                catch (Exception ex)
                {
                    var description = string.Format("Error in submitting single {0} feed for {1} . \nError Message: {2} \nRequested by: {3}",
                       (isPriceUpdate ? "PRICE" : "INVENTORY"),
                       inventoryFeeds[i].EisSKU,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.eBayInventoryUpdate, string.Format("Single: {0}/{1} - {2}", i + 1, inventoryFeeds.Count, description), ex.StackTrace);
                    Console.WriteLine(description);
                }
            }

        }

        private void logAndUpdateProductItemId(List<ItemContainer> items, LogEntryType entryType, bool isUpdateItemId = true)
        {
            foreach (var item in items)
            {
                _logger.AddProductItemFees(item, isUpdateItemId);

                if(!string.IsNullOrEmpty(item.Message))
                    _logger.LogWarning(entryType, string.Format("EisSKU: {0} - API Message: {1}", item.EisSKU, item.Message));
            }
        }

        private List<ItemContainer> getParsedResponseItemContainers(AddItemResponseContainerType[] itemContainers)
        {
            var items = new List<ItemContainer>();
            for (var i = 0; i < itemContainers.Length; i++)
            {
                var container = new ItemContainer
                {
                    EisSKU = itemContainers[i].CorrelationID,
                    ItemId = itemContainers[i].ItemID               
                };

                foreach (FeeType fee in itemContainers[i].Fees)
                {
                    if (fee.Fee.Value <= 0)
                        continue;

                    container.Fees.Add(new Fee
                    {
                        Name = fee.Name,
                        Amount = fee.Fee.Value,
                        CurrencyCode = fee.Fee.currencyID.ToString(),
                        ActionFeedType = eBayConstants.ITEM_ADDED
                    });
                }
                items.Add(container);
            }
            return items;
        }
    }
}
