using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.eBay.Helpers;
using eBay.Service.Util;
using System.Reflection;

namespace EIS.SchedulerTaskApp.Marketplaces
{
    public class eBayProductInventory : IMarketplaceProductInventory
    {
        private ApiContext _context;
        private eBayCredentialDto _credential;
        private readonly ILogService _logger;
        private readonly string _submittedBy;
        private readonly bool _isWriteToFile;
        private readonly string _filePath;

        public eBayProductInventory(string submittedBy, string filePath)
        {
            _logger = new LogService();
            _submittedBy = submittedBy;
            _filePath = string.Format("{0}\\{1:yyyy}{1:MM}{1:dd}-{1:HH}{1:mm}{1:ss}_{2}", filePath, DateTime.Now, "{0}.txt"); 
            _isWriteToFile = Convert.ToBoolean(ConfigurationManager.AppSettings["IsWriteToFile"]);
        }

        public string ChannelName
        {
            get { return "eBay"; }
        }

        public CredentialDto Credential
        {
            get { return _credential; }
            set
            {
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

        public void SubmitProductsInventoryFeed(List<MarketplaceInventoryFeed> inventoryFeeds)
        {
            // get all eBay products for inventory update
            var inventoryItems = inventoryFeeds
                .Where(x => !x.IsBlacklisted && x.eBayInventoryFeed != null 
                    && x.eBayInventoryFeed.ItemId != null)
                .ToList();
            if (!inventoryItems.Any())
            {
                _logger.LogWarning(LogEntryType.eBayInventoryUpdate, "No eBay products for inventory update.");
                return;
            }

            // set the log file name
            _context.ApiLogManager.ApiLoggerList.Add(new FileLogger(string.Format(_filePath, ChannelName + "-InventoryFeed"), false, true, true));

            var faultyInventoryFeeds = sendReviceProductInventory(inventoryItems, _submittedBy, isQuantityUpdate: true);

            if (faultyInventoryFeeds.Any())
                resubmitInventoryBySingleFeed(faultyInventoryFeeds, _submittedBy, isQuantityUpdate: true);

            Console.WriteLine("{0} - End of submitting inventory feed.", ChannelName);
        }

        private List<MarketplaceInventoryFeed> sendReviceProductInventory(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy, bool isPriceUpdate = false, bool isQuantityUpdate = false)
        {
            var faultBatches = new List<MarketplaceInventoryFeed>();
            var totalBatches = Math.Ceiling(inventoryFeeds.Count() / 4.0);

            for (var i = 0; i < totalBatches; i++)
            {
                // only 4 items are allowed in single request for ReviseInventoryStatus
                var batchedInventories = inventoryFeeds.Skip(i * 4).Take(4).ToList();
                var inventoryStatusCollection = new InventoryStatusTypeCollection();

                try
                {
                    foreach (var item in batchedInventories)
                    {
                        // create the item for inventory feed request
                        var inventoryStatus = new InventoryStatusType
                        {
                            ItemID = item.eBayInventoryFeed.ItemId
                        };

                        // set the new price if its price update
                        if (isPriceUpdate)
                        {
                            inventoryStatus.StartPrice = new AmountType
                            {
                                currencyID = CurrencyCodeType.USD,
                                Value = (double)item.eBayInventoryFeed.BinPrice
                            };
                        }

                        // set the new quantity if its for inventory update
                        if (isQuantityUpdate)
                        {
                            inventoryStatus.Quantity = item.eBayInventoryFeed.InventoryQuantity;
                            inventoryStatus.QuantitySpecified = true;
                        }

                        inventoryStatusCollection.Add(inventoryStatus);
                    }

                    // send the inventory collection request
                    var apiCall = new ReviseInventoryStatusCall(_context);
                    var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                    Console.WriteLine("Batch: {0}/{1} - {2} has been successfully posted product inventory feed.", i + 1, totalBatches, ChannelName);
                }
                catch (Exception ex)
                {
                    var description = string.Format("Batch: {0}/{1} - Error in submitting product inventory feed. \nError Message: {2} \nRequested by: {3}",
                       i + 1,
                       totalBatches,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.eBayInventoryUpdate, description, ex.StackTrace);
                    Console.WriteLine(description);

                    // add the batched items to the list for resubmission 1 by 1
                    faultBatches.AddRange(batchedInventories);
                }
            }

            if (!faultBatches.Any())
            {
                _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Successfully posted inventory/price feeds for {0} - {1} items. \nRequested by: {2}",
                    ChannelName, inventoryFeeds.Count(), submittedBy));
                Console.WriteLine(string.Format("Successfully posted inventory/price feeds for {0} - {1} items. \nRequested by: {2}", ChannelName, inventoryFeeds.Count(), submittedBy));
            }

            return faultBatches;
        }

        private void resubmitInventoryBySingleFeed(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy, bool isPriceUpdate = false, bool isQuantityUpdate = false)
        {
            for (var i = 0; i < inventoryFeeds.Count; i++ )
            {
                try
                {
                    // create inventory item feed
                    var inventoryStatus = new InventoryStatusType
                    {
                        ItemID = inventoryFeeds[i].eBayInventoryFeed.ItemId
                    };

                    // set the new price if its price update
                    if (isPriceUpdate)
                    {
                        inventoryStatus.StartPrice = new AmountType
                        {
                            currencyID = CurrencyCodeType.USD,
                            Value = (double)inventoryFeeds[i].eBayInventoryFeed.BinPrice,
                        };
                    }

                    // set the new quantity if its for inventory update
                    if (isQuantityUpdate)
                    {
                        inventoryStatus.Quantity = inventoryFeeds[i].eBayInventoryFeed.InventoryQuantity;
                        inventoryStatus.QuantitySpecified = true;
                    }

                    var inventoryStatusCollection = new InventoryStatusTypeCollection();
                    inventoryStatusCollection.Add(inventoryStatus);

                    // send the inventory collection request
                    var apiCall = new ReviseInventoryStatusCall(_context);
                    var inventoryStatusResult = apiCall.ReviseInventoryStatus(inventoryStatusCollection);

                    _logger.LogInfo(LogEntryType.eBayInventoryUpdate, string.Format("Successfully posted single inventory feed for {0} - {1} item. \nRequested by: {2}", ChannelName, inventoryFeeds[i].EisSKU, submittedBy));
                    Console.WriteLine(string.Format("Successfully posted single inventory feed for {0} - {1} item. \nRequested by: {2}", ChannelName, inventoryFeeds[i].EisSKU, submittedBy));
                }
                catch (Exception ex)
                {
                    var description = string.Format("Error in submitting single inventory feed for {0} . \nError Message: {1} \nRequested by: {2}",
                       ChannelName,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy,
                       inventoryFeeds[i].EisSKU);
                    _logger.LogError(LogEntryType.eBayInventoryUpdate, string.Format("Single: {0}/{1} - {2}", i + 1, inventoryFeeds.Count, description), ex.StackTrace);
                    Console.WriteLine(description);
                }
            }

        }
    }
}
