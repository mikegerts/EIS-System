using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class FileSettingService : IFileSettingService
    {
        public readonly string _connectionString;

        public FileSettingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<FileSettingViewModel> GetProductFileSettings()
        {
            var settings = new List<FileSettingViewModel>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,VendorName,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension,
                        SKU,Name,Description,ShortDescription,Category,UPC,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile
                    FROM productfilesettings", null);

                while (reader.Read())
                {
                    var setting = new FileSettingViewModel();
                    setting.VendorId = (int)reader[0];
                    setting.VendorName = reader[1].ToString();
                    setting.FileName = reader[2].ToString();
                    setting.FilePath = reader[3].ToString();
                    setting.TransferPath = reader[4].ToString();
                    setting.ReadTime = reader[5] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(((TimeSpan)reader[5]).ToString());
                    setting.RowAt = (int)reader[6];
                    setting.FileType = (FileType)reader[7];
                    setting.NextRunDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    setting.Extension = reader[9].ToString();
                    setting.SKU = (int)reader[10];
                    setting.Name = reader[11] as int?;
                    setting.Description = reader[12] as int?;
                    setting.ShortDescription = reader[13] as int?;
                    setting.Category = reader[14] as int?;
                    setting.UPC = reader[15] as int?;
                    setting.Cost = reader[16] as int?;
                    setting.Quantity = reader[17] as int?;
                    setting.Delimiter = reader[18] == DBNull.Value ? ';' : reader[18].ToString()[0];
                    setting.FtpServer = reader[19].ToString();
                    setting.FtpUser = reader[20].ToString();
                    setting.FtpPassword = reader[21].ToString();
                    setting.FtpPort = reader[22] == DBNull.Value ? null : (int?)reader[22];
                    setting.RemoteFolder = reader[23].ToString();
                    setting.IsDeleteFile = Convert.ToBoolean(reader[24]);

                    settings.Add(setting);
                }
            }

            return settings.AsQueryable();
        }

        public FileSettingViewModel GetProductFileSettingByVendor(long vendorId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", vendorId}
            };

            var setting = default(FileSettingViewModel);
            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,VendorName,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension,
                        SKU,Name,Description,ShortDescription,Category,UPC,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile 
                    FROM productfilesettings
                    WHERE VendorId=@VendorId", parameters);

                while (reader.Read())
                {
                    setting = new FileSettingViewModel();
                    setting.VendorId = (int)reader[0];
                    setting.VendorName = reader[1].ToString();
                    setting.FileName = reader[2].ToString();
                    setting.FilePath = reader[3].ToString();
                    setting.TransferPath = reader[4].ToString();
                    setting.ReadTime = reader[5] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(((TimeSpan)reader[5]).ToString());
                    setting.RowAt = (int)reader[6];
                    setting.FileType = (FileType)reader[7];
                    setting.NextRunDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    setting.Extension = reader[9].ToString();
                    setting.SKU = (int)reader[10];
                    setting.Name = reader[11] as int?;
                    setting.Description = reader[12] as int?;
                    setting.ShortDescription = reader[13] as int?;
                    setting.Category = reader[14] as int?;
                    setting.UPC = reader[15] as int?;
                    setting.Cost = reader[16] as int?;
                    setting.Quantity = reader[17] as int?;
                    setting.Delimiter = reader[18] == DBNull.Value ? ';' : reader[18].ToString()[0];
                    setting.FtpServer = reader[19].ToString();
                    setting.FtpUser = reader[20].ToString();
                    setting.FtpPassword = reader[21].ToString();
                    setting.FtpPort = reader[22] == DBNull.Value ? null : (int?)reader[22];
                    setting.RemoteFolder = reader[23].ToString();
                    setting.IsDeleteFile = Convert.ToBoolean(reader[24]);
                }
            }

            return setting;
        }

        public bool CreateProductFileSetting(FileSettingViewModel viewModel)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", viewModel.VendorId },
                    {"@VendorName", viewModel.VendorName },
                    {"@FileName", viewModel.FileName },
                    {"@FilePath", viewModel.FilePath },
                    {"@TransferPath", viewModel.TransferPath },
                    {"@ReadTime", viewModel.ReadTime },
                    {"@RowAt", viewModel.RowAt },
                    {"@FileType", viewModel.FileType },
                    {"@NextRunDate", viewModel.NextRunDate.Value.Date },
                    {"@Extension", viewModel.Extension },
                    {"@SKU", viewModel.SKU },
                    {"@Name", viewModel.Name },
                    {"@Description", viewModel.Description },
                    {"@ShortDescription", viewModel.ShortDescription },
                    {"@Category", viewModel.Category },
                    {"@UPC", viewModel.UPC},
                    {"@Cost", viewModel.Cost},
                    {"@Quantity", viewModel.Quantity},
                    {"@Dilimeter", viewModel.Delimiter},
                    {"@FtpServer", viewModel.FtpServer},
                    {"@FtpUser", viewModel.FtpUser},
                    {"@FtpPassword", viewModel.FtpPassword},
                    {"@FtpPort", viewModel.FtpPort},
                    {"@RemoteFolder", viewModel.RemoteFolder},
                    {"@IsDeleteFile", viewModel.IsDeleteFile},
                    {"@Created", DateTime.UtcNow }, 
                };

                MySqlHelper.ExecuteNonQuery(conn,
                        @"INSERT INTO productfilesettings(VendorId,VendorName,FileName,Extension,FilePath,Dilimeter,TransferPath,
                        ReadTime,RowAt,FileType,NextRunDate,SKU,Quantity,Cost,Name,Description,ShortDescription,Category,UPC,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile,Created) 
                        VALUES(@VendorId,@VendorName,@FileName,@Extension,@FilePath,@Dilimeter,@TransferPath,
                        @ReadTime,@RowAt,@FileType,@NextRunDate,@SKU,@Quantity,@Cost,@Name,@Description,@ShortDescription,@Category,@UPC,
                        @FtpServer,@FtpUser,@FtpPassword,@FtpPort,@RemoteFolder,@IsDeleteFile,@Created)",
                    parameters);
            }

            return true;
        }

        public bool UpdateProductFileSetting(long vendorId, FileSettingViewModel viewModel)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId },
                    {"@FileName", viewModel.FileName },
                    {"@FilePath", viewModel.FilePath },
                    {"@TransferPath", viewModel.TransferPath },
                    {"@ReadTime", viewModel.ReadTime },
                    {"@RowAt", viewModel.RowAt },
                    {"@FileType", viewModel.FileType },
                    {"@NextRunDate", viewModel.NextRunDate.Value.Date },
                    {"@Extension", viewModel.Extension },
                    {"@SKU", viewModel.SKU },
                    {"@Name", viewModel.Name },
                    {"@Description", viewModel.Description },
                    {"@ShortDescription", viewModel.ShortDescription },
                    {"@Category", viewModel.Category },
                    {"@UPC", viewModel.UPC},
                    {"@Cost", viewModel.Cost},
                    {"@Quantity", viewModel.Quantity},
                    {"@Dilimeter", viewModel.Delimiter},
                    {"@FtpServer", viewModel.FtpServer},
                    {"@FtpUser", viewModel.FtpUser},
                    {"@FtpPassword", viewModel.FtpPassword},
                    {"@FtpPort", viewModel.FtpPort},
                    {"@RemoteFolder", viewModel.RemoteFolder},
                    {"@IsDeleteFile", viewModel.IsDeleteFile},
                    {"@Modified", DateTime.UtcNow }, 
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"UPDATE productfilesettings SET
                    FileName=@FileName,FilePath=@FilePath,TransferPath=@TransferPath,ReadTime=@ReadTime,RowAt=@RowAt,FileType=@FileType,NextRunDate=@NextRunDate,Extension=@Extension,
                    SKU=@SKU,Name=@Name,Description=@Description,ShortDescription=@ShortDescription,Category=@Category,UPC=@UPC,Cost=@Cost,Quantity=@Quantity,Dilimeter=@Dilimeter, 
                    FtpServer=@FtpServer,FtpUser=@FtpUser,FtpPassword=@FtpPassword,FtpPort=@FtpPort,RemoteFolder=@RemoteFolder,IsDeleteFile=@IsDeleteFile,Modified=@Modified
                    WHERE VendorId=@VendorId", parameters);
            }

            return true;
        }

        public bool DeleteProductFileSetting(long vendorId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId }
                };

                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM productfilesettings WHERE VendorId=@VendorId", parameters);
            }

            return true;
        }

        public IEnumerable<FileSettingViewModel> GetInventoryFileSettings()
        {
            var settings = new List<FileSettingViewModel>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,VendorName,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension,
                        SKU,Name,Description,ShortDescription,Category,UPC,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile 
                    FROM inventoryfilesettings", null);

                while (reader.Read())
                {
                    var setting = new FileSettingViewModel();
                    setting.VendorId = (int)reader[0];
                    setting.VendorName = reader[1].ToString();
                    setting.FileName = reader[2].ToString();
                    setting.FilePath = reader[3].ToString();
                    setting.TransferPath = reader[4].ToString();
                    setting.ReadTime = reader[5] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(((TimeSpan)reader[5]).ToString());
                    setting.RowAt = (int)reader[6];
                    setting.FileType = (FileType)reader[7];
                    setting.NextRunDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    setting.Extension = reader[9].ToString();
                    setting.SKU = (int)reader[10];
                    setting.Name = reader[11] as int?;
                    setting.Description = reader[12] as int?;
                    setting.ShortDescription = reader[13] as int?;
                    setting.Category = reader[14] as int?;
                    setting.UPC = reader[15] as int?;
                    setting.Cost = reader[16] as int?;
                    setting.Quantity = reader[17] as int?;
                    setting.Delimiter = reader[18] == DBNull.Value ? ';' : reader[18].ToString()[0];
                    setting.FtpServer = reader[19].ToString();
                    setting.FtpUser = reader[20].ToString();
                    setting.FtpPassword = reader[21].ToString();
                    setting.FtpPort = reader[22] == DBNull.Value ? null : (int?)reader[22];
                    setting.RemoteFolder = reader[23].ToString();
                    setting.IsDeleteFile = Convert.ToBoolean(reader[24]);

                    settings.Add(setting);
                }
            }

            return settings.AsQueryable();
        }

        public FileSettingViewModel GetInventoryFileSettingByVendor(long vendorId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", vendorId}
            };

            var setting = default(FileSettingViewModel);

            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                       @"SELECT VendorId,VendorName,FileName,FilePath,TransferPath,ReadTime,RowAt,FileType,NextRunDate,Extension,
                        SKU,Name,Description,ShortDescription,Category,UPC,Cost,Quantity,Dilimeter,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile 
                    FROM inventoryfilesettings
                    WHERE VendorId=@VendorId", parameters);
                
                while (reader.Read())
                {
                    setting = new FileSettingViewModel();
                    setting.VendorId = (int)reader[0];
                    setting.VendorName = reader[1].ToString();
                    setting.FileName = reader[2].ToString();
                    setting.FilePath = reader[3].ToString();
                    setting.TransferPath = reader[4].ToString();
                    setting.ReadTime = reader[5] == DBNull.Value ? default(DateTime) : Convert.ToDateTime(((TimeSpan)reader[5]).ToString());
                    setting.RowAt = (int)reader[6];
                    setting.FileType = (FileType)reader[7];
                    setting.NextRunDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    setting.Extension = reader[9].ToString();
                    setting.SKU = (int)reader[10];
                    setting.Name = reader[11] as int?;
                    setting.Description = reader[12] as int?;
                    setting.ShortDescription = reader[13] as int?;
                    setting.Category = reader[14] as int?;
                    setting.UPC = reader[15] as int?;
                    setting.Cost = reader[16] as int?;
                    setting.Quantity = reader[17] as int?;
                    setting.Delimiter = reader[18] == DBNull.Value ? ';' : reader[18].ToString()[0];
                    setting.FtpServer = reader[19].ToString();
                    setting.FtpUser = reader[20].ToString();
                    setting.FtpPassword = reader[21].ToString();
                    setting.FtpPort = reader[22] == DBNull.Value ? null : (int?)reader[22];
                    setting.RemoteFolder = reader[23].ToString();
                    setting.IsDeleteFile = Convert.ToBoolean(reader[24]);
                }
            }

            return setting;
        }

        public bool CreateInventoryFileSetting(FileSettingViewModel viewModel)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", viewModel.VendorId },
                    {"@VendorName", viewModel.VendorName },
                    {"@FileName", viewModel.FileName },
                    {"@FilePath", viewModel.FilePath },
                    {"@TransferPath", viewModel.TransferPath },
                    {"@ReadTime", viewModel.ReadTime },
                    {"@RowAt", viewModel.RowAt },
                    {"@FileType", viewModel.FileType },
                    {"@NextRunDate", viewModel.NextRunDate.Value.Date },
                    {"@Extension", viewModel.Extension },
                    {"@SKU", viewModel.SKU },
                    {"@Name", viewModel.Name },
                    {"@Description", viewModel.Description },
                    {"@ShortDescription", viewModel.ShortDescription },
                    {"@Category", viewModel.Category },
                    {"@UPC", viewModel.UPC},
                    {"@Cost", viewModel.Cost},
                    {"@Quantity", viewModel.Quantity},
                    {"@Dilimeter", viewModel.Delimiter},
                    {"@FtpServer", viewModel.FtpServer},
                    {"@FtpUser", viewModel.FtpUser},
                    {"@FtpPassword", viewModel.FtpPassword},
                    {"@FtpPort", viewModel.FtpPort},
                    {"@RemoteFolder", viewModel.RemoteFolder},
                    {"@IsDeleteFile", viewModel.IsDeleteFile},
                    {"@Created", DateTime.UtcNow }, 
                };

                MySqlHelper.ExecuteNonQuery(conn,
                        @"INSERT INTO inventoryfilesettings(VendorId,VendorName,FileName,Extension,FilePath,Dilimeter,TransferPath,
                        ReadTime,RowAt,FileType,NextRunDate,SKU,Quantity,Cost,Name,Description,ShortDescription,Category,UPC,
                        FtpServer,FtpUser,FtpPassword,FtpPort,RemoteFolder,IsDeleteFile,Created) 
                        VALUES(@VendorId,@VendorName,@FileName,@Extension,@FilePath,@Dilimeter,@TransferPath,
                        @ReadTime,@RowAt,@FileType,@NextRunDate,@SKU,@Quantity,@Cost,@Name,@Description,@ShortDescription,@Category,@UPC,
                        @FtpServer,@FtpUser,@FtpPassword,@FtpPort,@RemoteFolder,@IsDeleteFile,@Created)",
                    parameters);
            }

            return true;
        }

        public bool UpdateInventoryFileSetting(long vendorId, FileSettingViewModel viewModel)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId },
                    {"@FileName", viewModel.FileName },
                    {"@FilePath", viewModel.FilePath },
                    {"@TransferPath", viewModel.TransferPath },
                    {"@ReadTime", viewModel.ReadTime },
                    {"@RowAt", viewModel.RowAt },
                    {"@FileType", viewModel.FileType },
                    {"@NextRunDate", viewModel.NextRunDate.Value.Date },
                    {"@Extension", viewModel.Extension },
                    {"@SKU", viewModel.SKU },
                    {"@Name", viewModel.Name },
                    {"@Description", viewModel.Description },
                    {"@ShortDescription", viewModel.ShortDescription },
                    {"@Category", viewModel.Category },
                    {"@UPC", viewModel.UPC},
                    {"@Cost", viewModel.Cost},
                    {"@Quantity", viewModel.Quantity},
                    {"@Dilimeter", viewModel.Delimiter},
                    {"@FtpServer", viewModel.FtpServer},
                    {"@FtpUser", viewModel.FtpUser},
                    {"@FtpPassword", viewModel.FtpPassword},
                    {"@FtpPort", viewModel.FtpPort},
                    {"@RemoteFolder", viewModel.RemoteFolder},
                    {"@IsDeleteFile", viewModel.IsDeleteFile},
                    {"@Modified", DateTime.UtcNow }, 
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"UPDATE inventoryfilesettings SET
                    FileName=@FileName,FilePath=@FilePath,TransferPath=@TransferPath,ReadTime=@ReadTime,RowAt=@RowAt,FileType=@FileType,NextRunDate=@NextRunDate,Extension=@Extension,
                    SKU=@SKU,Name=@Name,Description=@Description,ShortDescription=@ShortDescription,Category=@Category,UPC=@UPC,Cost=@Cost,Quantity=@Quantity,Dilimeter=@Dilimeter,
                    FtpServer=@FtpServer,FtpUser=@FtpUser,FtpPassword=@FtpPassword,FtpPort=@FtpPort,RemoteFolder=@RemoteFolder,Modified=@Modified,IsDeleteFile=@IsDeleteFile 
                    WHERE VendorId=@VendorId", parameters);
            }

            return true;
        }

        public bool DeleteInventoryFileSetting(long vendorId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId }
                };

                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM inventoryfilesettings WHERE VendorId=@VendorId", parameters);
            }

            return true;
        }
    }
}
