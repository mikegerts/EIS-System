using System;
using System.Collections.Generic;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public interface IProductService : IDisposable
    {
        /// <summary>
        /// Get the paged product list
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="searchString">The keyword to search for product's details</param>
        /// <param name="companyId">The id of the product's compony</param>
        /// <param name="vendorId">The id of the product's vendor</param>
        /// <param name="inventoryQtyFrom">The starting quantity of product</param>
        /// <param name="inventoryQtyTo">The ending quantity of the product</param>
        /// <param name="productGroupId">The id of the product group details</param>
        /// <param name="withImages">Boolean whether to include products' with image or not</param>
        /// <param name="isKit"></param>
        /// <param name="skuType">The SKU type of the product</param>
        /// <param name="isSKULinked">Determine if the product is linked with the vendor products</param>
        /// <param name="isAmazonEnabled">Boolean to determine of its is enable in Amazon</param>
        /// <param name="hasASIN">Boolean to determine if the product has ASIN or not.</param>
        /// <returns></returns>
        IPagedList<ProductListDto> GetPagedProducts(int page,
            int pageSize,
            string searchString,
            int companyId,
            int vendorId,
            int inventoryQtyFrom,
            int inventoryQtyTo,
            int productGroupId,
            int withImages,
            bool? isKit,
            SkuType? skuType,
            bool? isSKULinked,
            bool? isAmazonEnabled,
            bool? hasASIN);

        /// <summary>
        /// Get the the list of product post feed
        /// </summary>
        /// <param name="feed">Contains the search criteria for retrieving the products to post</param>
        /// <returns></returns>
        IEnumerable<MarketplaceProductFeedDto> GetProductPostFeeds(MarketplaceFeed feed);
        
        /// <summary>
        /// Get the list of marketplace inventory item to update its quantities
        /// </summary>
        /// <param name="feed">Contains the parameter for retrieving the products to post</param>
        /// <returns></returns>
        IEnumerable<MarketplaceInventoryFeed> GetProductInventoryFeed(MarketplaceFeed feed);

        /// <summary>
        /// Get the product inventory to update its quantity to the marketplace
        /// </summary>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <returns></returns>
        MarketplaceInventoryFeed GetProductInventoryBySku(string eisSku);

        /// <summary>
        /// Get the list of marketplace items have update pricing
        /// </summary>
        /// <param name="feed">Contains the parameter for retrieving the products to post</param>
        /// <returns></returns>
        IEnumerable<MarketplacePriceFeedDto> GetProductPriceFeed(MarketplaceFeed feed);

        /// <summary>
        /// Get the list of marketplace end item feed for product end listing
        /// </summary>
        /// <param name="feed">Contains the parameter for retrieving the products to post</param>
        /// <returns></returns>
        IEnumerable<ItemFeed> GeteBayItemFeeds(MarketplaceFeed feed);

        /// <summary>
        /// Get the product price information with the specified SKU
        /// </summary>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <returns></returns>
        MarketplacePriceFeedDto GetProductPriceFeedBySku(string eisSku);

        /// <summary>
        /// Get the EIS ProductDto with the specified EIS SKU code
        /// </summary>
        /// <param name="eisSku">The EIS SKU code</param>
        /// <returns></returns>
        ProductDto GetProductByEisSKU(string eisSku);

        /// <summary>
        /// Get the product data for marketplace post feed
        /// </summary>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <returns></returns>
        MarketplaceProductFeedDto GetProductPostFeedByEisSku(string eisSku);

        /// <summary>
        ///  Get the list of products' ASINs
        /// </summary>
        /// <param name="feed">The user's feed cretiria</param>
        /// <returns></returns>
        List<AmazonInfoFeed> GetProductInfoFeeds(MarketplaceFeed feed);

        /// <summary>
        /// Get the list of product keywords to get eBay suggested categories
        /// </summary>
        /// <param name="feed">The user's feed cretiria</param>
        /// <returns></returns>
        IEnumerable<eBayCategoryFeed> GeteBaySuggestedCategoryFeed(MarketplaceFeed feed);
        
        /// <summary>
        /// Save the eisProduct in the database
        /// </summary>
        /// <param name="viewModel">The eisProduct to save</param>
        /// <returns></returns>
        ProductDto SaveProduct(ProductDto viewModel);

        /// <summary>
        /// Update the eisProduct with the specified EIS SKU code and the modified model
        /// </summary>
        /// <param name="eisSku">The EIS SKU code</param>
        /// <param name="viewModel">The updated eisProduct</param>
        /// <returns></returns>
        ProductDto UpdateProduct(string eisSku, ProductDto viewModel);

        /// <summary>
        /// Delete the prouduct with the specified EIS SKU code
        /// </summary>
        /// <param name="eisSku">The EIS SKU code</param>
        /// <returns></returns>
        bool DeleteProduct(string eisSku);

        /// <summary>
        /// Delete the products with the specified list of its EIS SKUs
        /// </summary>
        /// <param name="feed">Contains the product parameters to delete</param>
        /// <returns></returns>
        List<string> GetProductsEisSku(MarketplaceFeed feed);
        
        /// <summary>
        /// Check if the EIS SKU code already exist with the specified vendor id
        /// </summary>
        /// <param name="eisSku">The EIS SKU code to check</param>
        /// <param name="vendorId">The vendor id</param>
        /// <returns></returns>
        bool IsEisSKUExists(string eisSku);

        /// <summary>
        /// Get the maximum value of EisSKU with the specified company id
        /// </summary>
        /// <param name="companyId">The id of company</param>
        /// <returns></returns>
        string GetMaxEisSKUByCompany(int companyId);

        /// <summary>
        /// Get the list of eisProduct's images with the specified EIS SKU code
        /// </summary>
        /// <param name="eisSku">The eisProduct EIS SKU code</param>
        /// <returns></returns>
        IEnumerable<MediaContent> GetProductImages(string eisSku);

        /// <summary>
        /// Get the product image with the specified product image id
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <returns></returns>
        MediaContent GetProductImage(long id);

        /// <summary>
        /// Delete the product image with the specified id
        /// </summary>
        /// <param name="id">The id of the product image</param>
        /// <param name="eisSKU">The EIS SKU</param>
        /// <returns></returns>
        bool DeleteProductImage(long id);

        /// <summary>
        /// Update the product images and delete the old Amazon images
        /// </summary>
        /// <param name="imageUrls">The list of image URLs</param>
        /// <param name="eisSku">The EIS SKU code of the product</param>
        void UpdateProductImages(List<MediaContent> imageUrls, string eisSku);

        /// <summary>
        /// Save the image to the database
        /// </summary>
        /// <param name="image"></param>
        void AddProductImage(MediaContent image);

        /// <summary>
        /// Update the image details
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <param name="fileName">The filename of the image</param>
        /// <param name="caption">The caption for the image</param>
        void UpdateProductImage(long id, string fileName, string caption);

        /// <summary>
        /// Get the eisProduct details from the Amazon with the specified EIS SKU
        /// </summary>
        /// <param name="eisSku">The EIS SKU</param>
        /// <returns></returns>
        ProductAmazonDto GetProductAmazon(string eisSku);

        /// <summary>
        /// Save the Amazon eisProduct in the database
        /// </summary>
        /// <param name="viewModel">The eisProduct to save</param>
        /// <returns></returns>
        ProductAmazonDto SaveProductAmazon(ProductAmazonDto model);

        /// <summary>
        /// Update the Amazon eisProduct with the specified EIS SKU code and the modified model
        /// </summary>
        /// <param name="eisSku">The EIS SKU code</param>
        /// <param name="model">The updated eisProduct</param>
        /// <returns></returns>
        ProductAmazonDto UpdateProductAmazon(string eisSku, ProductAmazonDto model);

        /// <summary>
        /// Get the marketplace eisProduct info with the specified marketplace name
        /// </summary>
        /// <param name="marketplace">The name of th marketplace</param>
        /// <param name="eisSku">The SKU of EIS</param>
        /// <returns></returns>
        MarketplaceProduct GetMarketplaceProductInfo(string marketplace, string eisSku);

        /// <summary>
        /// Get the eisProduct item which matches to the specified EIS sku
        /// </summary>
        /// <param name="eisSku">The part of the EIS SKU to find</param>
        /// <returns></returns>
        Models.Item GetProductItemByEisSku(string eisSku);

        /// <summary>
        /// Get the list of product info feed for Amazon Get Info
        /// </summary>
        /// <param name="eisSkus">The list of EIS SKUs</param>
        /// <returns></returns>
        IEnumerable<AmazonInfoFeed> GetProductInfoFeeds(List<string> eisSkus);

        /// <summary>
        /// Get the product information for eBay with the specified product EIS SKU
        /// </summary>
        /// <param name="eisSku">THE EIS SKU of the product</param>
        /// <returns></returns>
        ProducteBayDto GetProducteBay(string eisSku);

        /// <summary>
        /// Save the eBay product information to the database
        /// </summary>
        /// <param name="model">The eBay product info</param>
        /// <returns></returns>
        bool SaveProducteBay(ProducteBayDto model);

        /// <summary>
        /// Update the eBay product info witht the specified EIS SKU
        /// </summary>
        /// <param name="eisSku">The EIS of the eBay to update</param>
        /// <param name="model">The updated model</param>
        /// <returns></returns>
        bool UpdateProducteBay(string eisSku, ProducteBayDto model);

        /// <summary>
        /// Get the product information for Big Commerce with the specified product EIS SKU
        /// </summary>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <returns></returns>
        ProductBigCommerceDto GetProductBigCommerce(string eisSku);

        /// <summary>
        /// Save the Big Commerce product information to the database
        /// </summary>
        /// <param name="model">The Big Commerce product info</param>
        /// <returns></returns>
        bool SaveProductBigCommerce(ProductBigCommerceDto model);

        /// <summary>
        /// Update the Big Commerce product info witht the specified EIS SKU
        /// </summary>
        /// <param name="eisSku">The EIS of the Big Commerce to update</param>
        /// <param name="model">The updated model</param>
        /// <returns></returns>
        bool UpdateProductBigCommerce(string eisSku, ProductBigCommerceDto model);

        /// <summary>
        /// Update the Big Commerce Custom Fields
        /// </summary>
        /// <param name="customFields">List of Big Commerce Custom Fields</param>
        /// <returns></returns>
        bool CreateOrUpdateBigCommerceCustomFields(List<BigCommerceCustomFieldDto> customFields);
    }
}
