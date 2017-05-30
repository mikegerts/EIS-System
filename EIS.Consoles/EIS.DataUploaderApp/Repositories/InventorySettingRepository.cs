using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Models;
using MySql.Data.MySqlClient;

namespace EIS.DataUploaderApp.Repositories
{
    public class InventorySettingRepository
    {
        private readonly string _connectionString;

        public InventorySettingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueryable<InventoryFileSetting> GetInventoryFileSettings(DateTime runDateTime)
        {
            var settings = new List<InventoryFileSetting>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>(){
                    {"@NextRunDate", runDateTime.Date}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension
                        ,SKU,Name,Description,ShortDescription,Category,UPCCode,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile,IsRunNow,LastExecutedOn 
                    FROM inventoryfilesettings
                    WHERE IsRunNow = 1 OR CAST(NextRunDate AS Date)<=@NextRunDate", parameters);

                while (reader.Read())
                {
                    var setting = new InventoryFileSetting();
                    setting.VendorId = (int)reader[0];
                    setting.FileName = reader[1].ToString();
                    setting.FilePath = reader[2].ToString();
                    setting.TransferPath = reader[3].ToString();
                    setting.ReadTime = Convert.ToDateTime(reader[4].ToString()); // reader[4] == DBNull.Value ? default(TimeSpan) : (TimeSpan)reader[4];
                    setting.RowAt = (int)reader[5];
                    setting.FileType = (FileType)reader[6];
                    setting.NextRunDate = reader[7] == DBNull.Value ? null : (DateTime?)reader[7];
                    setting.Extension = reader[8].ToString();
                    setting.SKU = (int)reader[9];
                    setting.Name = reader[10] as int?;
                    setting.Description = reader[11] as int?;
                    setting.ShortDescription = reader[12] as int?;
                    setting.Category = reader[13] as int?;
                    setting.UPCCode = reader[14] as int?;
                    setting.Cost = reader[15] as int?;
                    setting.Quantity = reader[16] as int?;
                    setting.Delimiter = reader[17] == DBNull.Value ? ';' : reader[17].ToString()[0];
                    setting.FtpServer = reader[18].ToString();
                    setting.FtpUser = reader[19].ToString();
                    setting.FtpPassword = reader[20].ToString();
                    setting.FtpPort = reader[21] == DBNull.Value ? null : (int?)reader[21];
                    setting.RemoteFolder = reader[22].ToString();
                    setting.IsDeleteFile = Convert.ToBoolean(reader[23]);
                    setting.IsRunNow = Convert.ToBoolean(reader[24]);
                    setting.LastExecutedOn = reader[25] == DBNull.Value ? null : (DateTime?)reader[25];

                    settings.Add(setting);
                }
            }

            return settings.AsQueryable();
        }

        public void SetInventoryNextUploadDate(long vendorId, DateTime nextUploadDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>(){
                    {"@NextRunDate", nextUploadDate.Date},
                    {"@VendorId", vendorId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                   @"UPDATE inventoryfilesettings 
                    SET NextRunDate=@NextRunDate 
                    WHERE VendorId=@VendorId", parameters);
            }
        }
        
        /// <summary>
        /// Update the Last Execution and IsRunNow for the specified vendor ID
        /// </summary>
        /// <param name="vendorId">The id of vendor</param>
        public void UpdateFileSettingLastExecution(long vendorId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>(){
                    {"@LastExecutedOn", DateTime.Now },
                    {"@VendorId", vendorId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                   @"UPDATE inventoryfilesettings 
                    SET LastExecutedOn=@LastExecutedOn,
                        IsRunNow = 0 
                    WHERE VendorId=@VendorId", parameters);
            }
        }

        public UploadStatus GetCurrentUploadStatus(long vendorId, DateTime startUploadDate)
        {
            var status = default(UploadStatus);
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId},
                    {"@StartUploadDate", startUploadDate.Date}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,VendorId,StartUploadDate,EndUploadDate,Status,Attempt FROM inventoryuploadstatus
                WHERE VendorId=@VendorId and CAST(StartUploadDate AS DATE)=@StartUploadDate", parameters);

                while (reader.Read())
                {
                    status = new UploadStatus();
                    status.Id = (long)reader[0];
                    status.VendorId = (int)reader[1];
                    status.StartUploadDate = reader[2] == DBNull.Value ? null : (DateTime?)reader[2];
                    status.EndUploadDate = reader[3] == DBNull.Value ? null : (DateTime?)reader[3];
                    status.StatusType = (StatusType)reader[4];
                    status.Attempt = (int)reader[5];
                }
            }

            return status;
        }

