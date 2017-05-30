using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.MwsChannels
{
    public interface IMarketplaceInventoryProvider
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Gets or sets the MWS credential to the API
        /// </summary>
        CredentialDto Credential { set; }

        /// <summary>
        /// Submit the list proudct feeds to the marketplace
        /// </summary>
        /// <param name="productFeeds">The list of product for product listing in marketplaces</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitProductsListingFeed(List<MarketplaceProductFeedDto> productFeeds, string submittedBy);

        /// <summary>
        /// Submit the feed for the update of eisProduct's info
        /// </summary>
        /// <param name="productFeed">The updated eisProduct's details</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitSingleProductListingFeed(MarketplaceProductFeedDto productFeed, string submittedBy);

        /// <summary>
        /// Submit the list of products to marketplace to updates its details
        /// </summary>
        /// <param name="productFeeds">The products to update</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitProductsReviseFeed(List<MarketplaceProductFeedDto> productFeeds, string submittedBy);

        /// <summary>
        /// Submit a single product feed to marketplace to update its details
        /// </summary>
        /// <param name="productFeed">The product info to submit</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitSingleProductReviseFeed(MarketplaceProductFeedDto productFeed, string submittedBy);

        /// <summary>
        /// Submit the price feed to the marketplaces with the price items
        /// </summary>
        /// <param name="priceFeeds">Collection of objects that represent the item and its prices should be posted to marketplace(s)</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitProductPriceFeeds(List<MarketplacePriceFeedDto> priceFeeds, string submittedBy);

        /// <summary>
        /// Submit the price feed to the marketplace with the specified item
        /// </summary>
        /// <param name="priceFeed">The item which contains the product information to submit</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitSingleProductPriceFeed(MarketplacePriceFeedDto priceFeed, string submittedBy);

        /// <summary>
        /// Updates the marketplace products inventory so that items will have the specified quantity available
        /// </summary>
        /// <param name="inventoryFeeds">Collection of objects that represent the item and quantity the inventory should be updated to.</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitProductInventoryFeeds(List<MarketplaceInventoryFeed> inventoryFeeds, string submittedBy);

        /// <summary>
        /// Submit the inventory feed to the marketplace
        /// </summary>
        /// <param name="inventoryFeed">The product item contains the information to submit</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitSingleProductInventoryFeed(MarketplaceInventoryFeed inventoryFeed, string submittedBy);

        /// <summary>
        /// Submit the product end item feed to delete it from the marketplace
        /// </summary>
        /// <param name="itemFeeds">The list of item to end the listing</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitProductEndItemFeeds(List<ItemFeed> itemFeeds, string submittedBy);

        /// <summary>
        /// End the item in marketplace product listing or availability
        /// </summary>
        /// <param name="itemFeed">The item to end the listing</param>
        /// <param name="submittedBy">The name of the user who does the posting feed</param>
        void SubmitSingleProductEndItem(ItemFeed itemFeed, string submittedBy);
    }
}
