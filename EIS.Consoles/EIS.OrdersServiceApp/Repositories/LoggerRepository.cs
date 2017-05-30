using System;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;

namespace EIS.OrdersServiceApp.Repositories
{
    public class LoggerRepository
    {
        public void UpdateRequestReport(MarketplaceRequestReport requestReport)
        {
            using (var context = new EisInventoryContext())
            {
                // find the existing report request by id
                var oldRequestReport = context.requestreports
                        .FirstOrDefault(x => x.ReportRequestId == requestReport.ReportRequestId);

                context.Entry(oldRequestReport).CurrentValues.SetValues(requestReport);
                context.SaveChanges();
            }   
        }

        public void AddRequestReport(MarketplaceRequestReport model)
        {
            using (var context = new EisInventoryContext())
            {
                var report = new requestreport();
                CopyObject.CopyFields(model, report);

                context.requestreports.Add(report);
                context.SaveChanges();
            } 
        }

        public void AddProcessingReport(MarketplaceProcessingReport model)
        {            
            using (var context = new EisInventoryContext())
            {
                var report = new processingreport();
                CopyObject.CopyFields(model, report);

                context.processingreports.Add(report);

                foreach (var resultModel in model.ReportResults)
                {
                    var reportResult = new processingreportresult();
                    CopyObject.CopyFields(resultModel, reportResult);

                    context.processingreportresults.Add(reportResult);
                }

                context.SaveChanges();
            }
        }
        
        public void LogInfo(LogEntryType entryType, string description)
        {
            log(LogEntrySeverity.Information, entryType, description, string.Empty);
        }

        public void LogWarning(LogEntryType entryType, string description)
        {
            log(LogEntrySeverity.Warning, entryType, description, string.Empty);
        }

        public void LogError(LogEntryType entryType, string description = "", string stackTrace = "")
        {
            log(LogEntrySeverity.Error, entryType, description, stackTrace);
        }

        private void log(LogEntrySeverity severity, LogEntryType entryType, string description, string stackTrace)
        {
            using (var context = new EisInventoryContext())
            {
                context.logs.Add(new log
                {
                    Created = DateTime.Now,
                    Severity = (int)severity,
                    EntryType = (int)entryType,
                    Description = description,
                    StackTrace = stackTrace
                });

                context.SaveChanges();
            }    
        }
    }
}
