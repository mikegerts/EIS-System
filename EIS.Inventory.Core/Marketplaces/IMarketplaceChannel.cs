using System.Collections.Generic;
using StockManagement.Core.ViewModels;

namespace StockManagement.Core.Marketplaces
{
    public interface IMarketplaceChannel
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Gets or sets the MWS credential to the API
        /// </summary>
        IMarketplaceCredential MwsCredential { set; }

        /// <summary>
        /// Submit the list proudct feeds to the marketplace
        /// </summary>
        /// <param name="categorizedProducts"></param>
        void SubmitProductFeed(List<CategorizedProductModel> categorizedProducts);

        /// <summary>
        /// Submit the list of categorized products to update its pricing
        /// </summary>
        /// <param name="categorizedProducts"></param>
        void SubmitPriceFeed(List<CategorizedProductModel> categorizedProducts);

        /// <summary>
        /// Submit the list of categorized products to upddate the product's quantity
        /// </summary>
        /// <param name="categorizedProducts"></param>
        void SubmitInventoryFeed(List<CategorizedProductModel> categorizedProducts);
    }
}
