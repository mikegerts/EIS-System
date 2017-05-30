using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.MwsChannels
{
    public interface IMarketplaceProductProvider
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Gets or sets the MWS credential to the API
        /// </summary>
        CredentialDto MarketplaceCredential { set; }

        /// <summary>
        /// Get the Marketplace eisProduct info with the specified id - an ASIN or EIS SKU
        /// </summary>
        /// <param name="infoFeed">An ASIN or EIS SKU of the product</param>
        /// <returns></returns>
        MarketplaceProduct GetProductInfo(AmazonInfoFeed infoFeed);

        /// <summary>
        /// Get the list of suggested categories with the specified keyword
        /// </summary>
        /// <param name="keyword">The keyword to search</param>
        /// <returns></returns>

        List<MarketplaceCategoryDto> GetSuggestedCategories(string keyword);
    }
}
