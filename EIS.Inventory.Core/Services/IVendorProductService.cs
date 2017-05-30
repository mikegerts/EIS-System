using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public interface IVendorProductService : IDisposable
    {
        /// <summary>
        /// Get the paged vendor products with the specified parameters
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="searchString">The keyword to search for product's details</param>
        /// <param name="vendorId">The id of the product's vendor</param>
        /// <param name="companyId">The id of the product's compony</param>
        /// <param name="withEisSKULink">Whether to include with EIS SKU links</param>
        /// <param name="inventoryQtyFrom">The starting quantity of product</param>
        /// <param name="inventoryQtyTo">The ending quantity of the product</param>
        /// <returns></returns>
        IPagedList<VendorProductListDto> GetPagedVendorProducts(int page,
            int pageSize,
            string searchString,
            int vendorId,
            int companyId,
            int withEisSKULink,
            int inventoryQtyFrom,
            int inventoryQtyTo,
            int withImages);

        /// <summary>
        /// Get the filtered vendor products with the specified criteria
        /// </summary>
        /// <param name="model">The user's criteria for the vendor products</param>
        /// <returns></returns>
        List<string> GetVendorProductsEisSupplierSKUs(VendorProductFilterDto model);

        /// <summary>
        /// Get the vendor product with the specified EIS SKU
        /// </summary>
        /// <param name="eisSupplierSKU">The id of EIS vendor product</param>
        /// <returns></returns>
        VendorProductDto GetVendorProduct(string eisSupplierSKU);

        /// <summary>
        ///  Insert new vendor product into the database
        /// </summary>
        /// <param name="model">The vendor producct infor to save</param>
        /// <returns></returns>
        bool CreateVendorProduct(VendorProductDto model);

        /// <summary>
        /// Update the vendor product with the updated model
        /// </summary>
        /// <param name="eisSupplierSKU">The id fo EIS vendor product</param>
        /// <param name="model">The updated model</param>
        /// <returns></returns>
        bool UpdateVendorProduct(string eisSupplierSKU, VendorProductDto model);
        
        /// <summary>
        /// Delete the vendor product with the specified EIS SKU and supplier SKU
        /// </summary>
        /// <param name="eisSupplierSKU">The id of EIS vendor product</param>
        /// <returns></returns>
        bool DeleteVendorProduct(string eisSupplierSKU);

        /// <summary>
        /// Determine if this vendor product already exist
        /// </summary>
        /// <param name="eisSupplierSKU">The EIS supplier SKU</param>
        /// <returns></returns>
        bool IsEisSupplierSKUExists(string eisSupplierSKU);

        /// <summary>
        /// Search EIS products which contains the specified keyword
        /// </summary>
        /// <param name="keyword">The keyword for the product</param>
        /// <returns></returns>
        IEnumerable<ProductResultDto> SearchEisProducts(string keyword);

        /// <summary>
        /// Search vendor products which contains the specified keyword
        /// </summary>
        /// <param name="keyword">The keyword for the product</param>
        /// <returns></returns>
        IEnumerable<VendorProductResultDto> SearchVendorProducts(string keyword);

        /// <summary>
        /// Add vendor product link and create EIS product if no match with UPC code
        /// </summary>
        /// <param name="vendorProduct"></param>
        /// <returns></returns>
        string AddLinkAndCreateEisProductIfNoMatchWithUPC(VendorProduct vendorProduct);

        /// <summary>
        /// Delete the old product links which are not exist on the compatible EisSKU lists
        /// </summary>
        /// <param name="eisSupplierSKU">The EisSupplierSKU to update</param>
        /// <param name="compatibleEisProductSKUs">The new EisSKUs list which are currently link</param>
        void DeleteOldVendorProductLinks(string eisSupplierSKU, List<string> compatibleEisProductSKUs);

        /// <summary>
        /// Update EIS prouct links and resturn what action has taken
        /// </summary>
        /// <param name="eisSupplierSKU"></param>
        /// <param name="upc"></param>
        /// <param name="minPack"></param>
        /// <returns></returns>
        UploadResultType UpdateEisProductLinks(string eisSupplierSKU, string upc, int minPack);

        /// <summary>
        /// Get the list of eisVendor Product's images with the specified EIS SKU code
        /// </summary>
        /// <param name="eisSku">The eisVendorProduct EIS SKU code</param>
        /// <returns></returns>
        IEnumerable<MediaContent> GetVendorProductImages(string eisSupplierSku);

        /// <summary>
        /// Get the Vendor product image with the specified product image id
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <returns></returns>
        MediaContent GetVendorProductImage(long id);

        /// <summary>
        /// Delete the Vendor product image with the specified id
        /// </summary>
        /// <param name="id">The id of the vendor product image</param>
        /// <param name="eisSupplierSKU">The EIS Supplier SKU</param>
        /// <returns></returns>
        bool DeleteVendorProductImage(long id);

        /// <summary>
        /// Update the vendor product images and delete the old Amazon images
        /// </summary>
        /// <param name="imageUrls">The list of image URLs</param>
        /// <param name="eisSku">The EIS Supplier SKU code of the product</param>
        void UpdateVendorProductImages(List<MediaContent> imageUrls, string eisSku);

        /// <summary>
        /// Save the image to the database
        /// </summary>
        /// <param name="image"></param>
        void AddVendorProductImage(MediaContent image);

        /// <summary>
        /// Update the image details
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <param name="fileName">The filename of the image</param>
        /// <param name="caption">The caption for the image</param>
        void UpdateVendorProductImage(long id, string fileName, string caption);
    }
}
