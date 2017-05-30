using EIS.SchedulerTaskApp.Helpers;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Repositories;
using System.Linq;
using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models;
using System;

namespace EIS.SchedulerTaskApp.Services
{
    public class CustomImportOrderTaskService : TaskService
    {
        private readonly customimportordertask _task;


        public CustomImportOrderTaskService(customimportordertask task)
        {
            _task = task;
        }



        public override string TaskType
        {
            get { return ScheduledTaskType.CUSTOM_IMPORT_ORDER; }
        }

        public override bool Execute()
        {
            try
            {
                Logger.LogInfo(LogEntryType.OrderService, "Started parsing the order  inventory file.");

                TaskRepository taskRepo = new TaskRepository();
                var currentTaskAlreadyImportedFiles = taskRepo.GetCurrentScheduleAlreadyImportFiles(_task.Id);
                var ftpRequestor = new FtpWebRequestor(_task.FtpServer, _task.FtpUser, _task.FtpPassword, _task.FtpPort, _task.RemoteFolder);
                var getFiles = ftpRequestor.GetFileList(_task.FileType,_task.FileName);

                var fileNotImported = getFiles.Where(x => !currentTaskAlreadyImportedFiles.Any(y => y.FileName.ToLower() == x.ToLower())).ToList();

                OrderRepository orderRepo;
                foreach (var file in fileNotImported)
                {
                    var orders = new List<Order>();
                    orderRepo = new OrderRepository();

                    var fileOrderItems = CsvFileDataParser.GetOrderItems(ftpRequestor.GetFileStreamReader(file), _task.HasHeader, _task.FileHeaders, _task.CustomFields);
                    var fileOrders = CsvFileDataParser.ParseOrderFile(ftpRequestor.GetFileStreamReader(file), orders, fileOrderItems, _task.HasHeader, _task.FileHeaders, _task.CustomFields);

                    List<Order> importedOrder = new List<Order>();
                    foreach (var order in orders)
                    {
                        if (importedOrder.Count == 0)
                        {
                            importedOrder.Add(order);
                            orderRepo.ManageOrders(order);
                        }
                        else if (importedOrder.Count > 0)
                        {
                            var isImported = importedOrder.Any(x => x.OrderId == order.OrderId);
                            if (!isImported)
                            {
                                importedOrder.Add(order);
                                orderRepo.ManageOrders(order);
                            }
                        }
                    }
                }

                taskRepo.ManageImportFileSchedule(fileNotImported, _task.Id);

                Logger.LogInfo(LogEntryType.OrderService, "Successfully parsed the order inventory file.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FileInventoryTaskService,
                    "Error in parsing the import inventory file! Message: " + EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return false;
            }
        }

        public override void DoPostExecution()
        {

        }

    }
}