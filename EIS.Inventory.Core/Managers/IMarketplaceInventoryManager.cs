using EIS.Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.Managers
{
    public interface IMarketplaceInventoryManager
    {
        /// <summary>
        /// Submit the eisProduct feeds by categories to add or update products information
        /// </summary>
        /// <param name="marketplaceFeed">The object contains the information for posting price feed to marketplace/s</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitProductsListingFeedAsync(MarketplaceFeed marketplaceFeed, string userName);
       
        /// <summary>
        /// Submit single eisProduct feed to update eisProduct's information
        /// </summary>
        /// <param name="marketplaceType">The name of marketplace where to post the update</param>
        /// <param name="eisSku">The EIS SKU code of the eisProduct</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitSingleProductListingFeed(string marketplaceType, string eisSku, string userName);

        /// <summary>
        /// Submit product revise feed to marketplaces
        /// </summary>
        /// <param name="marketplaceFeed">The object contains the information for posting price feed to marketplace/s</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitProductsReviseFeedAsync(MarketplaceFeed marketplaceFeed, string userName);

        /// <summary>
        /// Submit single eisProduct feed to modify product details
        /// </summary>
        /// <param name="marketplaceType">The name of the marketplace</param>
        /// <param name="eisSku">The EIS SKU</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitSingleProductReviseFeed(string marketplaceType, string eisSku, string userName);

        /// <summary>
        /// Submit inventory feed by categories to update eisProduct availability or quantity
        /// </summary>
        /// <param name="marketplaceFeed">The object contains the information for posting price feed to marketplace/s</param>
        /// <param name="userName">The doer of this request</param>
        Task SubmitInventoryFeedAysnc(MarketplaceFeed marketplaceFeed, string userName);
        
        /// <summary>
        /// Submit inventory feed by EIS SKU to the specified marketplace
        /// </summary>
        /// <param name="marketplaceType">The marketplace name where to post the feed</param>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitInventoryFeedBySkuAsync(string marketplaceType, string eisSku, string userName);

        /// <summary>
        /// Submit price feed by categories to update the eisProduct's pricing
        /// </summary>
        /// <param name="marketplaceFeed">The object contains the information for posting price feed to marketplace/s</param>
        /// <param name="userName">The doer of this request</param>
        Task SubmitPriceFeedAsync(MarketplaceFeed marketplaceFeed, string userName);

        /// <summary>
        /// Submit the price feed with the specified marketplace and product's SKU
        /// </summary>
        /// <param name="marketplaceType">The marketplace name where to post the feed</param>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitPriceFeedBySkuAsync(string marketplaceType, string eisSku, string userName);

        /// <summary>
        /// Get the list of name for marketplace inventory channels
        /// </summary>
        /// <returns></returns>
        List<string> GetMarketplaceInventoryChannels();

        /// <summary>
        /// Submit the product feed for end listing on eBay only.
        /// </summary>
        /// <param name="marketplaceType">The marketplace name where to post the feed</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmiteBayEndProductListingAsync(MarketplaceFeed marketplaceFeed, string userName);

        /// <summary>
        /// Submit single end item feed for product to marketplace
        /// </summary>
        /// <param name="marketplaceType">The marketplace name where to post the feed</param>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmitEndItemFeedBySkuAsync(string marketplaceType, string eisSku, string userName);

        /// <summary>
        /// Submit single end item feed for product to marketplace eBay
        /// </summary>
        /// <param name="marketplaceType">The marketplace name where to post the feed</param>
        /// <param name="eisSku">The EIS SKU of the product</param>
        /// <param name="userName">The doer of this request</param>
        /// <returns></returns>
        Task SubmiteBayEndItemFeedBySkuAsync(string marketplaceType, string eisSku, string userName);
    }
}
