using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Managers
{
    public class MarketplaceInventoryManager : IMarketplaceInventoryManager
    {
        private readonly ICredentialService _credentialService;
        private readonly IProductService _productService;
        private readonly ILogService _logger;
        private readonly string _marketplaceMode;
        private static DateTime LAST_PRICE_UPDATE_FEED_TIME;
        
        public MarketplaceInventoryManager(IProductService productService,
            ICredentialService credentialService,
            ILogService logger)
        {
            Core.Container.ComposeParts(this);
            _productService = productService;
            _credentialService = credentialService;
            _logger = logger;
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
        }

        [ImportMany(typeof(IMarketplaceInventoryProvider))]
        protected List<IMarketplaceInventoryProvider> _marketInventoryProviders { get; set; }

        public async Task SubmitProductsListingFeedAsync(MarketplaceFeed marketplaceFeed, string userName)        
        {
            var productPostFeeds = _productService.GetProductPostFeeds(marketplaceFeed).ToList();
            if (!productPostFeeds.Any())
                return;

            // iterate first to the selected marketplaces
            foreach (var crendentialType in marketplaceFeed.Marketplaces)
            {
                // get the marketplace provider
                var marketplaceProvider = getMarketInventoryProvider(crendentialType);
                
                // then list to the list of selected companies
                foreach (var companyId in marketplaceFeed.Companies)
                {
                    // get the marketplace credential define for the company
                    var credential = _credentialService.GetCredential(crendentialType, companyId, marketplaceFeed.Mode);
                    if (credential == null)
                    {
                        _logger.LogWarning(LogEntryType.MarketplaceInventoryManager,
                            string.Format("Company with ID {0} has no credentials for {1} - {2} mode.",
                            companyId, crendentialType, marketplaceFeed.Mode));
                        continue;
                    }

                    // set the credential for this marketplace
                    marketplaceProvider.Credential = credential;

                    // execute the it asynchroounously
                    Task.Factory.StartNew(() =>
                    {
                        // Create a worker object
                        var worker = new MarketplaceWorker(marketplaceProvider,  userName);
                        worker.SubmitProductListingFeeds(productPostFeeds);
                    });
                }
            }
        }

        public async Task SubmitSingleProductListingFeed(string crendentialType, string eisSku, string userName)
        {
            var inventoryProvider = _marketInventoryProviders.First(x => x.ChannelName == crendentialType);
            var productPostFeed = _productService.GetProductPostFeedByEisSku(eisSku);
            

            inventoryProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            inventoryProvider.SubmitSingleProductListingFeed(productPostFeed, userName);
        }

        public async Task SubmitProductsReviseFeedAsync(MarketplaceFeed marketplaceFeed, string userName)
        {
            var productPostFeeds = _productService.GetProductPostFeeds(marketplaceFeed).ToList();
            if (!productPostFeeds.Any())
                return;

            // iterate first to the selected marketplaces
            foreach (var crendentialType in marketplaceFeed.Marketplaces)
            {
                // get the marketplace provider
                var marketplaceProvider = getMarketInventoryProvider(crendentialType);

                // then list to the list of selected companies
                foreach (var companyId in marketplaceFeed.Companies)
                {
                    // get the marketplace credential define for the company
                    var credential = _credentialService.GetCredential(crendentialType, companyId, marketplaceFeed.Mode);
                    if (credential == null)
                    {
                        _logger.LogWarning(LogEntryType.MarketplaceInventoryManager,
                            string.Format("Company with ID {0} has no credentials for {1} - {2} mode.",
                            companyId, crendentialType, marketplaceFeed.Mode));
                        continue;
                    }

                    // set the credential for this marketplace
                    marketplaceProvider.Credential = credential;

                    // execute the it asynchroounously
                    Task.Factory.StartNew(() =>
                    {
                        // Create a worker object
                        var worker = new MarketplaceWorker(marketplaceProvider, userName);
                        worker.SubmitProductReviseFeeds(productPostFeeds);
                    });
                }
            }
        }

        public async Task SubmitSingleProductReviseFeed(string crendentialType, string eisSku, string userName)
        {
            var inventoryProvider = _marketInventoryProviders.First(x => x.ChannelName == crendentialType);
            var productPostFeed = _productService.GetProductPostFeedByEisSku(eisSku);
           
            inventoryProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            inventoryProvider.SubmitSingleProductReviseFeed(productPostFeed, userName);
        }

        public async Task SubmitInventoryFeedAysnc(MarketplaceFeed marketplaceFeed, string userName)
        {
            // get all products with the specified eisProduct types
            var products = _productService.GetProductInventoryFeed(marketplaceFeed).ToList();
            if (!products.Any())
                return;
            
            // iterate first to the selected marketplaces
            foreach (var crendentialType in marketplaceFeed.Marketplaces)
            {
                // get the marketplace provider
                var marketplaceProvider = getMarketInventoryProvider(crendentialType);

                // then list to the list of selected companies
                foreach (var companyId in marketplaceFeed.Companies)
                {
                    // get the marketplace credential define for the company
                    var credential = _credentialService.GetCredential(crendentialType, companyId, marketplaceFeed.Mode);
                    if (credential == null)
                    {
                        _logger.LogWarning(LogEntryType.MarketplaceInventoryManager,
                            string.Format("Company with ID {0} has no credentials for {1} - {2} mode.",
                            companyId, crendentialType, marketplaceFeed.Mode));
                        continue;
                    }

                    marketplaceProvider.Credential = credential;

                    // execute the it asynchroounously
                    Task.Factory.StartNew(() =>
                    {
                        // Create a worker object
                        var worker = new MarketplaceWorker(marketplaceProvider, userName);
                        worker.SubmitInventoryFeeds(products);
                    });
                }
            }
        }

        public async Task SubmitInventoryFeedBySkuAsync(string crendentialType, string eisSku, string userName)
        {
            // get the product to to post it's inventory
            var productInventory = _productService.GetProductInventoryBySku(eisSku);
            
            // get the marketplace
            var markeplaceProvider = _marketInventoryProviders.FirstOrDefault(x => x.ChannelName == crendentialType);

            // set the credential and submit the feed
            markeplaceProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            markeplaceProvider.SubmitSingleProductInventoryFeed(productInventory, userName);
        }

        public async Task SubmitPriceFeedAsync(MarketplaceFeed marketplaceFeed, string userName)
        {
            LAST_PRICE_UPDATE_FEED_TIME = DateTime.Now;

            // get all products with the specified eisProduct types
            var products = _productService.GetProductPriceFeed(marketplaceFeed).ToList();
            if (!products.Any())
                return;

            // iterate first to the selected marketplaces
            foreach (var crendentialType in marketplaceFeed.Marketplaces)
            {
                // get the marketplace provider
                var marketplaceProvider = getMarketInventoryProvider(crendentialType);

                // then list to the list of selected companies
                foreach (var companyId in marketplaceFeed.Companies)
                {
                    // get the marketplace credential define for the company
                    var credential = _credentialService.GetCredential(crendentialType, companyId, marketplaceFeed.Mode);
                    if (credential == null)
                    {
                        _logger.LogWarning(LogEntryType.MarketplaceInventoryManager,
                            string.Format("Company with ID {0} has no credentials for {1} - {2} mode.",
                            companyId, crendentialType, marketplaceFeed.Mode));
                        continue;
                    } 
                    
                    marketplaceProvider.Credential = credential;

                    // execute the it asynchroounously
                    Task.Factory.StartNew(() =>
                    {
                        // Create a worker object
                        var worker = new MarketplaceWorker(marketplaceProvider, userName);
                        worker.SubmitPriceFeeds(products);
                    });
                }
            }
        }

        public async Task SubmitPriceFeedBySkuAsync(string crendentialType, string eisSku, string userName)
        {
            // determine the last minute was the previous sent to the marketplace feed
            var minuteElapse = (DateTime.Now - LAST_PRICE_UPDATE_FEED_TIME).TotalMinutes;
            //while (minuteElapse <= 2)
            //{
            //    // let's sleep for a minue
            //    try { Thread.Sleep(60000); }
            //    catch { }

            //    // redetermine the elapse time
            //    minuteElapse = (DateTime.Now - LAST_PRICE_UPDATE_FEED_TIME).TotalMinutes;
            //}

            // set the current time
            LAST_PRICE_UPDATE_FEED_TIME = DateTime.Now;

            // get the product to to post it's inventory
            var productPrice = _productService.GetProductPriceFeedBySku(eisSku);

            // get the marketplace
            var markeplaceProvider = _marketInventoryProviders.FirstOrDefault(x => x.ChannelName == crendentialType);
            // set the credential and submit the feed

            markeplaceProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            markeplaceProvider.SubmitSingleProductPriceFeed(productPrice, userName);
        }

        public async Task SubmiteBayEndProductListingAsync(MarketplaceFeed marketplaceFeed, string userName)
        {
            var productFeeds = _productService.GeteBayItemFeeds(marketplaceFeed).ToList();
            if (!productFeeds.Any())
                return;

            // iterate first to the selected marketplaces
            foreach (var crendentialType in marketplaceFeed.Marketplaces)
            {
                // get the marketplace provider
                var marketplaceProvider = getMarketInventoryProvider(crendentialType);

                // then list to the list of selected companies
                foreach (var companyId in marketplaceFeed.Companies)
                {
                    // get the marketplace credential define for the company
                    var credential = _credentialService.GetCredential(crendentialType, companyId, marketplaceFeed.Mode);
                    if (credential == null)
                    {
                        _logger.LogWarning(LogEntryType.MarketplaceInventoryManager,
                            string.Format("Company with ID {0} has no credentials for {1} - {2} mode.",
                            companyId, crendentialType, marketplaceFeed.Mode));
                        continue;
                    }

                    marketplaceProvider.Credential = credential;

                    // execute the it asynchroounously
                    Task.Factory.StartNew(() =>
                    {
                        // Create a worker object
                        var worker = new MarketplaceWorker(marketplaceProvider, userName);
                        worker.SubmitProductEndListingFeeds(productFeeds);
                    });
                }
            }

        }

        public async Task SubmiteBayEndItemFeedBySkuAsync(string crendentialType, string eisSku, string userName)
        { 
            // get the product to to post it's inventory
            var productInfo = _productService.GetProducteBay(eisSku);
            if (productInfo == null)
                return; // exit

            var itemFeed = new ItemFeed
            {
                EisSKU = productInfo.EisSKU,
                ItemId = productInfo.ItemId
            };

            // get the marketplace
            var markeplaceProvider = _marketInventoryProviders.FirstOrDefault(x => x.ChannelName == crendentialType);

            // set the credential and submit the feed
            markeplaceProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            markeplaceProvider.SubmitSingleProductEndItem(itemFeed, userName);
        }

        public async Task SubmitEndItemFeedBySkuAsync(string crendentialType, string eisSku, string userName)
        {
            // get the product to to post it's inventory
            var itemFeed = GetMarketplaceEndItemProduct(crendentialType, eisSku);
            if (itemFeed == null)
                return; // exit

            // get the marketplace
            var markeplaceProvider = _marketInventoryProviders.FirstOrDefault(x => x.ChannelName == crendentialType);

            // set the credential and submit the feed
            markeplaceProvider.Credential = _credentialService.GetCredential(crendentialType, _marketplaceMode);
            markeplaceProvider.SubmitSingleProductEndItem(itemFeed, userName);
        }

        public List<string> GetMarketplaceInventoryChannels()
        {
            return _marketInventoryProviders.Select(x => x.ChannelName).OrderBy(x => x).ToList();
        }

        private ItemFeed GetMarketplaceEndItemProduct(string crendentialType, string eisSku)
        {
            var marketplaceEndItem = new ItemFeed();

            switch (crendentialType)
            {
                case MarketPlaceTypes.Values.Amazon:

                    var productInfoAmazon = _productService.GetProductAmazon(eisSku);

                    if (productInfoAmazon == null)
                        break;
                    marketplaceEndItem.EisSKU = productInfoAmazon.EisSKU;
                    marketplaceEndItem.ItemId = productInfoAmazon.ASIN;

                    break;
                case MarketPlaceTypes.Values.BigCommerce:

                    var productInfoBigCommerce = _productService.GetProductBigCommerce(eisSku);

                    if (productInfoBigCommerce == null)
                        break;
                    marketplaceEndItem.EisSKU = productInfoBigCommerce.EisSKU;
                    marketplaceEndItem.ItemId = productInfoBigCommerce.ProductId.ToString();

                    break;
                case MarketPlaceTypes.Values.eBay:

                    var productInfoeBay = _productService.GetProducteBay(eisSku);

                    if (productInfoeBay == null)
                        break;
                    marketplaceEndItem.EisSKU = productInfoeBay.EisSKU;
                    marketplaceEndItem.ItemId = productInfoeBay.ItemId;

                    break;
                default:
                    marketplaceEndItem = null;
                    break;
            }

            return marketplaceEndItem;
        }

        private IMarketplaceInventoryProvider getMarketInventoryProvider(string marketplace)
        {
            return _marketInventoryProviders.FirstOrDefault(x => x.ChannelName == marketplace);
        }
    }

    internal class MarketplaceWorker
    {
        private readonly IMarketplaceInventoryProvider _channel;
        private readonly string _submittedBy;

        public MarketplaceWorker(IMarketplaceInventoryProvider channel,
            string userName)
        {
            _channel = channel;
            _submittedBy = userName;
        }

        public void SubmitProductListingFeeds(List<MarketplaceProductFeedDto> productPostFeeds)
        {
            _channel.SubmitProductsListingFeed(productPostFeeds, _submittedBy);
        }

        public void SubmitProductReviseFeeds(List<MarketplaceProductFeedDto> productFeeds)
        {
            _channel.SubmitProductsReviseFeed(productFeeds, _submittedBy);
        }

        public void SubmitProductEndListingFeeds(List<ItemFeed> productFeeds)
        {
            _channel.SubmitProductEndItemFeeds(productFeeds, _submittedBy);
        }

        public void SubmitInventoryFeeds(List<MarketplaceInventoryFeed> inventoryFeeds)
        {
            _channel.SubmitProductInventoryFeeds(inventoryFeeds, _submittedBy);
        }

        public void SubmitPriceFeeds(List<MarketplacePriceFeedDto> priceFeeds)
        {         
            _channel.SubmitProductPriceFeeds(priceFeeds, _submittedBy);
        }
    }
}
