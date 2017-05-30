using EIS.Inventory.Core.MwsChannels;
using System;
using System.Collections.Generic;
using System.Linq;
using BigCommerce4Net.Api;
using BigCommerce4Net.Domain;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Marketplace.BigCommerce
{
    [Export(typeof(IMarketplaceInventoryProvider))]
    public class BigCommerceMarketplaceInventoryProvider : IMarketplaceInventoryProvider
    {
        private Configuration _apiConfiguration;
        private Client _client;
        private BigCommerceCredentialDto _credential;
        private ILogService _logger;

        EisInventoryContext _context;

        public BigCommerceMarketplaceInventoryProvider()
        {
            _context = new EisInventoryContext();

            _logger = new LogService();
        }

        #region Public Methods

        public string ChannelName
        {
            get { return "BigCommerce"; }
        }

        public CredentialDto Credential
        {
            get { return _credential; }
            set
            {
                _credential = value as BigCommerceCredentialDto;

                // Apply Configuration once credentials are set
                _apiConfiguration = new Configuration()
                {
                    ServiceURL = _credential.ServiceEndPoint,
                    UserName = _credential.Username,
                    UserApiKey = _credential.ApiKey,
                    MaxPageLimit = 250,
                    AllowDeletions = true // Is false by default, must be true to allow deletions
                };

                _client = new Client(_apiConfiguration);
            }
        }

        public void SubmitProductsListingFeed(List<MarketplaceProductFeedDto> productPostFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has product type id or no information for Big Commerce
                var invalidProducts = productPostFeeds
                     .Where(x => x.IsBlacklisted 
                         || x.BigCommerceProductFeed == null 
                         || x.BigCommerceProductFeed.ProductId.HasValue
                         || !x.BigCommerceProductFeed.IsEnabled)
                    .ToList();

                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.BigCommerceProductListing, string.Format("{0}/{1} EIS products which will not be included to BigCommerce product listing feed due to no product ID or no BigCommerce information or black listed.",
                        invalidProducts.Count, productPostFeeds.Count));
                    productPostFeeds.RemoveAll(x => x.IsBlacklisted 
                                            || x.BigCommerceProductFeed == null 
                                            || x.BigCommerceProductFeed.ProductId.HasValue
                                            || !x.BigCommerceProductFeed.IsEnabled);
                }

                var successCount = 0;

                // Submit product listing
                foreach (var product in productPostFeeds)
                {
                    var bcProductFeed = mapEISProductToBCProduct(product);

                    var jsonBCProductFeed = JsonConvert.SerializeObject(bcProductFeed);

                    var response = _client.Products.Create(jsonBCProductFeed);

                    if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        var productID = response.Data.Id;

                        // Update Product ID of Product Big Commerce
                        UpdateEISProductID(product.EisSKU, productID);

                        // Update Product URL of Product Big Commerce
                        UpdateEISProductURL(product.EisSKU, response.Data.CustomUrl);

                        // Create Product Image Data in Big Commerce
                        if (product.ImageUrls.Count > 0)
                        {
                            foreach (var imageurl in product.ImageUrls)
                            {
                                CreateProductImage(productID, imageurl);
                            }
                        }

                        // Create Custom Fields
                        var eisProductCustomFieldList = GetEISCustomFields(productID);
                        CreateProductCustomFields(productID, eisProductCustomFieldList);

                        successCount++;
                    }
                }

                _logger.LogInfo(LogEntryType.BigCommerceProductListing, string.Format("Successfully posted product listing feed for {0} - {1} product items. \nRequested by: {2}",
                    ChannelName, successCount, submittedBy));

                var unsuccessfulPostingCount = productPostFeeds.Count - successCount;
                if (unsuccessfulPostingCount > 0)
                {
                    _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceProductListing, string.Format("Error posting product listing feed for {0} - {1} product items. \nRequested by: {2}",
                        ChannelName, unsuccessfulPostingCount, submittedBy));
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product listing feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productPostFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceProductListing, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductListingFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            try
            {
                if (productFeed.BigCommerceProductFeed != null && !productFeed.BigCommerceProductFeed.ProductId.HasValue)
                {
                    var bcProductFeed = mapEISProductToBCProduct(productFeed);

                    var jsonBCProductFeed = JsonConvert.SerializeObject(bcProductFeed);

                    var response = _client.Products.Create(jsonBCProductFeed);

                    if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        var productID = response.Data.Id;

                        // Update Product ID of Product Big Commerce
                        UpdateEISProductID(productFeed.EisSKU, productID);

                        // Update Product URL of Product Big Commerce
                        UpdateEISProductURL(productFeed.EisSKU, response.Data.CustomUrl);

                        // Create Product Image Data in Big Commerce
                        if (productFeed.ImageUrls.Count > 0)
                        {
                            foreach (var imageurl in productFeed.ImageUrls)
                            {
                                CreateProductImage(productID, imageurl);
                            }
                        }

                        // Create Custom Fields
                        var eisProductCustomFieldList = GetEISCustomFields(productID);
                        CreateProductCustomFields(productID, eisProductCustomFieldList);

                        _logger.LogInfo(LogEntryType.BigCommerceProductListing, string.Format("{0} - Successfully posted single product feed for EisSKU \'{1}\'.\nRequested by: {2}",
                            ChannelName, productFeed.EisSKU, submittedBy));
                    }
                    else
                    {
                        var errormessage = "";

                        if (response.ResponseErrors.Count > 0)
                        {
                            errormessage = response.ResponseErrors.First().Message;
                        }

                        _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceProductListing, string.Format("{0} - Error posting single product feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                            ChannelName, productFeed.EisSKU, errormessage, submittedBy));
                    }
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceProductListing, description, ex.StackTrace);
            }
        }

        public void SubmitProductsReviseFeed(List<MarketplaceProductFeedDto> productPostFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has no product type id or no information for Big Commerce
                var invalidProducts = productPostFeeds
                     .Where(x => x.IsBlacklisted || x.BigCommerceProductFeed == null || !x.BigCommerceProductFeed.ProductId.HasValue)
                    .ToList();

                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.BigCommerceProductListing, string.Format("{0}/{1} EIS products which will not be included to BigCommerce product listing feed due to no product ID or no BigCommerce information or black listed.",
                        invalidProducts.Count, productPostFeeds.Count));
                    productPostFeeds.RemoveAll(x => x.IsBlacklisted || x.BigCommerceProductFeed == null || !x.BigCommerceProductFeed.ProductId.HasValue);
                }

                var successCount = 0;

                // Submit product listing
                foreach (var product in productPostFeeds)
                {
                    var productId = product.BigCommerceProductFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);
                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISProductToBCProduct(product, bcProductFeed);

                        var updatedata = GetProductFeedUpdateString(bcProductFeed);

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var productID = response.Data.Id;

                            // Update Product Image Data in Big Commerce
                            var bcProdImageList = GetProductsImageListByProductID(productID);

                            if (bcProdImageList.Count > 0)
                            {
                                UpdateProductImages(productID, bcProdImageList, product.ImageUrls);
                            }
                            else if (product.ImageUrls.Count > 0)
                            {
                                CreateProductImages(productID, product.ImageUrls);
                            }

                            // Update Custom Fields
                            var eisProductCustomFieldList = GetEISCustomFields(productID);
                            var bcProductsCustomFieldList = GetProductsCustomeFieldListByProductID(productID);

                            UpdateProductCustomField(productID, bcProductsCustomFieldList, eisProductCustomFieldList);


                            successCount++;
                        }
                    }
                }

                _logger.LogInfo(LogEntryType.BigCommerceReviseItem, string.Format("{0} - Successfully posted product revise feed for {1} items. \nRequested by: {2}",
                    ChannelName, successCount, submittedBy));

                var unsuccessfulPostingCount = productPostFeeds.Count - successCount;
                if (unsuccessfulPostingCount > 0)
                {
                    _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceReviseItem, string.Format("Error posting product revise feed for {0} - {1} product items. \nRequested by: {2}",
                        ChannelName, unsuccessfulPostingCount, submittedBy));
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product revise feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productPostFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceReviseItem, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductReviseFeed(MarketplaceProductFeedDto productFeed, string submittedBy)
        {
            try
            {
                if (productFeed.BigCommerceProductFeed != null && productFeed.BigCommerceProductFeed.ProductId.HasValue)
                {
                    var productId = productFeed.BigCommerceProductFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);
                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISProductToBCProduct(productFeed, bcProductFeed);

                        var updatedata = GetProductFeedUpdateString(bcProductFeed);

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var productID = response.Data.Id;

                            // Update Product Image Data in Big Commerce
                            var bcProdImageList = GetProductsImageListByProductID(productID);

                            if (bcProdImageList.Count > 0)
                            {
                                UpdateProductImages(productID, bcProdImageList, productFeed.ImageUrls);
                            }
                            else if (productFeed.ImageUrls.Count > 0)
                            {
                                CreateProductImages(productID, productFeed.ImageUrls);
                            }


                            // Update Custom Fields
                            var eisProductCustomFieldList = GetEISCustomFields(productID);
                            var bcProductsCustomFieldList = GetProductsCustomeFieldListByProductID(productID);

                            UpdateProductCustomField(productID, bcProductsCustomFieldList, eisProductCustomFieldList);


                            _logger.LogInfo(LogEntryType.BigCommerceReviseItem, string.Format("{0} - Successfully posted single product revise feed for EisSKU \'{1}\'.\nRequested by: {2}",
                                ChannelName, productFeed.EisSKU, submittedBy));
                        }
                        else
                        {
                            var errormessage = "";

                            if (response.ResponseErrors.Count > 0)
                            {
                                errormessage = response.ResponseErrors.First().Message;
                            }

                            _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceReviseItem, string.Format("{0} - Error posting single product revise feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                                ChannelName, productFeed.EisSKU, errormessage, submittedBy));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product revise feed for EisSKU \'{1}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        productFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceReviseItem, description, ex.StackTrace);
            }
        }

        public void SubmitProductInventoryFeeds(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has no product type id or no information for Big Commerce
                var invalidProducts = inventoryFeeds
                     .Where(x => x.IsBlacklisted || x.BigCommerceInventoryFeed == null || !x.BigCommerceInventoryFeed.ProductId.HasValue)
                    .ToList();

                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.BigCommerceProductListing, string.Format("{0}/{1} EIS products which will not be included to BigCommerce product listing feed due to no product ID or no BigCommerce information or black listed.",
                        invalidProducts.Count, inventoryFeeds.Count));
                    inventoryFeeds.RemoveAll(x => x.IsBlacklisted || x.BigCommerceInventoryFeed == null || !x.BigCommerceInventoryFeed.ProductId.HasValue);
                }

                var successCount = 0;

                // Submit product listing
                foreach (var product in inventoryFeeds)
                {
                    var productId = product.BigCommerceInventoryFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);
                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISInventoryToBCProduct(product, bcProductFeed);

                        var updatedata = new
                        {
                            inventory_level = bcProductFeed.InventoryLevel,
                            inventory_warning_level = bcProductFeed.InventoryWarningLevel,
                            inventory_tracking = bcProductFeed.InventoryTracking,
                            order_quantity_minimum = bcProductFeed.OrderQuantityMinimum,
                            order_quantity_maximum = bcProductFeed.OrderQuantityMaximum
                        };

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            successCount++;
                    }
                }

                _logger.LogInfo(LogEntryType.BigCommerceInventoryUpdate, string.Format("{0} - Successfully posted product inventory feed for {1} items. \nRequested by: {2}",
                    ChannelName, successCount, submittedBy));

                var unsuccessfulPostingCount = inventoryFeeds.Count - successCount;
                if (unsuccessfulPostingCount > 0)
                {
                    _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceInventoryUpdate, string.Format("Error posting product inventory feed for {0} - {1} product items. \nRequested by: {2}",
                        ChannelName, unsuccessfulPostingCount, submittedBy));
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product inventory feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        inventoryFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductInventoryFeed(MarketplaceInventoryFeed inventoryFeed, string submittedBy)
        {
            try
            {
                if (inventoryFeed.BigCommerceInventoryFeed != null && inventoryFeed.BigCommerceInventoryFeed.ProductId.HasValue)
                {
                    var productId = inventoryFeed.BigCommerceInventoryFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);
                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISInventoryToBCProduct(inventoryFeed, bcProductFeed);

                        var updatedata = new
                        {
                            inventory_level = bcProductFeed.InventoryLevel,
                            inventory_warning_level = bcProductFeed.InventoryWarningLevel,
                            inventory_tracking = bcProductFeed.InventoryTracking,
                            order_quantity_minimum = bcProductFeed.OrderQuantityMinimum,
                            order_quantity_maximum = bcProductFeed.OrderQuantityMaximum
                        };

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogInfo(LogEntryType.BigCommerceInventoryUpdate, string.Format("{0} - Successfully posted single inventory feed for EisSKU \'{1}\'.\nRequested by: {2}",
                                ChannelName, inventoryFeed.EisSKU, submittedBy));
                        }
                        else
                        {
                            var errormessage = "";

                            if (response.ResponseErrors.Count > 0)
                            {
                                errormessage = response.ResponseErrors.First().Message;
                            }

                            _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceInventoryUpdate, string.Format("{0} - Error posting single inventory feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                                ChannelName, inventoryFeed.EisSKU, errormessage, submittedBy));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product inventory feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        inventoryFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitProductPriceFeeds(List<MarketplacePriceFeedDto> priceFeeds, string submittedBy)
        {
            try
            {
                // take out the products which has no product type id or no information for Amazon
                var invalidProducts = priceFeeds
                     .Where(x => x.IsBlacklisted || x.BigCommerceProductFeed == null || !x.BigCommerceProductFeed.ProductId.HasValue)
                    .ToList();

                if (invalidProducts.Any())
                {
                    _logger.Add(LogEntrySeverity.Warning, LogEntryType.BigCommerceProductListing, string.Format("{0}/{1} EIS products which will not be included to BigCommerce product listing feed due to no product ID or no BigCommerce information or black listed.",
                        invalidProducts.Count, priceFeeds.Count));
                    priceFeeds.RemoveAll(x => x.IsBlacklisted || x.BigCommerceProductFeed == null || !x.BigCommerceProductFeed.ProductId.HasValue);
                }

                var successCount = 0;

                // Submit product listing
                foreach (var product in priceFeeds)
                {
                    var productId = product.BigCommerceProductFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);
                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISPriceToBCProduct(product, bcProductFeed);

                        var updatedata = new
                        {
                            price = bcProductFeed.Price,
                            retail_price = bcProductFeed.RetailPrice,
                            fixed_cost_shipping_price = bcProductFeed.FixedCostShippingPrice
                        };

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                            successCount++;
                    }
                }

                _logger.LogInfo(LogEntryType.BigCommercePriceUpdate, string.Format("{0} - Successfully posted product price feed for {1} items. \nRequested by: {2}",
                    ChannelName, successCount, submittedBy));

                var unsuccessfulPostingCount = priceFeeds.Count - successCount;
                if (unsuccessfulPostingCount > 0)
                {
                    _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommercePriceUpdate, string.Format("Error posting product price feed for {0} - {1} product items. \nRequested by: {2}",
                        ChannelName, unsuccessfulPostingCount, submittedBy));
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product price feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        priceFeeds.Count);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceInventoryUpdate, description, ex.StackTrace);
            }
        }

        public void SubmitSingleProductPriceFeed(MarketplacePriceFeedDto priceFeed, string submittedBy)
        {
            try
            {
                if (priceFeed.BigCommerceProductFeed != null && priceFeed.BigCommerceProductFeed.ProductId.HasValue)
                {
                    var productId = priceFeed.BigCommerceProductFeed.ProductId.Value;
                    var bcProductFeedResponse = _client.Products.Get(productId);

                    if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var bcProductFeed = bcProductFeedResponse.Data;

                        bcProductFeed = mapEISPriceToBCProduct(priceFeed, bcProductFeed);

                        var updatedata = new
                        {
                            price = bcProductFeed.Price,
                            retail_price = bcProductFeed.RetailPrice,
                            fixed_cost_shipping_price = bcProductFeed.FixedCostShippingPrice
                        };

                        var response = _client.Products.Update(productId, updatedata);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            _logger.LogInfo(LogEntryType.BigCommercePriceUpdate, string.Format("{0} - Successfully posted single price feed for EisSKU \'{1}\'.\nRequested by: {2}",
                                ChannelName, priceFeed.EisSKU, submittedBy));
                        }
                        else
                        {
                            var errormessage = "";

                            if (response.ResponseErrors.Count > 0)
                            {
                                errormessage = response.ResponseErrors.First().Message;
                            }

                            _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommercePriceUpdate, string.Format("{0} - Error posting single price feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                                ChannelName, priceFeed.EisSKU, errormessage, submittedBy));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting single product price feed for EisSKU \'{3}\'. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        submittedBy,
                        priceFeed.EisSKU);
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommercePriceUpdate, description, ex.StackTrace);
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
            
            // we can specify up to 10 listing to end in single request
            var totalBatches = Math.Ceiling(itemFeeds.Count / 10.0);
            var failedBatches = new List<ItemFeed>();
            
            for (var i = 0; i < totalBatches; i++)
            {
                // send product end listing by batches
                var batchedProducts = itemFeeds.Skip(i * 10).Take(10).ToList();

                try
                {
                    foreach (var product in batchedProducts)
                    {
                        var productId = Convert.ToInt32(product.ItemId);

                        var bcProductFeedResponse = _client.Products.Get(productId);

                        if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var bcProductFeed = bcProductFeedResponse.Data;

                            var response = _client.Products.Delete(productId);

                            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                            {
                                _logger.LogInfo(LogEntryType.BigCommerceEndListing, string.Format("Successfully posted single END_ITEM feed for {0} - {1} item. \nRequested by: {2} \nAPI Message: {3}",
                                    ChannelName, product.EisSKU, submittedBy, response.RestResponse.StatusDescription));
                            }
                            else
                            {
                                var errormessage = "";

                                if (response.ResponseErrors.Count > 0)
                                {
                                    errormessage = response.ResponseErrors.First().Message;
                                }

                                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceEndListing, string.Format("{0} - Error posting single product revise feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                                    ChannelName, product.EisSKU, errormessage, submittedBy));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var description = string.Format("Batch: {0}/{1} - Error in submitting END_ITEM feed. \nError Message: {2} \nRequested by: {3}",
                       i + 1,
                       totalBatches,
                       EisHelper.GetExceptionMessage(ex),
                       submittedBy);
                    _logger.LogError(LogEntryType.BigCommerceEndListing, description, ex.StackTrace);
                    Console.WriteLine(description);

                    // add the batched items to the list for resubmission 1 by 1
                    failedBatches.AddRange(batchedProducts);
                }
            }
        }

        public void SubmitSingleProductEndItem(ItemFeed itemFeed, string submittedBy)
        {
            try
            {
                if (string.IsNullOrEmpty(itemFeed.ItemId))
                {
                    _logger.LogInfo(LogEntryType.BigCommerceEndListing, string.Format("Unable to send END_ITEM feed for \'{0}\' since it doesn't have ItemId. \nRequested by: {1}", itemFeed.EisSKU, submittedBy));
                    return;
                }
                
                var productId = Convert.ToInt32(itemFeed.ItemId);

                var bcProductFeedResponse = _client.Products.Get(productId);

                if (bcProductFeedResponse.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var bcProductFeed = bcProductFeedResponse.Data;
                    
                    var response = _client.Products.Delete(productId);

                    if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        _logger.LogInfo(LogEntryType.BigCommerceEndListing, string.Format("Successfully posted single END_ITEM feed for {0} - {1} item. \nRequested by: {2} \nAPI Message: {3}",
                            ChannelName, itemFeed.EisSKU, submittedBy, response.RestResponse.StatusDescription));
                    }
                    else
                    {
                        var errormessage = "";

                        if (response.ResponseErrors.Count > 0)
                        {
                            errormessage = response.ResponseErrors.First().Message;
                        }

                        _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceEndListing, string.Format("{0} - Error posting single product revise feed for EisSKU \'{1}\'.\nError Message: {2} \nRequested by: {3}",
                            ChannelName, itemFeed.EisSKU, errormessage, submittedBy));
                    }
                }
            }
            catch (Exception ex)
            {
                var description = string.Format("Error in submitting END_ITEM feed for {0}. \nError Message: {2} \nRequested by: {3}",
                      itemFeed.EisSKU,
                      EisHelper.GetExceptionMessage(ex),
                      submittedBy);
                _logger.LogError(LogEntryType.BigCommerceEndListing, description, ex.StackTrace);
            }
        }

        public List<Category> GetCategoryList()
        {
            var categoryList = new List<Category>();

            categoryList = _client.Categories.GetList().ToList();

            return categoryList;
        }

        public List<ProductsImage> GetProductsImageListByProductID(int productID)
        {
            var prodImageList = new List<ProductsImage>();

            var response = _client.ProductsImages.Get(productID);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                prodImageList = response.Data.ToList();
            }

            return prodImageList;
        }

        public List<ProductsCustomField> GetProductsCustomeFieldListByProductID(int productID)
        {
            var customFieldList = new List<ProductsCustomField>();

            var response = _client.ProductsCustomFields.Get(productID);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                customFieldList = response.Data.ToList();
            }

            return customFieldList;
        }

        #endregion


        #region Private Methods

        private BigCommerce4Net.Domain.Product mapEISProductToBCProduct(MarketplaceProductFeedDto productFeed, BigCommerce4Net.Domain.Product bcProduct = null)
        {
            if (bcProduct == null)
            {
                bcProduct = new BigCommerce4Net.Domain.Product();
            }

            bcProduct.Name = productFeed.Name;
            bcProduct.BrandId = productFeed.BigCommerceProductFeed.Brand.HasValue ? productFeed.BigCommerceProductFeed.Brand.Value : 0;
            bcProduct.Description = productFeed.BigCommerceProductFeed.Description != null ? productFeed.BigCommerceProductFeed.Description: "";
            bcProduct.OpenGraphTitle = productFeed.BigCommerceProductFeed.Title != null ? productFeed.BigCommerceProductFeed.Title : "";
            bcProduct.Weight = productFeed.ItemWidth.HasValue ? productFeed.ItemWidth.Value : 0;
            bcProduct.Height = productFeed.ItemHeight.HasValue ? productFeed.ItemHeight.Value : 0;
            bcProduct.Width = productFeed.ItemWidth.HasValue ? productFeed.ItemWidth.Value : 0;
            bcProduct.Depth = productFeed.ItemLength.HasValue ? productFeed.ItemLength.Value : 0;
            bcProduct.Price = productFeed.BigCommerceProductFeed.Price.HasValue ? productFeed.BigCommerceProductFeed.Price.Value : 0;
            bcProduct.Type = (ProductsType)productFeed.BigCommerceProductFeed.ProductsType;
            bcProduct.Availability = ProductsAvailability.Available;
            bcProduct.Upc = productFeed.UPC;
            bcProduct.IsVisible = true;
            bcProduct.Sku = productFeed.EisSKU;
            bcProduct.RelatedProducts = "-1";
            bcProduct.Warranty = "";
            bcProduct.DateCreated = DateTime.Now;
            bcProduct.DateModified = DateTime.Now;
            bcProduct.AvailabilityDescription = "";
            bcProduct.BinPickingNumber = "";
            bcProduct.Condition = (ProductsCondition)Convert.ToInt32(productFeed.BigCommerceProductFeed.Condition);
            bcProduct.PageTitle = productFeed.BigCommerceProductFeed.Title != null ? productFeed.BigCommerceProductFeed.Title : "";
            bcProduct.LayoutFile = "";
            bcProduct.PriceHiddenLabel = "";
            bcProduct.OptionSetId = null;
            bcProduct.OpenGraphDescription = productFeed.BigCommerceProductFeed.Description != null ? productFeed.BigCommerceProductFeed.Description : "";
            bcProduct.PreorderMessage = "";
            bcProduct.PeachtreeGlAccount = "";
            bcProduct.MyobAssetAccount = "";
            bcProduct.MyobExpenseAccount = "";
            bcProduct.MyobIncomeAccount = "";
            bcProduct.EventDateFieldName = "";
            bcProduct.MetaDescription = "";
            bcProduct.MetaKeywords = "";
            bcProduct.SearchKeywords = "";
            bcProduct.EventDateStart = DateTime.Now;
            bcProduct.EventDateEnd = DateTime.Now;
            bcProduct.CustomUrl = "";
            bcProduct.PreorderReleaseDate = DateTime.Now;
            bcProduct.DateLastImported = DateTime.Now;
            bcProduct.TaxClassId = 1;

            // Inventory Fields
            bcProduct.InventoryLevel = productFeed.BigCommerceProductFeed.ProductQuantity;

            if (productFeed.BigCommerceProductFeed.InventoryWarningLevel.HasValue)
                bcProduct.InventoryWarningLevel = productFeed.BigCommerceProductFeed.InventoryWarningLevel.Value;

            if (productFeed.BigCommerceProductFeed.InventoryTracking.HasValue)
                bcProduct.InventoryTracking = (ProductsInventoryTracking)productFeed.BigCommerceProductFeed.InventoryTracking.Value;

            if (productFeed.BigCommerceProductFeed.OrderQuantityMinimum.HasValue)
                bcProduct.OrderQuantityMinimum = productFeed.BigCommerceProductFeed.OrderQuantityMinimum.Value;

            if (productFeed.BigCommerceProductFeed.OrderQuantityMaximum.HasValue)
                bcProduct.OrderQuantityMaximum = productFeed.BigCommerceProductFeed.OrderQuantityMaximum.Value;

            // Price Fields
            bcProduct.Price = productFeed.BigCommerceProductFeed.Price.HasValue ? productFeed.BigCommerceProductFeed.Price.Value : 0;

            bcProduct.RetailPrice = productFeed.BigCommerceProductFeed.RetailPrice.HasValue ? productFeed.BigCommerceProductFeed.RetailPrice.Value : 0;

            bcProduct.FixedCostShippingPrice = productFeed.BigCommerceProductFeed.FixedCostShippingPrice.HasValue ? productFeed.BigCommerceProductFeed.FixedCostShippingPrice.Value : 0;



            // Categories : Single Category Implementation
            //var prodTypeIdArray = new int[1];

            //if (productFeed.BigCommerceProductFeed.CategoryId.HasValue)
            //{
            //    prodTypeIdArray[0] = productFeed.BigCommerceProductFeed.CategoryId.Value;
            //}
            //else
            //{
            //    prodTypeIdArray[0] = 0;
            //}


            // Categories : Multiple Category Implementation
            var prodTypeIdArray = new int[100];

            if (!string.IsNullOrEmpty(productFeed.BigCommerceProductFeed.Categories))
            {
                prodTypeIdArray = productFeed.BigCommerceProductFeed.Categories.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            }
            else
            {
                prodTypeIdArray[0] = 0;
            }

            bcProduct.Categories = prodTypeIdArray;

            return bcProduct;
        }

        private BigCommerce4Net.Domain.Product mapEISInventoryToBCProduct(MarketplaceInventoryFeed productFeed, BigCommerce4Net.Domain.Product bcProduct)
        {
            bcProduct.Sku = productFeed.EisSKU;

            bcProduct.InventoryLevel = productFeed.BigCommerceInventoryFeed.ProductQuantity;

            if (productFeed.BigCommerceInventoryFeed.InventoryWarningLevel.HasValue)
                bcProduct.InventoryWarningLevel = productFeed.BigCommerceInventoryFeed.InventoryWarningLevel.Value;

            if (productFeed.BigCommerceInventoryFeed.InventoryTracking.HasValue)
                bcProduct.InventoryTracking = (ProductsInventoryTracking)productFeed.BigCommerceInventoryFeed.InventoryTracking.Value;

            if (productFeed.BigCommerceInventoryFeed.OrderQuantityMinimum.HasValue)
                bcProduct.OrderQuantityMinimum = productFeed.BigCommerceInventoryFeed.OrderQuantityMinimum.Value;

            if (productFeed.BigCommerceInventoryFeed.OrderQuantityMaximum.HasValue)
                bcProduct.OrderQuantityMaximum = productFeed.BigCommerceInventoryFeed.OrderQuantityMaximum.Value;

            return bcProduct;
        }

        private BigCommerce4Net.Domain.Product mapEISPriceToBCProduct(MarketplacePriceFeedDto productFeed, BigCommerce4Net.Domain.Product bcProduct)
        {
            bcProduct.Sku = productFeed.EisSKU;

            bcProduct.Price = productFeed.BigCommerceProductFeed.Price.HasValue ? productFeed.BigCommerceProductFeed.Price.Value : 0;

            bcProduct.RetailPrice = productFeed.BigCommerceProductFeed.RetailPrice.HasValue ? productFeed.BigCommerceProductFeed.RetailPrice.Value : 0;

            bcProduct.FixedCostShippingPrice = productFeed.BigCommerceProductFeed.FixedCostShippingPrice.HasValue ? productFeed.BigCommerceProductFeed.FixedCostShippingPrice.Value : 0;

            return bcProduct;
        }

        private List<BigCommerce4Net.Domain.ProductsImage> mapEISImageUrlToBCProductImage(List<string> imageUrls)
        {
            var imageList = new List<BigCommerce4Net.Domain.ProductsImage>();

            foreach (var url in imageUrls)
            {
                var prodImage = new BigCommerce4Net.Domain.ProductsImage();

                prodImage.ImageFile = url;

                imageList.Add(prodImage);
            }

            return imageList;
        }

        private Resource GetResourceImage(List<string> imageUrls)
        {
            var imageResource = new Resource();

            if (imageUrls.Count > 0)
            {
                imageResource.Url = imageUrls.First();
            }

            return imageResource;
        }

        private void UpdateEISProductID(string EISSku, int ProductId)
        {
            var eisproduct = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == EISSku);

            eisproduct.ProductId = ProductId;

            _context.SaveChanges();
        }

        private void UpdateEISProductURL(string EISSku, string seoURL)
        {
            var eisproduct = _context.productbigcommerces.FirstOrDefault(o => o.EisSKU == EISSku);

            eisproduct.ProductURL = seoURL;

            _context.SaveChanges();
        }

        private string GetProductFeedUpdateString(BigCommerce4Net.Domain.Product bcProductFeed)
        {
            var updatedata = new
            {
                name = bcProductFeed.Name,
                type = bcProductFeed.Type,
                sku = bcProductFeed.Sku,
                description = bcProductFeed.Description,
                search_keywords = bcProductFeed.SearchKeywords,
                availability_description = bcProductFeed.AvailabilityDescription,
                cost_price = bcProductFeed.CostPrice,
                sale_price = bcProductFeed.SalePrice,
                sort_order = bcProductFeed.SortOrder,
                is_visible = bcProductFeed.IsVisible,
                is_featured = bcProductFeed.IsFeatured,
                related_products = bcProductFeed.RelatedProducts,
                warranty = bcProductFeed.Warranty,
                weight = bcProductFeed.Weight,
                width = bcProductFeed.Width,
                height = bcProductFeed.Height,
                depth = bcProductFeed.Depth,
                is_free_shipping = bcProductFeed.IsFreeShipping,
                page_title = bcProductFeed.PageTitle,
                meta_keywords = bcProductFeed.MetaKeywords,
                meta_description = bcProductFeed.MetaDescription,
                layout_file = bcProductFeed.LayoutFile,
                is_price_hidden = bcProductFeed.IsPriceHidden,
                price_hidden_label = bcProductFeed.PriceHiddenLabel,
                //event_date_field_name = bcProductFeed.EventDateFieldName,
                //event_date_type = bcProductFeed.EventDateType,
                //event_date_start = bcProductFeed.EventDateStart,
                //event_date_end = bcProductFeed.EventDateEnd,
                myob_asset_account = bcProductFeed.MyobAssetAccount,
                myob_income_account = bcProductFeed.MyobIncomeAccount,
                myob_expense_account = bcProductFeed.MyobExpenseAccount,
                peachtree_gl_account = bcProductFeed.PeachtreeGlAccount,
                condition = bcProductFeed.Condition,
                is_condition_shown = bcProductFeed.IsConditionShown,
                //preorder_release_date = bcProductFeed.PreorderReleaseDate,
                is_preorder_only = bcProductFeed.IsPreorderOnly,
                preorder_message = bcProductFeed.PreorderMessage,
                open_graph_type = bcProductFeed.OpenGraphType,
                open_graph_title = bcProductFeed.OpenGraphTitle,
                open_graph_description = bcProductFeed.OpenGraphDescription,
                is_open_graph_thumbnail = bcProductFeed.IsOpenGraphThumbnail,
                upc = bcProductFeed.Upc,
                bin_picking_number = bcProductFeed.BinPickingNumber,
                availability = bcProductFeed.Availability,
                custom_url = bcProductFeed.CustomUrl,
                categories = bcProductFeed.Categories,
                brand_id = bcProductFeed.BrandId
            };

            return JsonConvert.SerializeObject(updatedata);
        }

        // Product Images
        private bool CreateProductImage(int productId, string imageUrl)
        {
            var responseResult = false;

            var createData = new
            {
                image_file = imageUrl
            };

            var response = _client.ProductsImages.Create(productId, createData);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                responseResult = true;
            }

            return responseResult;
        }

        private bool CreateProductImages(int productId, List<string> imageUrls)
        {
            var responseResult = false;

            foreach (var imageUrl in imageUrls)
            {
                var createData = new
                {
                    image_file = imageUrl
                };

                var response = _client.ProductsImages.Create(productId, createData);

                if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    responseResult = true;
                }
            }


            return responseResult;
        }

        private bool UpdateProductImage(int productId, int imageId, string imageUrl)
        {
            var responseResult = false;

            var updateData = new
            {
                image_file = imageUrl
            };

            var response = _client.ProductsImages.Update(productId, imageId, updateData);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                responseResult = true;
            }

            return responseResult;

        }

        private bool UpdateProductImages(int productId, List<ProductsImage> bgImageList, List<string> imageUrls)
        {
            var responseResult = false;
            var imageCnt = 0;

            try
            {
                foreach (var bgImage in bgImageList)
                {
                    // Defensive programming - making sure imageCnt is within index range of imageUrls list
                    if (imageCnt < imageUrls.Count)
                    {
                        var imageUrl = imageUrls[imageCnt];

                        var updateData = new
                        {
                            image_file = imageUrl
                        };

                        var response = _client.ProductsImages.Update(productId, bgImage.Id, updateData);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            responseResult = true;
                        }

                        imageCnt++;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceReviseItem, string.Format("{0} - Error updating of product images. \nError Message: {1}",
                    ChannelName, ex.Message));
            }

            return responseResult;

        }

        // Product Custom Fields
        private bool CreateProductCustomFields(int productId, List<bigcommercecustomfield> EISCustomFields)
        {
            var responseResult = false;

            foreach (var customField in EISCustomFields)
            {
                var createData = new
                {
                    name = customField.Name,
                    text = customField.Text
                };

                var response = _client.ProductsCustomFields.Create(productId, createData);

                if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    customField.CustomFieldId = response.Data.Id;

                    responseResult = true;
                }
            }

            // Save changes on the EIS Custom Field ID from API
            _context.SaveChanges();

            return responseResult;
        }

        private bool UpdateProductCustomField(int productId, List<ProductsCustomField> BCCustomFields, List<bigcommercecustomfield> EISCustomFields)
        {
            var responseResult = false;

            foreach (var eisCustomField in EISCustomFields)
            {
                if (eisCustomField.CustomFieldId.HasValue)
                {
                    var bcCustomField = BCCustomFields.FirstOrDefault(o => o.Id == eisCustomField.CustomFieldId);

                    // Update existing custom fields
                    if (bcCustomField != null)
                    {
                        var updateData = new
                        {
                            name = eisCustomField.Name,
                            text = eisCustomField.Text
                        };

                        var response = _client.ProductsCustomFields.Update(productId, bcCustomField.Id, updateData);

                        if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            responseResult = true;
                        }
                    }
                }
                else
                {
                    // Create a new custom field
                    var createData = new
                    {
                        name = eisCustomField.Name,
                        text = eisCustomField.Text
                    };

                    var response = _client.ProductsCustomFields.Create(productId, createData);

                    if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        eisCustomField.CustomFieldId = response.Data.Id;

                        responseResult = true;
                    }
                }
            }


            // Remove Deleted Custom Fields in Big Commerce Site
            var eisExistingCustomFields = EISCustomFields.Where(o => o.CustomFieldId != null).Select(o => o.CustomFieldId.Value).ToList();
            var bcCustomFieldsForDelete = BCCustomFields.Where(o => !eisExistingCustomFields.Contains(o.Id)).ToList();

            foreach (var customField in bcCustomFieldsForDelete)
            {
                var response = _client.ProductsCustomFields.Delete(productId, customField.Id);

                if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    responseResult = true;
                }
            }


            // Save changes on the EIS Custom Field ID from API
            _context.SaveChanges();

            return responseResult;

        }

        private void UpdateBigCommerceCustomField(List<bigcommercecustomfield> EISCustomFields)
        {
            foreach (var customField in EISCustomFields)
            {
                var dbCustomField = _context.bigcommercecustomfields.FirstOrDefault(o => o.Id == customField.Id);

                if (dbCustomField != null)
                {
                    dbCustomField.CustomFieldId = customField.CustomFieldId;

                    _context.SaveChanges();
                }
            }
        }

        private List<bigcommercecustomfield> GetEISCustomFields(int productID)
        {
            var customFieldList = new List<bigcommercecustomfield>();

            customFieldList = _context.bigcommercecustomfields.Where(o => o.ProductId == productID).ToList();

            return customFieldList;
        }

        #endregion
    }
}
