using BigCommerce4Net.Api;
using EIS.Inventory.Shared.ViewModels;
using EIS.SchedulerTaskApp.Repositories;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models;
using System;
using EIS.Inventory.Shared.Helpers;
using System.Linq;
using BigCommerce4Net.Domain;

namespace EIS.SchedulerTaskApp.Marketplaces
{
    public class BigCommerceProductInventory : IMarketplaceProductInventory
    {
        private Configuration _apiConfiguration;
        private Client _client;
        private BigCommerceCredentialDto _credential;
        private readonly string _submittedBy;

        public BigCommerceProductInventory(string submittedBy, string filePath)
        {
            _submittedBy = submittedBy;
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

        public void SubmitProductsInventoryFeed(List<MarketplaceInventoryFeed> inventoryFeeds)
        {
            try
            {
                // take out the products which has no product type id or no information for Big Commerce
                var invalidProducts = inventoryFeeds
                     .Where(x => x.IsBlacklisted 
                         || x.BigCommerceInventoryFeed == null 
                         || !x.BigCommerceInventoryFeed.ProductId.HasValue
                         || !x.BigCommerceInventoryFeed.IsEnabled)
                    .ToList();

                if (invalidProducts.Any())
                {
                    Logger.LogWarning(LogEntryType.BigCommerceProductListing, string.Format("{0}/{1} EIS products which will not be included to BigCommerce product listing feed due to no product ID or no BigCommerce information or black listed.",
                        invalidProducts.Count, inventoryFeeds.Count));
                    inventoryFeeds.RemoveAll(x => x.IsBlacklisted 
                                            || x.BigCommerceInventoryFeed == null 
                                            || !x.BigCommerceInventoryFeed.ProductId.HasValue
                                            || !x.BigCommerceInventoryFeed.IsEnabled);
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

                Logger.LogInfo(LogEntryType.BigCommerceInventoryUpdate, string.Format("{0} - Successfully posted product inventory feed for {1} items. \nRequested by: {2}",
                    ChannelName, successCount, _submittedBy));

                var unsuccessfulPostingCount = inventoryFeeds.Count - successCount;
                if (unsuccessfulPostingCount > 0)
                {
                    Logger.LogError(LogEntryType.BigCommerceInventoryUpdate, string.Format("Error posting product inventory feed for {0} - {1} product items. \nRequested by: {2}",
                        ChannelName, unsuccessfulPostingCount, _submittedBy));
                }

            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting product inventory feed for {3} items. \nError Message: {1} \nRequested by: {2}",
                        ChannelName,
                        EisHelper.GetExceptionMessage(ex),
                        _submittedBy,
                        inventoryFeeds.Count);
                Logger.LogError(LogEntryType.BigCommerceInventoryUpdate, description, ex.StackTrace);
            }
        }


        #endregion

        #region Private Methods

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

        #endregion

    }
}
