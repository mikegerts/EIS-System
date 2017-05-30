using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public interface IReportLogService
    {
        /// <summary>
        /// Get all the list of request reports
        /// </summary>
        /// <returns></returns>
        IEnumerable<RequestReportViewModel> GetAllRequestReports();

        /// <summary>
        /// Get the request report with the specified request report id
        /// </summary>
        /// <param name="requestReportId">The id of the request report</param>
        /// <returns></returns>
        RequestReportViewModel GetRequestReport(string requestReportId);

        /// <summary>
        /// Get the processing report of the specified request report
        /// </summary>
        /// <param name="requestReportId">The id of the request report</param>
        /// <returns></returns>
        MarketplaceProcessingReport GetProcessingReport(string requestReportId);

        /// <summary>
        /// Get the list of processing report errors
        /// </summary>
        /// <param name="requestReportId">The id of the report transaction</param>
        /// <returns></returns>
        IEnumerable<MarketplaceProcessingReportResult> GetProcessingReportErrors(string requestReportId);

        /// <summary>
        /// Get the list of processing report warningss
        /// </summary>
        /// <param name="requestReportId">The id of the report transaction</param>
        /// <returns></returns>
        IEnumerable<MarketplaceProcessingReportResult> GetProcessingReportWarnings(string requestReportId);

        /// <summary>
        /// Get all the list of main logs
        /// </summary>
        /// <returns></returns>
        IEnumerable<LogViewModel> GetAllLogs();

        /// <summary>
        /// Get the log object with the specified id
        /// </summary>
        /// <param name="id">The id of the log</param>
        /// <returns></returns>
        LogViewModel GetLog(int id);

        /// <summary>
        /// Get the list of file uploader logs
        /// </summary>
        /// <returns></returns>
        IEnumerable<LogViewModel> GetFileUploaderLogs();

        /// <summary>
        /// Get the file uploader log object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LogViewModel GetFileUploaderLog(int id);
    }
}
