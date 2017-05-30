using System;
using System.Collections.Generic;
using System.Configuration;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Repositories
{
    public class LoggerRepository
    {
        private string _connectionString;

        public LoggerRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public  void LogInfo(LogEntryType entryType, string description)
        {
            Console.WriteLine("Entry Type:{0} Message: {1}", entryType, description);
            log(LogEntrySeverity.Information, entryType, description, string.Empty);
        }

        public void LogWarning(LogEntryType entryType, string description)
        {
            Console.WriteLine("Entry Type:{0} Message: {1}", entryType, description);
            log(LogEntrySeverity.Warning, entryType, description, string.Empty);
        }

        public  void LogError(LogEntryType entryType, string description = "", string stackTrace = "")
        {
            Console.WriteLine("Entry Type:{0} \nMessage: {1} Stacktrace: {2}", entryType, description, stackTrace);
            log(LogEntrySeverity.Error, entryType, description, stackTrace);
        }

        private void log(LogEntrySeverity severity, LogEntryType entryType, string description, string stackTrace)
        {
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
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
    }
}
