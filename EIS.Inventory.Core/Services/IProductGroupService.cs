using EIS.Inventory.Core.ViewModels;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public interface IProductGroupService : IDisposable
    {
        /// <summary>
        /// Get the list of all product groups
        /// </summary>
        /// <returns></returns>
        IEnumerable<ProductGroupListDto> GetAllProductGroups();

        /// <summary>
        /// Get the paginated list of product groups
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        IPagedList<ProductGroupListDto> GetPagedProductGroups(int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Get the product group with the specified id
        /// </summary>
        /// <param name="id">The product group id</param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        ProductGroupDetailDto GetProductGroupDetails(long id, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get the list of products by group id
        /// </summary>
        /// <param name="groupId">The product group id</param>
        /// <returns></returns>
        IEnumerable<ProductDto> GetProductsByGroup(long groupId);

        /// <summary>
        /// Save the product group into the database
        /// </summary>
        /// <param name="model">The prodouct group to save</param>
        /// <returns></returns>
        ProductGroupDetailDto CreateProductGroup(ProductGroupDetailDto model);

        /// <summary>
        /// Update the product group with the specified Id and updated product group
        /// </summary>
        /// <param name="id">The product group id</param>
        /// <param name="model">The updated product group to save</param>
        /// <returns></returns>
        ProductGroupDetailDto UpdateProductGroup(long id, ProductGroupDetailDto model);

        /// <summary>
        /// Delete the product group with the specified Id
        /// </summary>
        /// <param name="id">The product group id</param>
        /// <returns></returns>
        bool DeleteProductGroup(long id);

        /// <summary>
        /// Add new EIS SKUs from the list and delele the EIS SKUs which doesn't exist from the list of EIS SKUs
        /// </summary>
        /// <param name="groupId">The product group id</param>
        /// <param name="eisSkuRecords">The list of new  EIS SKUs for the product group</param>
        /// <returns>Returns the number of EIS SKUs added</returns>
        int UpdateProductGroupEisSKUs(int groupId, List<string> eisSkuRecords);
    }
}
