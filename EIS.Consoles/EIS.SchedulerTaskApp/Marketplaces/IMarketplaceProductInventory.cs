using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;

namespace EIS.SchedulerTaskApp.Marketplaces
{
    public interface IMarketplaceProductInventory
    {
        /// <summary>
        /// Get the marketplace or the channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Set the credential details for this marketplace
        /// </summary>
        CredentialDto Credential { set; }

        /// <summary>
        /// Submit the inventory feed for the products to the marketplace
        /// </summary>
        /// <param name="inventoryFeeds"></param>
        void SubmitProductsInventoryFeed(List<MarketplaceInventoryFeed> inventoryFeeds);
    }
}
