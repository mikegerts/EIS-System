using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class Logger
    {
        private static string _inventoryConnectionString;        

        static Logger()
        {
            _inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public static void LogInfo(LogEntryType entryType, string description)
        {
            Console.WriteLine("Entry Type:{0} Message: {1}", entryType, description);
            log(LogEntrySeverity.Information, entryType, description, string.Empty);
        }

        public static void LogWarning(LogEntryType entryType, string description)
        {
            Console.WriteLine("Entry Type:{0} Message: {1}", entryType, description);
            log(LogEntrySeverity.Warning, entryType, description, string.Empty);
        }

        public static void LogError(LogEntryType entryType, string description = "", string stackTrace = "")
        {
            Console.WriteLine("Entry Type:{0} \nMessage: {1} Stacktrace: {2}", entryType, description, stackTrace);
            log(LogEntrySeverity.Error, entryType, description, stackTrace);
        }

        private static void log(LogEntrySeverity severity, LogEntryType entryType, string description, string stackTrace)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Severity", severity},
                    {"@EntryType", entryType},
                    {"@Description", description},
                    {"@StackTrace", stackTrace},
                    {"@Created", DateTime.UtcNow},
                };

                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO logs(Severity,EntryType,Description,StackTrace,Created)
                VALUES(@Severity,@EntryType,@Description,@StackTrace,@Created)", parameters);
            }
        }

        public static void AddProcessingReport(MarketplaceProcessingReport report)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@TransactionId", report.TransactionId },
                    {"@MessageType", report.MessageType},
                    {"@StatusCode", report.StatusCode},
                    {"@MessagesProcessed", report.MessagesProcessed},
                    {"@MessagesSuccessful", report.MessagesSuccessful},
                    {"@MessagesWithError", report.MessagesWithError},
                    {"@MessagesWithWarning", report.MessagesWithWarning }, 
                    {"@MerchantId", report.MerchantId}, 
                    {"@SubmittedBy", report.SubmittedBy},  
                    {"@Created", DateTime.UtcNow }
                };
                
                // insert first the report
                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO processingreports(TransactionId,MessageType,StatusCode,
                        MessagesProcessed,MessagesSuccessful,MessagesWithError,MessagesWithWarning,
                        MerchantId,SubmittedBy,Created) 
                        VALUES(@TransactionId,@MessageType,@StatusCode,@MessagesProcessed,@MessagesSuccessful,
                        @MessagesWithError,@MessagesWithWarning,@MerchantId,@SubmittedBy,@Created)", parameters);
                
                // then its details
                foreach (var item in report.ReportResults)
                {
                    var param = new Dictionary<string, object>
                    {
                        {"@TransactionId", item.TransactionId },
                        {"@MessageId", item.MessageId},
                        {"@Code", item.Code},
                        {"@MessageCode", item.MessageCode },
                        {"@AdditionalInfo", item.AdditionalInfo},
                        {"@Description", item.Description},
                    };
                    
                    // then its report details
                    MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO processingreportresults(TransactionId,MessageId,Code,
                        MessageCode,AdditionalInfo,Description) 
                        VALUES(@TransactionId,@MessageId,@Code,@MessageCode,@AdditionalInfo,@Description)", param);
                }
            }
        }

        public static void AddRequestReport(MarketplaceRequestReport report)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@RequestId", report.RequestId },
                    {"@ReportRequestId", report.ReportRequestId},
                    {"@FeedType", report.FeedType},
                    {"@StartDate", report.StartDate},
                    {"@EndDate", report.EndDate},
                    {"@ProcessingStatus", report.ProcessingStatus},
                    {"@IsScheduled", report.IsScheduled }, 
                    {"@SubmittedDate", report.SubmittedDate}, 
                    {"@SubmittedBy", report.SubmittedBy},  
                };

                // insert first the report
                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO requestreports(RequestId,ReportRequestId,FeedType,
                        StartDate,EndDate,ProcessingStatus,IsScheduled,SubmittedDate,SubmittedBy) 
                        VALUES(@RequestId,@ReportRequestId,@FeedType,@StartDate,@EndDate,
                        @ProcessingStatus,@IsScheduled,@SubmittedDate,@SubmittedBy)", parameters);                                
            }
        }

        public static void UpdateRequestReport(MarketplaceRequestReport report)
        {
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@RequestId", report.RequestId },
                    {"@ReportRequestId", report.ReportRequestId},
                    {"@FeedType", report.FeedType},
                    {"@StartDate", report.StartDate},
                    {"@EndDate", report.EndDate},
                    {"@ProcessingStatus", report.ProcessingStatus},
                    {"@IsScheduled", report.IsScheduled }, 
                    {"@SubmittedDate", report.SubmittedDate}, 
                    {"@SubmittedBy", report.SubmittedBy},  
                };

                // insert first the report
                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE requestreports
                    SET FeedType=@FeedType, StartDate=@StartDate, EndDate=@EndDate, ProcessingStatus=@ProcessingStatus,
                     IsScheduled=@IsScheduled, SubmittedDate=@SubmittedDate, SubmittedBy=@SubmittedBy
                    WHERE ReportRequestId = @ReportRequestId", parameters);
            }
        }
    }
}
