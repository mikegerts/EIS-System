using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.Inventory.Core;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Marketplace.eBay.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;
using System.Linq;

namespace EIS.Marketplace.eBay
{
    [Export(typeof(IMarketplaceProductProvider))]
    public class eBayProductProvider : IMarketplaceProductProvider
    {
        private eBayCredentialDto _credential;
        private ILogService _logger;

        public eBayProductProvider() : this (Core.Get<ILogService>())
        {
        }

        public eBayProductProvider(ILogService logger)
        {
            _logger = logger;
        }

        public string ChannelName
        {
            get { return "eBay"; }
        }

        public CredentialDto MarketplaceCredential
        {
            get { return _credential; }
            set
            {
                _credential = value as eBayCredentialDto;
                RequestHelper.SetCredentials(_credential);
            }
        }

        public MarketplaceProduct GetProductInfo(AmazonInfoFeed infoFeed)
        {
            throw new NotImplementedException();
        }

        public List<MarketplaceCategoryDto> GetSuggestedCategories(string keyword)
        {
            // create and init the API Context
            var context = new ApiContext();
            context.ApiCredential = RequestHelper.ApiCredential;
            context.SoapApiServerUrl = RequestHelper.ServiceUrl;

            try
            {
                // create the API call object and pass the keyword
                var apiCall = new GetSuggestedCategoriesCall(context);
                var categories = apiCall.GetSuggestedCategories(keyword);

                var results = new List<MarketplaceCategoryDto>();
                foreach (SuggestedCategoryType item in categories)
                {
                    results.Add(new MarketplaceCategoryDto
                    {
                        Id = Convert.ToInt32(item.Category.CategoryID),
                        Name = string.Format("{0} > {1}", string.Join(" > ", item.Category.CategoryParentName.ToArray()),
                            item.Category.CategoryName)
                    });
                }

                return results.OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.eBayProductProvider,
                    string.Format("Unexpected error in getting suggested categories for \"{0}\". Err Message: {1}", keyword, EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return null;
            }
        }

        public bool SetMarketplaceUserPreference(UserPreferenceFeed preference)
        {
            // create and init the API Context
            var context = new ApiContext();
            context.ApiCredential = RequestHelper.ApiCredential;
            context.SoapApiServerUrl = RequestHelper.ServiceUrl;

            try
            {
                // create the preference feed
                var preferenceType = new SetUserPreferencesRequestType
                {
                    OutOfStockControlPreference = preference.OutOfStockControlPreference,
                    OutOfStockControlPreferenceSpecified = preference.isOutOfStockControlPreference,
                };

                var apiCall = new SetUserPreferencesCall(context);
                var response = apiCall.ExecuteRequest(preferenceType);

                if (response.Ack == AckCodeType.Success)
                    return true;

                // log the error
                _logger.LogError(LogEntryType.eBayProductProvider,
                    string.Format("{0} - Error in sending user preference - Message: {1}", ChannelName, response.Errors[0].LongMessage), 
                    string.Join(",", response.Errors[0].ErrorParameters));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.eBayProductProvider,
                    string.Format("Unexpected error in sending user preference\". Err Message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return false;
            }
        }

        public bool GetMarketplaceUserPreference()
        {
            // create and init the API Context
            var context = new ApiContext();
            context.ApiCredential = RequestHelper.ApiCredential;
            context.SoapApiServerUrl = RequestHelper.ServiceUrl;

            try
            {
                // create the preference feed
                var preferenceType = new GetUserPreferencesRequestType
                {
                    ShowOutOfStockControlPreference = true,
                    ShowSellerPaymentPreferences = true,
                };

                var apiCall = new GetUserPreferencesCall(context);
                var response = apiCall.ExecuteRequest(preferenceType);

                if (response.Ack == AckCodeType.Success)
                    return true;

                // log the error
                _logger.LogError(LogEntryType.eBayProductProvider,
                    string.Format("{0} - Error in getting user preference - Message: {1}", ChannelName, response.Errors[0].LongMessage),
                    string.Join(",", response.Errors[0].ErrorParameters));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.eBayProductProvider,
                    string.Format("Unexpected error in getting user preference\". Err Message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return false;
            }
        }
    }
}