        public void CreateUploadStatus(long vendorId, DateTime resultDate, StatusType statusType)
        {
            // check if it this vendor started its upload for this result date
            var status = GetUploadStatus(vendorId, resultDate);

            using (var conn = new MySqlConnection(_connectionString))
            {
                if (status != null)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        {"@VendorId", vendorId},
                        {"@StartUploadDate", DateTime.Now}, // update the start update datetime
                        {"@EndUploadDate", null},
                        {"@Status", (int)statusType},
                        {"@Attempt", status.Attempt + 1},
                        {"@resultDate", resultDate.Date}
                    };

                    MySqlHelper.ExecuteNonQuery(conn,
                        @"UPDATE inventoryuploadstatus
                    SET StartUploadDate=@StartUploadDate,EndUploadDate=@EndUploadDate,Status=@Status,Attempt=@Attempt
                    WHERE VendorId=@VendorId AND CAST(StartUploadDate AS DATE)=@resultDate", parameters);
                }
                else // create new record
                {
                    var parameters = new Dictionary<string, object>
                    {
                        {"@VendorId", vendorId},
                        {"@StartUploadDate", DateTime.Now}, // update the start update datetime
                        {"@EndUploadDate", null},
                        {"@Status", (int)statusType},
                        {"@Attempt", 1}
                    };

                    MySqlHelper.ExecuteNonQuery(conn,
                        @"INSERT INTO inventoryuploadstatus(VendorId,StartUploadDate,EndUploadDate,Status,Attempt)
                          VALUES(@VendorId,@StartUploadDate,@EndUploadDate,@Status,@Attempt)", parameters);
                }
            }
        }

        public void SetUploadStatus(FileResult result, DateTime resultDate, StatusType status)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                    {
                        {"@VendorId", result.OwnerId},
                        {"@StartUploadDate", resultDate.Date},
                        {"@EndUploadDate", DateTime.Now},
                        {"@Status", status},
                        {"@Comment", string.Format("Total records added: {0}, Error message: {1}", result.TotalRecords, result.ErrorMessage)},
                        {"@resultDate", resultDate.Date}
                    };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"UPDATE inventoryuploadstatus 
                    SET EndUploadDate=@EndUploadDate,Status=@Status,Comment=@Comment 
                    WHERE VendorId=@VendorId AND CAST(StartUploadDate AS DATE)=@resultDate", parameters);
            }
        }

        public UploadStatus GetUploadStatus(long vendorId, DateTime resultDate)
        {
            var status = default(UploadStatus);
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId},
                    {"@resultDate", resultDate.Date}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,VendorId,StartUploadDate,EndUploadDate,Status,Attempt FROM inventoryuploadstatus
                WHERE VendorId=@VendorId and CAST(StartUploadDate AS DATE)=@resultDate", parameters);

                while (reader.Read())
                {
                    status = new UploadStatus();
                    status.Id = (long)reader[0];
                    status.VendorId = (int)reader[1];
                    status.StartUploadDate = reader[2] == DBNull.Value ? null : (DateTime?)reader[2];
                    status.EndUploadDate = reader[3] == DBNull.Value ? null : (DateTime?)reader[3];
                    status.StatusType = (StatusType)reader[4];
                    status.Attempt = (int)reader[5];
                }
            }

            return status;
        }
    }
}
