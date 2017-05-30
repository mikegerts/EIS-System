using EIS.Inventory.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.MwsReportsApp.Marketplaces
{
    [Export(typeof(IMarketplaceReport))]
    public class eBayMarketplaceReport : IMarketplaceReport
    {
        public string ChannelName
        {
            get { return "eBay"; }
        }

        public SettlementReportDto GetSettlementReport(DateTime createdAfter, bool includeAcknowledged)
        {
            return null;
        }
    }
}
