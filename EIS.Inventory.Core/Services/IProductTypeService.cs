using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using System;

namespace EIS.Inventory.Core.Services
{
    public interface IProductTypeService : IDisposable
    {
        /// <summary>
        /// Gets the list of all eisProduct types
        /// </summary>
        /// <returns></returns>
        IEnumerable<ProductTypeViewModel> GetAllProductTypes();

        /// <summary>
        /// Get the list all mapped eisProduct types
        /// </summary>
        /// <returns></returns>
        IEnumerable<ProductTypeViewModel> GetMappedProductTypes();
        
        /// <summary>
        /// Gets the eisProduct type with the specified id
        /// </summary>
        /// <param name="id">The eisProduct type id</param>
        /// <returns></returns>
        ProductTypeViewModel GetProductType(int id);

        /// <summary>
        /// Gets the list of all Amazon main categories
        /// </summary>
        /// <returns></returns>
        IEnumerable<CategoryViewModel> GetAmazonMainCategories();

        /// <summary>
        /// Get the list of all sub-categories of Amazon with the specified parent code
        /// </summary>
        /// <param name="parentCode">The parent code</param>
        /// <returns></returns>
        IEnumerable<CategoryViewModel> GetAmazonSubCategories(string parentCode);

        /// <summary>
        /// Gets the list of all Ebay main categories
        /// </summary>
        /// <returns></returns>
        IEnumerable<CategoryViewModel> GetEbayMainCategories();

        /// <summary>
        /// Get the list of all sub-categories of Ebay with the specified parent code
        /// </summary>
        /// <param name="parentCode">The parent code</param>
        /// <returns></returns>
        IEnumerable<CategoryViewModel> GetEbaySubCategories(string parentCode);

        /// <summary>
        /// Create new eisProduct type in the database
        /// </summary>
        /// <param name="viewModel">The view model to save</param>
        /// <returns></returns>
        ProductTypeViewModel CreateProductType(ProductTypeViewModel viewModel);

        /// <summary>
        /// Update the eisProduct type with the specified id and the updated view model
        /// </summary>
        /// <param name="id">The eisProduct type to update</param>
        /// <param name="viewModel">The updated view model to save</param>
        /// <returns></returns>
        ProductTypeViewModel UpdateProductType(int id, ProductTypeViewModel viewModel);

        /// <summary>
        /// Delete the eisProduct type with the specified id
        /// </summary>
        /// <param name="id">The eisProduct type id to delete</param>
        /// <returns></returns>
        bool DeleteProductType(int id);
        
        /// <summary>
        /// Get the list of mapped categories with the specified eisProduct type id
        /// </summary>
        /// <param name="productTypeId">The eisProduct type id</param>
        /// <returns></returns>
        IEnumerable<ProductTypeCategoryViewModel> GetProductCategoryMappings(int productTypeId);

        /// <summary>
        /// Get the list of all unmapped eisProduct categories
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetUnMappedProductCategories();

        /// <summary>
        /// Insert a new eisProduct type catogegory mapping to the database
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        bool AddProductCategories(int productTypeId, List<string> categories);

        /// <summary>
        /// Delete the specified eisProduct category mapping from the database
        /// </summary>
        /// <param name="viewModel">The mapping view model to delete</param>
        /// <returns></returns>
        bool DeleteProductCategoryMapping(int productTypeId, string category);

        /// <summary>
        /// Determine if the product type name already exists in the database otherwise create
        /// </summary>
        /// <param name="productTypeName">The name of the product type</param>
        /// <param name="productGroup">The name of the product group</param>
        /// <returns>Returns the ID of product type</returns>
        int? ConfigureProductTypeName(string productTypeName, string productGroup);

        /// <summary>
        /// Get the list of eBay categories
        /// </summary>
        /// <returns></returns>
        IEnumerable<MarketplaceCategoryDto> GeteBayCategories();

        /// <summary>
        /// Get the list of Big Commerce categories
        /// </summary>
        /// <returns></returns>
        IEnumerable<MarketplaceCategoryDto> GetBigCommerceCategories();
    }
}
