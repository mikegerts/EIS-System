using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace StockManagement.Core.Marketplaces
{
    public class MarketplaceProviderList : IMarketplaceProviderList
    {
        public MarketplaceProviderList()
        {
            CoreMef.Container.ComposeParts(this);
        }
        
        [ImportMany(typeof(IMarketplaceChannel))]
        protected List<IMarketplaceChannel> marketplaceChannels { get; set; }
        
        public List<IMarketplaceChannel> GetMarketplaceChannels()
        {
            return marketplaceChannels;
        }

        public List<string> GetMarketplaceChannelNames()
        {
            return marketplaceChannels
                .Select(x => x.ChannelName)
                .OrderBy(x => x)
                .ToList();
        }
    }
}
