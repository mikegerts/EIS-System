using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace EIS.DataUploaderApp.Repositories
{
    public class Logger
    {
        private static string _connectionString;

        static Logger()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["VendorsConfigConnection"].ConnectionString;
        }

        public static void LogInfo(string entryType, string description = "", string stackTrace = "")
        {
            log(1, entryType, description, stackTrace);
        }

        public static void LogWarn(string entryType, string description = "", string stackTrace = "")
        {
            log(2, entryType, description, stackTrace);
        }

        public static void LogError(string entryType, string description = "", string stackTrace = "")
        {
            log(3, entryType, description, stackTrace);
        }

        private static void log(int severity, string entryType, string description, string stackTrace)
        {
            Console.Error.WriteLineAsync(description);
            Console.Error.WriteLineAsync(stackTrace);
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Severity", severity},
                    {"@EntryType", entryType},
                    {"@Description", description},
                    {"@StackTrace", stackTrace},
                    {"@Created", DateTime.Now},
                };

                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO logs(Severity,EntryType,Description,StackTrace,Created)
                VALUES(@Severity,@EntryType,@Description,@StackTrace,@Created)", parameters);
            }
        }
    }
}
