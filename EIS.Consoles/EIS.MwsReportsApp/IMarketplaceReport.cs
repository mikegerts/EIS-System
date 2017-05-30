using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp
{
    public interface IMarketplaceReport
    {
        /// <summary>
        /// Get the marketplace name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Get the settlement report data from the marketplace
        /// </summary>
        /// <param name="createdAfter"></param>
        /// <returns></returns>
        SettlementReportDto GetSettlementReport(DateTime createdAfter, bool includeAcknowledged);
    }
}
