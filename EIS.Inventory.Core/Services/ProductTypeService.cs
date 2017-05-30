using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class ProductTypeService : IProductTypeService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public ProductTypeService(ILogService logger)
        {
            _context = new EisInventoryContext();
            _logger = logger;
        }

        public IEnumerable<ProductTypeViewModel> GetAllProductTypes()
        {
            var productTypes = _context.producttypes
                .OrderBy(x => x.AmazonMainCategoryCode)
                .ToList();

            return Mapper.Map<IEnumerable<producttype>, IEnumerable<ProductTypeViewModel>>(productTypes);
        }

        public IEnumerable<ProductTypeViewModel> GetMappedProductTypes()
        {
            var productTypes = _context.producttypecategorymappings
                .Select(x => x.producttype)
                .ToList();

            return Mapper.Map<IEnumerable<producttype>, IEnumerable<ProductTypeViewModel>>(productTypes);
        }

        public ProductTypeViewModel GetProductType(int id)
        {
            var productType = _context.producttypes.Find(id);

            return Mapper.Map<producttype, ProductTypeViewModel>(productType);
        }
        public IEnumerable<CategoryViewModel> GetAmazonMainCategories()
        {
            var categories = _context.amazoncategories.ToList();

            return Mapper.Map<IEnumerable<amazoncategory>, IEnumerable<CategoryViewModel>>(categories);
        }

        public IEnumerable<CategoryViewModel> GetAmazonSubCategories(string parentCode)
        {
            var subCategories = _context.amazonsubcategories.Where(x => x.ParentCode == parentCode)
                .ToList();

            return Mapper.Map<IEnumerable<amazonsubcategory>, IEnumerable<CategoryViewModel>>(subCategories);
        }

        public IEnumerable<CategoryViewModel> GetEbayMainCategories()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<CategoryViewModel> GetEbaySubCategories(string parentCode)
        {
            throw new System.NotImplementedException();
        }

        public ProductTypeViewModel CreateProductType(ProductTypeViewModel viewModel)
        {
            var productType = Mapper.Map<ProductTypeViewModel, producttype>(viewModel);

            _context.producttypes.Add(productType);
            _context.SaveChanges();

            return Mapper.Map<producttype, ProductTypeViewModel>(productType);
        }

        public ProductTypeViewModel UpdateProductType(int id, ProductTypeViewModel viewModel)
        {
            var oldProductTYpe = _context.producttypes.Find(id);

            _context.Entry(oldProductTYpe).CurrentValues.SetValues(viewModel);
            _context.SaveChanges();

            return viewModel;
        }

        public bool DeleteProductType(int id)
        {
            var productType = _context.producttypes.Find(id);
            if (productType == null)
                return true;

            _context.producttypes.Remove(productType);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<ProductTypeCategoryViewModel> GetProductCategoryMappings(int productTypeId)
        {
            // get the eisProduct type mapping for the for the specified eisProduct type id
            var mappings = _context.producttypecategorymappings.Where(x => x.ProductTypeId == productTypeId);

            // convert it to view models
            var viewModels = Mapper.Map<IEnumerable<producttypecategorymapping>, IEnumerable<ProductTypeCategoryViewModel>>(mappings);

            // get the eisProduct which under to the mapped categories
            foreach (var model in viewModels)
            {
                var productNames = _context.products
                    .Where(x => x.Category == model.Category)
                    .GroupBy(x => x.Name)
                    .Select(g => g.FirstOrDefault().Name)
                    .Take(5)
                    .ToList();
                model.Products = productNames;
            }

            return viewModels;
        }

        public IEnumerable<string> GetUnMappedProductCategories()
        {
            // get the uniqued mapped categories
            var mappedCategories = _context.producttypecategorymappings
                .GroupBy(x => x.Category)
                .Select(g => g.FirstOrDefault().Category);

            // get all unique categories in all products
            var categories = _context.products
                .GroupBy(x => x.Category)
                .Select(g => g.FirstOrDefault().Category);

            return categories.Where(x => !mappedCategories.Contains(x));
        }

        public bool AddProductCategories(int productTypeId, List<string> categories)
        {
            foreach (var category in categories)
            {
                var mapping = new producttypecategorymapping
                {
                    ProductTypeId = productTypeId,
                    Category = category
                };
                _context.producttypecategorymappings.Add(mapping);
            }

            _context.SaveChanges();

            return true;
        }

        public bool DeleteProductCategoryMapping(int productTypeId, string category)
        {
            var mapping = _context.producttypecategorymappings.Find(productTypeId, category);
            if (mapping == null)
                return true;

            _context.producttypecategorymappings.Remove(mapping);
            _context.SaveChanges();

            return true;
        }

        public int? ConfigureProductTypeName(string productTypeName, string productGroup)
        {
            // check if the product type name exist in Amazon sub-category
            var subCategory = _context.amazonsubcategories.FirstOrDefault(x => x.Name.Equals(productTypeName, StringComparison.InvariantCultureIgnoreCase));
            if (subCategory == null)
            {
                _logger.LogWarning(LogEntryType.ProductTypeService, string.Format("ProductType: {0} - ProductGroup: {1} not found in Amazon Sub-Category", productTypeName, productGroup));
                return null; // should null for the unknown product type for now
            }

            // get the product type if it has and return its ID
            var productType = _context.producttypes
                .FirstOrDefault(x => x.AmazonMainCategoryCode == subCategory.ParentCode && x.AmazonSubCategoryCode == subCategory.Code);
            if (productType != null)
                return productType.Id;

            // otherwise, let's create new product type
            var newProductType = new producttype
            {
                TypeName = string.Format("{0} - {1}", subCategory.amazoncategory.Name, subCategory.Name),
                AmazonMainCategoryCode = subCategory.ParentCode,
                AmazonSubCategoryCode = subCategory.Code,
            };

            // add save it to database
            _context.producttypes.Add(newProductType);
            _context.SaveChanges();

            return newProductType.Id;
        }

        public IEnumerable<MarketplaceCategoryDto> GeteBayCategories()
        {
            // get the categories that are enabled only
            var categories = _context.ebaystructuredcategories
                .Where(x => x.IsEnabled);

            return Mapper.Map<IEnumerable<MarketplaceCategoryDto>>(categories);
        }
        
        public IEnumerable<MarketplaceCategoryDto> GetBigCommerceCategories()
        {
            var categories = _context.bigcommercecategories.Where(o => o.ParentId != 0).ToList();


            var marketplacecategoryList = new List<MarketplaceCategoryDto>();

            // Arrange Parent and SubCategory
            foreach(var category in categories)
            {
                var parentCategory = _context.bigcommercecategories.FirstOrDefault(o => o.Id == category.ParentId);

                marketplacecategoryList.Add(new MarketplaceCategoryDto() {
                    Id = category.Id,
                    Name = category.Name,
                    ParentName = parentCategory != null ? parentCategory.Name : ""
                });
            }

            return marketplacecategoryList;
        }
        

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
