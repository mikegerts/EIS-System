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
    public class ExportOrderTaskService : TaskService
    {
        private readonly exportordertask _task;
        private readonly  List<string> _exportedOrderIds;
        private bool _hasOrders = false;

        public ExportOrderTaskService(exportordertask task)
        {
            _task = task;
            _exportedOrderIds = new List<string>();
            _generatedFile = string.Format("{0}\\{3}\\{1}{2}",
                _exportedFileFolder,
                string.Format(task.FileName, DateTime.Now), 
                task.FileType,
                TaskType);
        }

        public override string TaskType
        {
            get { return ScheduledTaskType.EXPORT_ORDER; }
        }

        public override void Execute()
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
                        var customizedOrderFields = parsedSqlProjectionOrderFields(_task.OrderFieldsList);
                        var query = string.Format(@"SELECT orders.OrderId, {0} 
                                        FROM orderproducts orderproducts
                                        INNER JOIN vendorproducts vendorproducts
	                                        ON vendorproducts.EisSupplierSKU = orderproducts.EisSupplierSKU
                                        INNER JOIN orderitems orderitems
	                                        ON orderitems.OrderItemId = orderproducts.OrderItemId
                                        INNER JOIN orders orders
	                                        ON orders.OrderId = orderitems.OrderId    
	                                        AND (@StatusIn = -1 OR orders.OrderStatus IN ({1}))
                                        LEFT JOIN shadows shadows	ON shadows.ShadowSKU = orderitems.SKU
                                        WHERE vendorproducts.VendorId = @VendorId AND (@IsExported = -1 OR orderproducts.IsExported = @IsExported)
                                        ORDER BY orders.PurchaseDate DESC", string.Join(",", customizedOrderFields), orderStatusesIn);

                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                                query,
                                parameters);

                        var csvWriter = new CsvWriter(streamWriter);

                        // write the header text if its want to include
                        if (_task.HasHeader)
                        {
                            foreach (var field in _task.OrderFieldsList)
                                csvWriter.WriteField(removePrefixColumnName(field));

                            csvWriter.NextRecord();
                        }

                        while (reader.Read())
                        {
                            // add the order id to the list first
                            _exportedOrderIds.Add(reader["OrderId"].ToString());
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
            }
            catch (Exception ex)
            {               
                Logger.LogError(LogEntryType.ExportOrderTaskService,
                    string.Format("Error in writing file for {0}. <br/> Error message: {1}",
                    _task.Name,
                    ex.InnerException == null ? ex.Message : string.Format("{0} Inner Error: {1}", ex.Message, ex.InnerException.Message)),
                    ex.StackTrace);
            }
        }

        public override void DoPostExecution()
        {
            if (!_exportedOrderIds.Any())
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
            if (_task.MarkOrderExport && _exportedOrderIds.Any())
                markOrdersAsExported();
        }

        private void markOrdersAsExported()
        {
            using(var context = new EisInventoryContext())
            {
                // get the order products -> THIS WILL DO LIKE RIGHT-JOIN
                var orderProducts = context.orderproducts
                        .Join(context.orderitems,
                                op => op.OrderItemId,
                                oi => oi.OrderItemId,
                                (op, oi) => new { OrderProduct = op, OrderItem = oi })
                       .Join(context.orders.Where(x => _exportedOrderIds.Contains(x.OrderId)),
                                ooi => ooi.OrderItem.OrderId,
                                o => o.OrderId,
                                (ooi, o) => new { ooi.OrderProduct })
                       .Select(x => x.OrderProduct);

                // marked as IsExported to true
                orderProducts.ToList().ForEach(o => o.IsExported = true);

                context.SaveChanges();
            }
        }

        private List<string> parsedSqlProjectionOrderFields(List<string> orderFields)
        {
            var parsedOrderFields = orderFields.ToList();

            // let's find if there is QtyOrdered field
            for (var i = 0; i < parsedOrderFields.Count; i++)
            {
                if (parsedOrderFields[i] == "orderitems.QtyOrdered")
                    parsedOrderFields[i] = "IF(shadows.FactorQuantity IS NULL, orderitems.QtyOrdered, orderitems.QtyOrdered  * shadows.FactorQuantity) AS QtyOrdered";
            }

            return parsedOrderFields;
        }

        private string removePrefixColumnName(string columnName)
        {
            if (columnName.Equals("orders.OrderId"))
                return "Marketplace OrderId";

            return columnName.Split('.')[1];
        }
    }
}
