using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.MwsChannels
{
    /// <summary>
    /// An interface contains the reporting objects for marketplace(s)
    /// </summary>
    public interface IMarketplaceReportProvider
    {
        /// <summary>
        /// Get the marketplace channel name
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Gets or sets the MWS credential to the API
        /// </summary>
        CredentialDto MarketplaceCredential { set; }

        IEnumerable<object> GetReportList(DateTime startDate, DateTime endDate);

        string DoRequestReport(string reportType, DateTime startDate);

        void DoRequestReportList(List<string> requestReportIds);
    }
}
