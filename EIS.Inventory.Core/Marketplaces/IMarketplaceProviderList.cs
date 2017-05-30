using System.Collections.Generic;

namespace StockManagement.Core.Marketplaces
{
    public interface IMarketplaceProviderList
    {
        /// <summary>
        /// Get the list of marketplace channel provider
        /// </summary>
        /// <returns></returns>
        List<IMarketplaceChannel> GetMarketplaceChannels();

        /// <summary>
        /// Get the list of marketplace channel names
        /// </summary>
        /// <returns></returns>
        List<string> GetMarketplaceChannelNames();
    }
}
