using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CsvHelper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Models;
using EIS.SchedulerTaskApp.Profits;
using EIS.SchedulerTaskApp.Repositories;
using MySql.Data.MySqlClient;

namespace EIS.SchedulerTaskApp.Services
{
    public class GeneratePoTaskService : TaskService
    {
        private readonly generatepotask _task;
        private readonly IProfitProcessor _profitProcessor;
        private PurchaseOrder _purchaseOrder;

        public GeneratePoTaskService(generatepotask task)
        {
            _task = task;
            _profitProcessor = new ProfitProcessor();
            _generatedFile = string.Format("{0}\\{1}\\{2}{3}",
                _exportedFileFolder,
                TaskType,
                string.Format(task.FileName, DateTime.Now),
                task.FileType);

        }

        public override string TaskType
        {
            get { return ScheduledTaskType.GENERATE_PO; }
        }

        public override bool Execute()
        {
            try
            {
                var exportedOrders = new List<PurchaseOrderItem>();
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", _task.VendorId },
                    {"@OrderStatus", (int)OrderStatus.Shipped}
                }; 

                using (var conn = new MySqlConnection(_connectionString))
                {
                    var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT orders.EisOrderId, vendorproducts.VendorId, vendorproducts.Name, vendorproducts.SupplierSKU,
	                        orderproducts.EisSupplierSKU, orderproducts.Quantity as SupplierQtyOrdered,
	                        orderitems.QtyOrdered, vendorproducts.SupplierPrice,
	                        (orderitems.ShippingPrice + orderitems.GiftWrapPrice) as ShippingPrices,
	                        (orderitems.ItemTax +  orderitems.ShippingTax + orderitems.GiftWrapTax ) as Taxes,
	                        (orderitems.ShippingDiscount + orderitems.PromotionDiscount) as Discounts, orders.PurchaseDate
                        FROM orders orders 
                        INNER JOIN orderitems orderitems
	                        ON orders.OrderId = orderitems.OrderId
                        INNER JOIN orderproducts orderproducts
	                        ON orderproducts.OrderItemId = orderitems.OrderItemId
	                        AND orderproducts.IsPoGenerated = 0
                        INNER JOIN vendorproducts vendorproducts
	                        ON vendorproducts.EisSupplierSKU = orderproducts.EisSupplierSKU
                        WHERE orders.OrderStatus = @OrderStatus
	                        AND (@VendorId = -1 OR vendorproducts.VendorId = @VendorId) 
                        ORDER BY orders.PurchaseDate", parameters);

                    while (reader.Read())
                    {
                        var orderItem = new PurchaseOrderItem();
                        orderItem.EisOrderId = Convert.ToInt32(reader["EisOrderId"]);
                        orderItem.ItemName = reader["Name"].ToString();
                        orderItem.SupplierSKU = reader["SupplierSKU"].ToString();
                        orderItem.EisSupplierSKU = reader["EisSupplierSKU"].ToString();
                        orderItem.SupplierQtyOrdered = Convert.ToInt16(reader["SupplierQtyOrdered"]);
                        orderItem.UnitPrice = Convert.ToDecimal(reader["SupplierPrice"]);
                        orderItem.ShippingPrices = Convert.ToDecimal(reader["ShippingPrices"]);
                        orderItem.Taxes = Convert.ToDecimal(reader["Taxes"]);
                        orderItem.Discounts = Convert.ToDecimal(reader["Discounts"]);
                        orderItem.PurchaseDate = (DateTime)reader["PurchaseDate"];
                        exportedOrders.Add(orderItem);
                    }
                }

                // apply first the profits for this items
                exportedOrders.ForEach(item => _profitProcessor.Apply(item, _task.VendorId));

                // write the order items into the file and save it to the database
                persistOrderItems(exportedOrders);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("Error in writing file for {0}. \nError message: {1}\nStack Trace: {2}",
                    _task.Name,
                    EisHelper.GetExceptionMessage(ex),
                    ex.StackTrace));
                Logger.LogError(LogEntryType.GeneratePoTaskService,
                    string.Format("Error in writing PO file for {0}. <br/> Error message: {1}",
                    _task.Name, 
                    EisHelper.GetExceptionMessage(ex),
                    ex.StackTrace));
                return false;
            }
        }

        public override void DoPostExecution()
        {
            // write the file into task's FTP define
            if (_purchaseOrder == null)
            {
                Logger.LogInfo(LogEntryType.GeneratePoTaskService, "No PO generated for scheduled task name: " + _task.Name);
                return;
            }

            if (_task.ExportTo == ExportTo.FTP)
            {
                var ftpRequestor = new FtpWebRequestor(_task.FtpServer, _task.FtpUser, _task.FtpPassword, _task.FtpPort, _task.RemoteFolder);
                ftpRequestor.WriteFtpFile(_generatedFile);
            }
            else if (_task.ExportTo == ExportTo.EMAIL)
            {
                EmailSender.SendPoMessage(_task.EmailTo.Split(','),
                    string.IsNullOrEmpty(_task.EmailCc) ? null : _task.EmailCc.Split(','),
                    _task.EmailSubject,
                    _purchaseOrder);
            }

            // create export file record for this taks for the generated file
            createExportedFilesRecord(_task.Id);          
            
            // update the EIS orders IsPoGenerated to TRUE
            markOrdersAsPoGenerated();
        }

        private void markOrdersAsPoGenerated()
        {
            using (var context = new EisInventoryContext())
            {  
                // get exported EIS order id
                var exportedEisSupplierSKUs = _purchaseOrder.Items.Select(x => x.EisSupplierSKU);

                // get the order products -> THIS WILL DO LIKE RIGHT-JOIN
                var orderProducts = context.orderproducts
                        .Where(x => exportedEisSupplierSKUs.Contains(x.EisSupplierSKU))
                        .ToList();

                // toggle the IsPoGenerated
                orderProducts.ForEach(o =>
                {
                    o.IsPoGenerated = true;
                    o.PoGeneratedDate = DateTime.UtcNow;
                });

                context.SaveChanges();
            }
        }
        
        private void persistOrderItems(List<PurchaseOrderItem> orders)
        {
            // write order items to the file and insert it into the database
            if (!orders.Any())
            {
                Logger.LogInfo(LogEntryType.GeneratePoTaskService, "No purchase orders to be generatated for task name: " + _task.Name);
                return;
            }

            // get the purchase order details
            _purchaseOrder = initPoVendorInfo(_task.VendorId);
            _purchaseOrder.Items = orders;
            _purchaseOrder.PurchaseOrderId = string.Format("{0:MM}{0:dd}{0:yy}-{0:hh}{0:mm}{0:ss}", DateTime.UtcNow);

            using (var streamWriter = new StreamWriter(_generatedFile))
            {
                using (var context = new EisInventoryContext())
                {
                    var csvWriter = new CsvWriter(streamWriter);

                    // write first the headers for the CSV file
                    csvWriter.WriteField("PurchaseOrderId");
                    csvWriter.WriteField("EisOrderId");
                    csvWriter.WriteField("SKU");
                    csvWriter.WriteField("Item");
                    csvWriter.WriteField("Quantity");
                    csvWriter.WriteField("Subtotal");
                    csvWriter.WriteField("VendorFees");
                    csvWriter.WriteField("PurchaseDate");
                    csvWriter.NextRecord();

                    // add first the purchase order data
                    context.purchaseorders.Add(new purchaseorder
                    {
                        Id = _purchaseOrder.PurchaseOrderId,
                        VendorId = _purchaseOrder.VendorId,
                        IsManual = false,
                        PaymentStatus = PaymentStatus.NotPaid,
                        Created = DateTime.UtcNow
                    });

                    // and save it
                    context.SaveChanges();

                    // iterate to its orders and save it to the database
                    foreach(var order in orders)
                    {
                        // write first to the file
                        csvWriter.WriteField(_purchaseOrder.PurchaseOrderId);
                        csvWriter.WriteField(order.EisOrderId);
                        csvWriter.WriteField(order.SupplierSKU);
                        csvWriter.WriteField(order.ItemName);
                        csvWriter.WriteField(order.SupplierQtyOrdered);
                        csvWriter.WriteField(order.Total);
                        csvWriter.WriteField(order.VendorFees);
                        csvWriter.WriteField(order.PurchaseDate);
                        csvWriter.NextRecord();

                        // then, add it to the database
                        context.purchaseorderitems.Add(new purchaseorderitem
                        {
                            PurchaseOrderId = _purchaseOrder.PurchaseOrderId,
                            EisOrderId = order.EisOrderId,
                            ItemName = order.ItemName,
                            SKU = order.SupplierSKU,
                            Qty = order.SupplierQtyOrdered,
                            UnitPrice = order.UnitPrice,
                            ShippingPrices = order.ShippingPrices,
                            Taxes = order.Taxes,
                            Discounts = order.Discounts,
                            IsPaid = order.IsPaid,
                            Profit = order.Profit
                        });
                    }

                    // save it to the database
                    context.SaveChanges();
                }
            }
            
            Console.WriteLine("Done creating PO file -> " + _generatedFile);
        }

        private PurchaseOrder initPoVendorInfo(int? vendorId)
        {
            var purchaseOrder = new PurchaseOrder();
            using(var context = new EisInventoryContext())
            {
                // get the vendor 
                var vendor = context.vendors.FirstOrDefault(x => x.Id == vendorId);
                if (vendor == null)
                    return purchaseOrder;

                purchaseOrder.VendorId = vendor.Id;
                purchaseOrder.VendorName = vendor.Name;
                purchaseOrder.ContactEmail = vendor.Email;
                purchaseOrder.Address = vendor.VendorAddress;
                purchaseOrder.ContactPerson = vendor.ContactPerson;
                purchaseOrder.City = vendor.City;
                purchaseOrder.ZipCode = vendor.ZipCode;
                purchaseOrder.PhoneNumber = vendor.PhoneNumber;
            }

            return purchaseOrder;
        }
    }
}
