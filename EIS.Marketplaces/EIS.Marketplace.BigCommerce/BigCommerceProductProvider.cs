using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using EIS.Inventory.Core;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;
using BigCommerce4Net.Api;
using BigCommerce4Net.Domain;
using System.Linq;
using EIS.Inventory.DAL.Database;

namespace EIS.Marketplace.BigCommerce
{
    [Export(typeof(IMarketplaceProductProvider))]
    public class BigCommerceProductProvider : IMarketplaceProductProvider
    {
        private Configuration _apiConfiguration;
        private Client _client;
        private BigCommerceCredentialDto _credential;
        private ILogService _logger;
        EisInventoryContext _context;

        public BigCommerceProductProvider()
        {
            _context = new EisInventoryContext();
            _logger = Core.Get<ILogService>();
        }

        #region Public Methods

        public string ChannelName
        {
            get { return "BigCommerce"; }
        }

        public CredentialDto MarketplaceCredential
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

        public bool GetMarketplaceUserPreference()
        {
            throw new NotImplementedException();
        }

        public MarketplaceProduct GetProductInfo(AmazonInfoFeed infoFeed)
        {
            throw new NotImplementedException();
        }

        public List<MarketplaceCategoryDto> GetSuggestedCategories(string keyword)
        {
            try
            {
                var categories = GetSuggestedEISCategoryByKeyword(keyword);

                var results = new List<MarketplaceCategoryDto>();
                foreach (var item in categories)
                {
                    var parentList = GetEISParentHierarchy(item.Id);

                    results.Add(new MarketplaceCategoryDto
                    {
                        Id = item.Id,
                        Name = string.Format("{0} {1}", parentList.Count > 0 ? string.Join(" > ", parentList.ToArray()) + " >" : "", item.Name),
                        ParentName = string.Format("{0}", parentList.Count > 0 ? parentList[0] : item.Name)
                    });
                }

                return results.OrderBy(o => o.ParentName).ThenBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.BigCommerceProductProvider,
                    string.Format("Unexpected error in getting suggested categories for \"{0}\". Err Message: {1}", keyword, EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return null;
            }
        }

        public bool SetMarketplaceUserPreference(UserPreferenceFeed preference)
        {
            throw new NotImplementedException();
        }
         
        public List<Category> GetCategoryList()
        {
            var categoryList = new List<Category>();

            categoryList = _client.Categories.GetList().ToList();

            return categoryList;
        }

        public List<bigcommercecategory> GetEISCategoryList()
        {
            var categoryList = new List<bigcommercecategory>();

            categoryList = _context.bigcommercecategories.ToList();

            return categoryList;
        }

        public List<Category> GetCategoryList(IFilter filter)
        {
            var categoryList = new List<Category>();

            categoryList = _client.Categories.GetList(filter).ToList();

            return categoryList;
        }

        #endregion

        #region Private Methods

        private List<Category> GetSuggestedCategoryByKeyword(string keyword)
        {
            List<Category> categoryList = new List<Category>();

            categoryList = GetCategoryList();

            // Filter keyword with similar names
            categoryList = categoryList.Where(o => o.Name.Contains(keyword.ToUpper())).ToList();

            return categoryList;
        }

        private List<bigcommercecategory> GetSuggestedEISCategoryByKeyword(string keyword)
        {
            List<bigcommercecategory> categoryList = new List<bigcommercecategory>();

            categoryList = GetEISCategoryList();

            if(keyword.ToUpper() != "ALL")
            {
                var categoryId = 0;
                if(int.TryParse(keyword, out categoryId))
                {
                    // Filter keyword by ID
                    categoryList = categoryList.Where(o => o.Id == categoryId).ToList();
                } else
                {
                    // Filter keyword with similar names
                    categoryList = categoryList.Where(o => o.Name.Contains(keyword.ToUpper())).ToList();
                }
            }

            return categoryList;
        }

        private List<string> GetParentNameList(List<int> categoryListId)
        {
            var parentNameList = new List<string>();

            foreach(var categoryId in categoryListId)
            {
                var filter = new FilterCategories
                {
                    MinimumId = categoryId,
                    MaximumId = categoryId
                };

                var categoryObject = _client.Categories.GetList(filter);

                if(categoryObject.Count > 0)
                {
                    parentNameList.Add(categoryObject.FirstOrDefault().Name);
                }

            }

            return parentNameList;
        }

        private List<string> GetEISParentHierarchy(int categoryId)
        {
            var retValue = new List<string>();

            var categoryObject = _context.bigcommercecategories.FirstOrDefault(o => o.Id == categoryId);

            if(categoryObject != null)
            {
                var parentId = categoryObject.ParentId;

                while(parentId != 0)
                {
                    var parentobject = _context.bigcommercecategories.FirstOrDefault(o => o.Id == parentId);

                    if(parentobject != null)
                    {
                        retValue.Add(parentobject.Name);
                        parentId = parentobject.ParentId;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            retValue.Reverse();

            return retValue;
        }

        #endregion
    }
}
