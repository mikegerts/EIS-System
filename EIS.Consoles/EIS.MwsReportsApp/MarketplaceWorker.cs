using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.Loggers;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp
{
    public class MarketplaceWorker
    {
        private IMarketplaceReport _reportManager;

        public MarketplaceWorker(IMarketplaceReport reportManager)
        {
            _reportManager = reportManager;
        }

        public SettlementReportDto GetSettlementReport(DateTime createdAfter, bool includeAcknowledged)
        {
            try
            {
                return _reportManager.GetSettlementReport(createdAfter, includeAcknowledged);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error Message: " + ex.Message + "\n" + ex.StackTrace);
                Logger.LogError("Fetching Orders", string.Format("Error occured in managing settlement report for {0} <br/>Error message: {1}",
                    _reportManager.ChannelName, ex.InnerException == null ? ex.Message : string.Format("{0} Inner message: {1}", ex.Message, ex.InnerException)),
                    ex.StackTrace);
                return null;
            }
        }
    }
}
