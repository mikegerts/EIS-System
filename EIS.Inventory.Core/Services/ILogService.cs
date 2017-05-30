using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using System;
using System.Collections.Generic;

namespace EIS.Inventory.Core.Services
{
    public interface ILogService : IDisposable
    {
        /// <summary>
        /// Log warning message with the specified description and entry type
        /// </summary>
        /// <param name="entryType">The entry type where the messsage from</param>
        /// <param name="description">The description of the message</param>
        void LogWarning(LogEntryType entryType, string description);

        /// <summary>
        /// Log error message with the specified description and entry type
        /// </summary>
        /// <param name="entryType">The entry type where the messsage from</param>
        /// <param name="description">The description of the message</param>
        /// <param name="stackTrace">The stacktrace of error logs</param>
        void LogError(LogEntryType entryType, string description, string stackTrace);

        /// <summary>
        /// Log info message with the specified description and entry type
        /// </summary>
        /// <param name="entryType">The entry type where the messsage from</param>
        /// <param name="description">The description of the message</param>
        void LogInfo(LogEntryType entryType, string description);

        /// <summary>
        /// Add new entry to the database with severity, type, description and the stacktrace of the error
        /// </summary>
        /// <param name="severity">The type of severity</param>
        /// <param name="entryType">The type of entry</param>
        /// <param name="description">The message to log</param>
        /// <param name="stackTrace">The stacktrace of the error</param>
        void Add(LogEntrySeverity severity, LogEntryType entryType, string description, string stackTrace = "");

        /// <summary>
        /// Log the request report object to the database
        /// </summary>
        /// <param name="requestReportModel">The request report to persist to database</param>
        void AddRequestReport(MarketplaceRequestReport requestReportModel);

        /// <summary>
        /// Update the request report object from the database
        /// </summary>
        /// <param name="requestReportModel">Contains the updated request report</param>
        void UpdateRequestReport(MarketplaceRequestReport requestReportModel);

        /// <summary>
        /// Log the processing report object to thedatabase
        /// </summary>
        /// <param name="processingReportModel">The processing report to persist to database</param>
        void AddProcessingReport(MarketplaceProcessingReport processingReportModel);

        /// <summary>
        /// Insert the item to the database
        /// </summary>
        /// <param name="itemContainer">The fee container for the item</param>
        /// <param name="isUpdateItemId"></param>
        void AddProductItemFees(ItemContainer itemContainer, bool isUpdateItemId = true);

        /// <summary>
        /// Set the product ebay itemid to null
        /// </summary>
        /// <param name="endItems"></param>
        void SeteBayItemIdToNull(List<ItemFeed> endItems);
    }
}
