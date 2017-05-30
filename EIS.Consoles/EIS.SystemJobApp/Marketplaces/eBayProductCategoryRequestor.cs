using System;
using System.Collections.Generic;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.SystemJobApp.Marketplaces
{
    public class eBayProductCategoryRequestor
    {
        private readonly ApiContext _context;
        protected readonly LoggerRepository _logger;

        public eBayProductCategoryRequestor(string applicationId, string developerId, string certificateId, string userToken)
        {
            _logger = new LoggerRepository();

            var apiCredential = new ApiCredential();
            apiCredential.ApiAccount = new ApiAccount
            {
                Application = applicationId,
                Developer = developerId,
                Certificate = certificateId
            };
            apiCredential.eBayToken = userToken;

            _context = new ApiContext();
            _context.ApiCredential = apiCredential;
            _context.SoapApiServerUrl = "https://api.ebay.com/wsapi";
        }

        public eBayCategoryResult GeteBaySuggestedCategories(string eisSku, string keyword)
        {
            try
            {
                // create the API call object and pass the keyword
                var apiCall = new GetSuggestedCategoriesCall(_context);
                var categories = apiCall.GetSuggestedCategories(keyword);
                if(categories == null)
                {
                    Console.WriteLine("NO EBAY SUGGESSTED CATEGORIES FOUND FOR {0} - {1}", eisSku, keyword);
                    _logger.LogWarning(LogEntryType.eBaySuggestedCategoriesWorker, string.Format("No eBay categories found for EIS SKU: {0} - Keyword: {1}", eisSku, keyword));
                    return null;
                }

                var results = new List<eBayCategory>();
                foreach (SuggestedCategoryType item in categories)
                {
                    results.Add(new eBayCategory
                    {
                        Id = Convert.ToInt32(item.Category.CategoryID),
                        Name = string.Format("{0} > {1}", string.Join(" > ", item.Category.CategoryParentName.ToArray()),
                            item.Category.CategoryName)
                    });
                }

                return new eBayCategoryResult
                {
                    EisSKU = eisSku,
                    Categories = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.eBaySuggestedCategoriesWorker,
                    string.Format("Unexpected error in getting suggested categories for \"{0}\". Err Message: {1}", keyword, EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return null;
            }
        }
    }
}
