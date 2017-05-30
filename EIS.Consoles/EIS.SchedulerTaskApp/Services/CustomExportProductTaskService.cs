using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using CsvHelper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Repositories;
using MySql.Data.MySqlClient;
using EIS.Inventory.Shared.Models;
using System.Linq;

namespace EIS.SchedulerTaskApp.Services
{
    public class CustomExportProductTaskService : TaskService
    {
        private readonly customexportproducttask _task;
        private bool _hasAnyData;

        public CustomExportProductTaskService(customexportproducttask task)
        {
            _task = task;
            _generatedFile = string.Format("{0}\\{1}\\{2}{3}",
                _exportedFileFolder,
                TaskType,
                string.Format(task.FileName, DateTime.Now),
                task.FileType);
        }

        public override string TaskType
        {
            get { return ScheduledTaskType.CUSTOM_EXPORT_PRODUCT; }
        }

        public override bool Execute()
        {
            var query = new StringBuilder();

            query.AppendFormat(@"SELECT {0} 
                    FROM products products 
                    LEFT JOIN shadows shadows ON shadows.ShadowSKU = products.EisSKU AND shadows.IsConnected = 1
                    WHERE products.CompanyId IN ({1})",
                getParsedSqlProjectionProductFields(_task.CustomFieldsList),
                _task.CompanyIds);
            
            using (var conn = new MySqlConnection(_connectionString))
            {
                Console.WriteLine("Creating {0} file -> {1}", TaskType, _generatedFile);

                using (var streamWriter = new StreamWriter(_generatedFile))
                {
                    // execute the query 
                    var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, query.ToString(), null);
                    var csvWriter = new CsvWriter(streamWriter);
                    
                    // write first the headers for the file
                    if (_task.HasHeader)
                    {
                        foreach (var header in _task.FileHeadersList)
                            csvWriter.WriteField(header);

                        // move on to the next row
                        csvWriter.NextRecord();
                    }

                    // read the data from SQL reader and write it directly to the file
                    while (reader.Read())
                    {
                        for (var index = 0; index < _task.CustomFieldsCount; index++)
                            csvWriter.WriteField(reader[index]);

                        // move to the next row
                        csvWriter.NextRecord();
                        _hasAnyData = true;
                    }
                }

                Console.WriteLine("Done creating {0} file -> {1}", TaskType, _generatedFile);
            }
            return true;
        }

        public override void DoPostExecution()
        {
            if (!_hasAnyData)
            {
                Logger.LogWarning(LogEntryType.CustomExportProductTaskService,
                    "No products have been exported for scheduled task name: " + _task.Name);
                return;
            }

            // write the file into task's FTP define
            if (_task.ExportTo == ExportTo.FTP)
            {
                var ftpRequestor = new FtpWebRequestor(_task.FtpServer,
                    _task.FtpUser,
                    _task.FtpPassword, 
                    _task.FtpPort, 
                    _task.RemoteFolder);
                ftpRequestor.WriteFtpFile(_generatedFile);
            }
            else if (_task.ExportTo == ExportTo.EMAIL)
            {
                // get the message template
                var msgTemplate = getMessageTemplate(TaskType);
                EmailSender.SendMessage(_task.EmailTo == null ? null : _task.EmailTo.Split(','),
                    _task.EmailCc == null ? null : _task.EmailCc.Split(','), 
                    _task.EmailSubject,
                    msgTemplate == null ? string.Empty : msgTemplate.ContentHtml,
                    _generatedFile);
            }

            // create export file record for this taks for the generated file
            createExportedFilesRecord(_task.Id);
        }

        private string getParsedSqlProjectionProductFields(List<string> customFields)
        {
            var parsedProductFields = customFields.ToList();

            // let's find if there is SellerPrice, AccurateShipping and AccurateWeight fields
            for (var i = 0; i < parsedProductFields.Count; i++)
            {
                var currentFieldName = parsedProductFields[i];

                if (currentFieldName.StartsWith("products.") || currentFieldName.StartsWith("vendor_product."))
                {
                    if (currentFieldName == "vendor_product.EisSupplierSKU")
                        parsedProductFields[i] = "(SELECT vwa.EisSupplierSKU FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = products.EisSKU LIMIT 1) AS EisSupplierSKU";

                    if (currentFieldName == "vendor_product.Quantity")
                        parsedProductFields[i] = "(SELECT CASE WHEN products.SkuType = 1 THEN FLOOR((vwa.Quantity * vwa.MinPack) / shadows.FactorQuantity) ELSE vwa.Quantity END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = products.EisSKU LIMIT 1) AS Quantity";

                    if (currentFieldName == "vendor_product.SupplierPrice")
                        parsedProductFields[i] = "(SELECT CASE WHEN products.SkuType = 1 THEN ROUND((vwa.SupplierPrice * (shadows.FactorQuantity / vwa.MinPack)), 2) ELSE vwa.SupplierPrice END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = products.EisSKU LIMIT 1) AS SupplierPrice";

                    if (currentFieldName == "products.SellerPrice" && _task.IsAddDropShipFee)
                        parsedProductFields[i] = @"(products.SellerPrice + vendors.DropShipFee) AS SellerPrice";

                    if (currentFieldName == "products.AccurateWeight" && _task.IsUseGuessedWeight)
                        parsedProductFields[i] = @"IF(NULLIF(products.AccurateWeight, '') IS NULL, products.GuessedWeight, products.AccurateWeight) AS AccurateWeight";

                    if (currentFieldName == "products.AccurateShipping" && _task.IsUseGuessedShipping)
                        parsedProductFields[i] = @"IF(NULLIF(products.AccurateShipping, '') IS NULL, products.GuessedShipping, products.AccurateShipping) AS AccurateShipping";

                }
                else
                {
                    parsedProductFields[i] = string.Format("\'{0}\' AS \'{1}\'", currentFieldName, currentFieldName);
                }
            }

            return string.Join(",", parsedProductFields);
        }
    }
}
