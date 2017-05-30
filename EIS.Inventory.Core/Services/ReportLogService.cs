using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using System.Data;
using MySql.Data.MySqlClient;
using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class ReportLogService : IReportLogService
    {
        private readonly EisInventoryContext _context;
        private readonly string _connectionString;

        public ReportLogService(string connectionString)
        {
            _context = new EisInventoryContext();
            _connectionString = connectionString;
        }

        public IEnumerable<RequestReportViewModel> GetAllRequestReports()
        {
            var requestReports = _context.requestreports.OrderByDescending(x => x.Id).AsQueryable();

            return Mapper.Map<IEnumerable<requestreport>, IEnumerable<RequestReportViewModel>>(requestReports);
        }

        public RequestReportViewModel GetRequestReport(string requestReportId)
        {
            var requestReport = _context.requestreports.FirstOrDefault(x => x.ReportRequestId == requestReportId);

            return Mapper.Map<requestreport, RequestReportViewModel>(requestReport);
        }

        public MarketplaceProcessingReport GetProcessingReport(string requestReportId)
        {
            var processingReport = _context.processingreports.FirstOrDefault(x => x.TransactionId == requestReportId);

            return Mapper.Map<processingreport, MarketplaceProcessingReport>(processingReport);
        }

        public IEnumerable<MarketplaceProcessingReportResult> GetProcessingReportErrors(string requestReportId)
        {
            var errorResults = _context.processingreportresults
                .Where(x => x.TransactionId == requestReportId && x.Code == "Error");

            return Mapper.Map<IEnumerable<processingreportresult>, IEnumerable<MarketplaceProcessingReportResult>>(errorResults);
        }

        public IEnumerable<MarketplaceProcessingReportResult> GetProcessingReportWarnings(string requestReportId)
        {
            var warningResults = _context.processingreportresults
                .Where(x => x.TransactionId == requestReportId && x.Code == "Warning");

            return Mapper.Map<IEnumerable<processingreportresult>, IEnumerable<MarketplaceProcessingReportResult>>(warningResults);
        }

        public IEnumerable<LogViewModel> GetAllLogs()
        {
            var logs = _context.logs.OrderByDescending(x => x.Id).AsQueryable();

            return Mapper.Map<IEnumerable<log>, IEnumerable<LogViewModel>>(logs);
        }

        public LogViewModel GetLog(int id)
        {
            var log = _context.logs.FirstOrDefault(x => x.Id == id);

            return Mapper.Map<log, LogViewModel>(log);
        }

        public IEnumerable<LogViewModel> GetFileUploaderLogs()
        {
            var results = new List<LogViewModel>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT Created,Severity,EntryType,Description,StackTrace,Id FROM logs ORDER BY Id DESC", null);

                while (reader.Read())
                {
                    var model = new LogViewModel();
                    model.Created = reader[0] == DBNull.Value ? default(DateTime) : (DateTime)reader[0];
                    model.Severity = (LogEntrySeverity)reader[1];
                    model.EntryTypeStr = reader[2].ToString();
                    model.Description = reader[3].ToString();
                    model.StackTrace = reader[4].ToString();
                    model.Id = (int)reader[5];

                    results.Add(model);
                }
            }

            return results;
        }

        public LogViewModel GetFileUploaderLog(int id)
        {
            var model = new LogViewModel();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameter = new Dictionary<string, object>
                {
                    {"@Id", id}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT Created,Severity,EntryType,Description,StackTrace,Id FROM logs WHERE Id=@Id",
                       parameter);

                while (reader.Read())
                {
                    model.Created = reader[0] == DBNull.Value ? default(DateTime) : (DateTime)reader[0];
                    model.Severity = (LogEntrySeverity)reader[1];
                    model.EntryTypeStr = reader[2].ToString();
                    model.Description = reader[3].ToString();
                    model.StackTrace = reader[4].ToString();
                    model.Id = (int)reader[5];
                }
            }

            return model;
        }
    }
}
