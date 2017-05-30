using System;
using System.Collections.Generic;
using System.Linq;
using BigCommerce4Net.Domain;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Core.ViewModels;
using AutoMapper;
using BigCommerce4Net.Api;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace EIS.Inventory.Core.Services
{
    public class BigCommerceService 
    {
        private BigCommerce4Net.Api.Configuration _apiConfiguration;
        private Client _client;

        EisInventoryContext _context;

        public BigCommerceService()
        {
            _context = new EisInventoryContext();

            var credential = GetBigCommerceCredential();

            // Apply Configuration once credentials are set
            _apiConfiguration = new BigCommerce4Net.Api.Configuration()
            {
                ServiceURL = credential.ServiceEndPoint,
                UserName = credential.Username,
                UserApiKey = credential.ApiKey,
                MaxPageLimit = 250,
                AllowDeletions = true // Is false by default, must be true to allow deletions
            };

            _client = new Client(_apiConfiguration);
        }
        
        #region Public Methods
                
        // Categories
        public List<Category> GetCategoryList()
        {
            var categoryList = new List<Category>();

            categoryList = _client.Categories.GetList().ToList();

            return categoryList;
        }

        public string GetCategoryNameById(int categoryId)
        {
            var retValue = "";

            var categoryObject = _context.bigcommercecategories.FirstOrDefault(o => o.Id == categoryId);

            if(categoryObject != null)
            {
                retValue = categoryObject.Name;
            }

            return retValue;
        }

        public IEnumerable<BigCommerceCategoryDto> GetEISCategoryList()
        {
            var categoryList = new List<bigcommercecategory>();

            categoryList = _context.bigcommercecategories.ToList();

            var mappedcategoryList = Mapper.Map<IEnumerable<bigcommercecategory>, IEnumerable<BigCommerceCategoryDto>>(categoryList);

            return mappedcategoryList;
        }

        public bool UpdateEISBigCommerceCategory()
        {
            var success = false;

            var categoryList = GetCategoryList();

            foreach(var category in categoryList)
            {
                var eisCategory = getCategoryExistingById(category.Id);
                var isnew = false;

                if(eisCategory == null)
                {
                    eisCategory = new bigcommercecategory();
                    isnew = true;
                }

                // Map Big Commerce Category API to Eis Big Commerce Category
                eisCategory = convertAPICategoryToEISCategory(eisCategory, category);

                if (isnew)
                {
                    _context.bigcommercecategories.Add(eisCategory);
                }

            }

            if (_context.SaveChanges() > 0)
                success = true;

            return success;
        }

        public List<string> GetEISParentHierarchy(int categoryId)
        {
            var retValue = new List<string>();

            var categoryObject = _context.bigcommercecategories.FirstOrDefault(o => o.Id == categoryId);

            if (categoryObject != null)
            {
                var parentId = categoryObject.ParentId;

                while (parentId != 0)
                {
                    var parentobject = _context.bigcommercecategories.FirstOrDefault(o => o.Id == parentId);

                    if (parentobject != null)
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

        public List<BigCommerceCategoryOrderedList> GetCategoryOrderedList(List<BigCommerceCategoryDto> Categories)
        {
            var orderedList = new List<BigCommerceCategoryOrderedList>();

            foreach (var item in Categories)
            {
                var parentList = GetEISParentHierarchy(item.Id);

                orderedList.Add(new BigCommerceCategoryOrderedList
                {
                    Id = item.Id,
                    Name = string.Format("{0} {1}", parentList.Count > 0 ? string.Join(" > ", parentList.ToArray()) + " >" : "", item.Name),
                    ParentName = string.Format("{0}", parentList.Count > 0 ? parentList[0] : item.Name)
                });
            }

            return orderedList.OrderBy(o => o.ParentName).ThenBy(o => o.Name).ToList();
        }

        // Brands
        public List<Brand> GetBrandList()
        {
            var brandList = new List<Brand>();

            brandList = _client.Brands.GetList().ToList();

            return brandList;
        }

        public IEnumerable<BigCommerceBrandDto> GetEISBrandList()
        {
            var brandList = new List<bigcommercebrand>();

            brandList = _context.bigcommercebrands.ToList();

            var mappedbrandList = Mapper.Map<IEnumerable<bigcommercebrand>, IEnumerable<BigCommerceBrandDto>>(brandList);

            return mappedbrandList;
        }

        public bool UpdateEISBigCommerceBrand()
        {
            var success = false;

            var brandList = GetBrandList();

            foreach (var brand in brandList)
            {
                var eisBrand = getBrandExistingById(brand.Id);
                var isnew = false;

                if (eisBrand == null)
                {
                    eisBrand = new bigcommercebrand();
                    isnew = true;
                }

                // Map Big Commerce Category API to Eis Big Commerce Category
                eisBrand = convertAPIBrandToEISBrand(eisBrand, brand);

                if (isnew)
                {
                    _context.bigcommercebrands.Add(eisBrand);
                }

            }

            if (_context.SaveChanges() > 0)
                success = true;

            return success;
        }

        public IEnumerable<BigCommerceCustomFieldDto> GetProductCustomFields(int productID)
        {
            IEnumerable<BigCommerceCustomFieldDto> eisCustomFieldList = null;
            var customFieldList = _context.bigcommercecustomfields.Where(o => o.ProductId == productID);

            if(customFieldList.Count() > 0)
            {
                eisCustomFieldList = Mapper.Map<IEnumerable<bigcommercecustomfield>, IEnumerable<BigCommerceCustomFieldDto>>(customFieldList);
            }

            return eisCustomFieldList;
        }

        #endregion

        #region Private Methods

        // Categories
        private bigcommercecategory getCategoryExistingById(int categoryId)
        {
            var bcCategory = _context.bigcommercecategories.FirstOrDefault(o => o.Id == categoryId);

            return bcCategory;
        }

        private bigcommercecategory convertAPICategoryToEISCategory(bigcommercecategory eisCategory, Category apiCategory)
        {
            eisCategory.Id = apiCategory.Id;
            eisCategory.Name = apiCategory.Name;
            eisCategory.ParentId = apiCategory.ParentId;
            eisCategory.Description = apiCategory.Description;
            eisCategory.Url = apiCategory.ImageFile;

            return eisCategory;
        }

        // Brands
        private bigcommercebrand getBrandExistingById(int brandId)
        {
            var bcBrand = _context.bigcommercebrands.FirstOrDefault(o => o.Id == brandId);

            return bcBrand;
        }

        private bigcommercebrand convertAPIBrandToEISBrand(bigcommercebrand eisBrand, Brand apiBrand)
        {
            eisBrand.Id = apiBrand.Id;
            eisBrand.Name = apiBrand.Name;
            eisBrand.PageTitle = apiBrand.PageTitle;
            eisBrand.ImageFile = apiBrand.ImageFile;

            return eisBrand;
        }

        private bigcommercecredential GetBigCommerceCredential()
        {
            var marketplacemode = ConfigurationManager.AppSettings["MarketplaceMode"];
            var credential = _context.credentials.FirstOrDefault(o => o.MarketplaceType == "BigCommerce" && o.Mode == marketplacemode);

            return (bigcommercecredential)credential;
        }

        #endregion

    }

    public class BigCommerceCategoryOrderedList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
    }
}
