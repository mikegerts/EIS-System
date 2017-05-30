using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CsvHelper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Repositories;
using MySql.Data.MySqlClient;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Services
{
    public class CustomExportOrderTaskService : TaskService
    {
        private readonly customexportordertask _task;
        private readonly  List<string> _exportedEisSupplierSKUs;
        private readonly List<string> _tableNames;
        private bool _hasOrders = false;

        public CustomExportOrderTaskService(customexportordertask task)
        {
            _task = task;
            _exportedEisSupplierSKUs = new List<string>();
            _generatedFile = string.Format("{0}\\{1}\\{2}{3}",
                _exportedFileFolder,
                TaskType,
                string.Format(task.FileName, DateTime.Now), 
                task.FileType);
            _tableNames = new List<string>() {
                "orders.",
                "orderitems.",
                "vendorproducts.",
                "orderproducts."
            };
        }

        public override string TaskType
        {
            get { return ScheduledTaskType.CUSTOM_EXPORT_ORDER; }
        }

        public override bool Execute()
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", _task.VendorId},
                {"@IsExported", _task.IsExported },
                {"@StatusIn", string.IsNullOrEmpty(_task.StatusIn) ?  -1 : 2 },
            };

            var orderStatusesIn = string.IsNullOrEmpty(_task.StatusIn) ? "-1"
                : string.Join(", ", _task.OrderStatusInList);

            try
            {
                Console.WriteLine("Creating {0} file -> {1}", TaskType, _generatedFile);

                using (var streamWriter = new StreamWriter(_generatedFile))
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        // let's parsed some of its columns for advanced query
                        var customizedOrderFields = parsedSqlProjectionOrderFields(_task.CustomFieldsList);
                        var query = string.Format(@"SELECT orderproducts.EisSupplierSKU, {0} 
                                        FROM orderproducts orderproducts
                                        INNER JOIN vendorproducts vendorproducts
	                                        ON vendorproducts.EisSupplierSKU = orderproducts.EisSupplierSKU
                                        INNER JOIN orderitems orderitems
	                                        ON orderitems.OrderItemId = orderproducts.OrderItemId
                                        INNER JOIN orders orders
	                                        ON orders.OrderId = orderitems.OrderId    
	                                        AND (@StatusIn = -1 OR orders.OrderStatus IN ({1}))
                                        WHERE vendorproducts.VendorId = @VendorId AND (@IsExported = -1 OR orderproducts.IsExported = @IsExported)
                                        ORDER BY orders.PurchaseDate DESC", string.Join(",", customizedOrderFields), orderStatusesIn);

                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                                query,
                                parameters);

                        var csvWriter = new CsvWriter(streamWriter);

                        // write the header text if its want to include
                        if (_task.HasHeader)
                        {
                            foreach (var header in _task.FileHeadersList)
                                csvWriter.WriteField(header);

                            csvWriter.NextRecord();
                        }

                        while (reader.Read())
                        {
                            // add the EisSupplierSKU to the list first
                            _exportedEisSupplierSKUs.Add(reader["EisSupplierSKU"].ToString());
                            _hasOrders = true;

                            // we will use the customized Order Fields to get the value from DataReader
                            for (var i = 0; i < customizedOrderFields.Count; i++)
                            {
                                // we move index by 1 since OrderId is in 0
                                var index = i + 1;
                                csvWriter.WriteField(reader[index]);
                            }

                            csvWriter.NextRecord();
                        }
                    }
                }

                Console.WriteLine("Done creating {0} file -> {1}", TaskType, _generatedFile);
                return true;
            }
            catch (Exception ex)
            {               
                Logger.LogError(LogEntryType.ExportOrderTaskService,
                    string.Format("Error in writing file for {0}. <br/> Error message: {1}",
                    _task.Name,
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return false;
            }
        }

        public override void DoPostExecution()
        {
            try
            {
                if (!_exportedEisSupplierSKUs.Any())
                    Logger.LogInfo(LogEntryType.ExportOrderTaskService, "No orders have been exported for task name: " + _task.Name);

                // write the file into task's FTP define
                if (_task.ExportTo.Equals("FTP") && (_task.IsDropNoOrderFile || _hasOrders))
                {
                    var ftpRequestor = new FtpWebRequestor(_task.FtpServer, _task.FtpUser, _task.FtpPassword, _task.FtpPort, _task.RemoteFolder);
                    ftpRequestor.WriteFtpFile(_generatedFile);
                }

                // send confirmation message
                if (!string.IsNullOrEmpty(_task.ConfirmationEmailTos))
                {
                    EmailSender.SendConfirmationMessage(_task.ConfirmationEmailTos.Split(','), _hasOrders, _generatedFile);
                }

                // insert new record for exported file logs
                if (_hasOrders)
                    createExportedFilesRecord(_task.Id);

                // mark the orders as exported
                if (_task.MarkOrderExport && _exportedEisSupplierSKUs.Any())
                    markOrdersAsExported();
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.ExportOrderTaskService,
                    string.Format("Error in sending email for {0}. <br/> Error message: {1}",
                    _task.Name,
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
            }
        }

        private void markOrdersAsExported()
        {
            using(var context = new EisInventoryContext())
            {
                // get the order products
                var orderProducts = context.orderproducts
                    .Where(o => _exportedEisSupplierSKUs.Contains(o.EisSupplierSKU))
                    .ToList();

                // marked as IsExported to true
                orderProducts.ForEach(o =>
                {
                    o.IsExported = true;
                    o.ExportedDate = DateTime.UtcNow;
                });

                context.SaveChanges();
            }
        }

        private List<string> parsedSqlProjectionOrderFields(List<string> orderFields)
        {
            var parsedOrderFields = orderFields.ToList();

            // let's find if there is QtyOrdered field
            for (var i = 0; i < parsedOrderFields.Count; i++)
            {
                var fieldName = parsedOrderFields[i];
                if (!hasTableName(fieldName))
                {
                    parsedOrderFields[i] = string.Format("\'{0}\' AS \'{1}\'", fieldName, fieldName);
                }
                else if (fieldName == "orderitems.QtyOrdered")
                    parsedOrderFields[i] = "orderproducts.Quantity AS QtyOrdered"; // use the orderproducts'Quantity as the real Qty taken for this order
            }

            return parsedOrderFields;
        }

        private string removePrefixColumnName(string columnName)
        {
            if (columnName.Equals("orders.OrderId"))
                return "Marketplace OrderId";

            return columnName.Split('.')[1];
        }

        private bool hasTableName(string field)
        {
            return _tableNames.Any(name => field.StartsWith(name));
        }
    }
}
