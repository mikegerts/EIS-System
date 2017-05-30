using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum JobType
    {
        /// <summary>
        /// Upload or update product and/or Amazon details
        /// </summary>
        [Description("Product File Upload")]
        ProductFileUpload = 0,

        /// <summary>
        /// Get product information from the Amazon
        /// </summary>
        [Description("Amazon Get Info")]
        AmazonGetInfo = 1,

        /// <summary>
        /// Delete bulk EIS products
        /// </summary>
        [Description("Bulk Delete Products")]
        BulkDeleteProduct = 2,

        /// <summary>
        /// Upload or update kit details
        /// </summary>
        [Description("Kit File Upload")]
        KitFileUpload = 3,

        /// <summary>
        /// Upload or upate shadow products
        /// </summary>
        [Description("Shadow File Upload")]
        ShadowFileUpload = 4,

        /// <summary>
        /// Upload or upate blacklisted skus
        /// </summary>
        [Description("Blacklisted SKU File Upload")]
        BlacklistedSkuUpload = 5,

        /// <summary>
        /// Get bulk eBay suggested categories
        /// </summary>
        [Description("eBay Bulk Suggested Categories")]
        BulkeBaySuggestedCategories = 6,

        /// <summary>
        /// Upload vendor product file
        /// </summary>
        [Description("Vendor Product File Upload")]
        VendorProductFileUpload = 7,

        /// <summary>
        /// Delete bulk vendor products
        /// </summary>
        [Description("Bulk Delete Vendor Products")]
        BulkDeleteVendorProduct = 8,

        /// <summary>
        /// Upload shipping rate file
        /// </summary>
        [Description("Shipping Rate File Upload")]
        ShippingRateFileUpload = 9,

        /// <summary>
        /// Upload shipping rate file
        /// </summary>
        [Description("Vendor Inventory File Upload")]
        VendorInventoryFileUpload = 10,

        /// <summary>
        /// Relist product items into eBay
        /// </summary>
        [Description("eBay Products ReListing")]
        eBayProductsReListing = 11,

        /// <summary>
        /// End item products from eBay
        /// </summary>
        [Description("eBay Products EndItem")]
        eBayProductsEndItem = 12,
    }
}
