using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Models;

namespace EIS.DataUploaderApp.Repositories
{
    public class ProductSettingRepository
    {
        private readonly string _connectionString;

        public ProductSettingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueryable<ProductFileSetting> GetProductFileSettings(DateTime runDateTime)
        {
            var settings = new List<ProductFileSetting>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>(){
                    {"@NextRunDate", runDateTime.Date}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension
                        ,SKU,Name,Description,ShortDescription,Category,UPCCode,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile,IsRunNow,LastExecutedOn
                    FROM productfilesettings
                    WHERE IsRunNow = 1 OR CAST(NextRunDate AS Date)<=@NextRunDate", parameters);

                while (reader.Read())
                {
                    var setting = new ProductFileSetting();
                    setting.VendorId = (int)reader[0];
                    setting.FileName = reader[1].ToString();
                    setting.FilePath = reader[2].ToString();
                    setting.TransferPath = reader[3].ToString();
                    setting.ReadTime = Convert.ToDateTime(reader[4].ToString());//  reader[4] == DBNull.Value ? default(TimeSpan) : (TimeSpan)reader[4];
                    setting.RowAt = (int)reader[5];
                    setting.FileType = (FileType)reader[6];
                    setting.NextRunDate = reader[7] == DBNull.Value ? null : (DateTime?)reader[7];
                    setting.Extension = reader[8].ToString();
                    setting.SKU = (int)reader[9];
                    setting.Name = (int)reader[10];
                    setting.Description = (int)reader[11];
                    setting.ShortDescription = (int)reader[12];
                    setting.Category = (int)reader[13];
                    setting.UPCCode = reader[14] == DBNull.Value ? null : (int?)reader[14];
                    setting.Cost = (int)reader[15];
                    setting.Quantity = (int)reader[16];
                    setting.Delimiter = string.IsNullOrEmpty(reader[17].ToString()) ? default(char) : reader[17].ToString()[0];
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

        public void SetProductNextUploadDate(long vendorId, DateTime nextUploadDate)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>(){
                    {"@NextRunDate", nextUploadDate.Date},
                    {"@VendorId", vendorId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                   @"UPDATE productfilesettings 
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
                   @"UPDATE productfilesettings 
                    SET LastExecutedOn=@LastExecutedOn,
                        IsRunNow = 0 
                    WHERE VendorId=@VendorId", parameters);
            }
        }
    }
}
