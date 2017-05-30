using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Managers
{
    public interface IMarketplaceProductManager
    {
        /// <summary>
        /// Get the Marketplace eisProduct info with the specified id
        /// </summary>
        /// <param name="marketplaceType">The type of the marketplace</param>
        /// <param name="eisSku">The ASIN of the EIS product</param>
        /// <returns></returns>
        MarketplaceProduct GetMarketplaceProductInfo(string marketplaceType, string eisSku);

        /// <summary>
        /// Get the list of marketplace categories
        /// </summary>
        /// <param name="marketplaceType">The channel name</param>
        /// <param name="keyword">The keyword to search</param>
        /// <returns></returns>
        List<MarketplaceCategoryDto> GetSuggestedCategories(string marketplaceType, string keyword);
    }
}
