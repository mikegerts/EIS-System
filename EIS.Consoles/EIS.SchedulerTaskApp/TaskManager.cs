using System;
using EIS.SchedulerTaskApp.Repositories;
using EIS.SchedulerTaskApp.Services;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp
{
    public class TaskManager
    {
        private readonly TaskRepository _repository;

        public TaskManager()
        {
            _repository = new TaskRepository();
        }

        public void ExecuteScheduledTask(ScheduledTask task)
        {
            // get the more detailed scheduled task
            var refineTask = _repository.GetScheduledTask(task.Id);
            var taskService = _createTaskServiceInstance(refineTask);

            // execute the required action for the task
            var isContinue = taskService.Execute();

            // do some post execution for the task, if we want it to continue
            if (isContinue)
                taskService.DoPostExecution();
        }

        public void ExecuteCustomerScheduledTask(ScheduledTask task)
        {
            // get the more detailed scheduled task
            var refineTask = _repository.GetCustomerScheduledTask(task.Id);
            var taskService = _createCustomerTaskServiceInstance(refineTask);

            // execute the required action for the task
            var isContinue = taskService.Execute();

            // do some post execution for the task, if we want it to continue
            if (isContinue)
                taskService.DoPostExecution();
        }

        public void UpdateScheduledTaskExecution(ScheduledTask task)
        {
            // update the task last execution and set the IsRunNow to false
            _repository.UpdateScheduledTaskLastExecution(task.Id);
        }


        public void UpdateCustomerScheduledTaskExecution(ScheduledTask task)
        {
            // update the task last execution and set the IsRunNow to false
            _repository.UpdateCustomerScheduledTaskLastExecution(task.Id);
        }

        private TaskService _createCustomerTaskServiceInstance(customerscheduledtask task)
        {
            if (task.TaskType == CustomerScheduledTaskType.CUSTOMER_EXPORT_SKU)
                return new CustomerExportWholeSalePriceSkuService(task as customerscheduledtask);
            else
                throw new ArgumentException("Unknown task type: " + task.TaskType);
        }


        private TaskService _createTaskServiceInstance(scheduledtask task)
        {
            if (task.TaskType == ScheduledTaskType.GENERATE_PO)
                return new GeneratePoTaskService(task as generatepotask);
            else if (task.TaskType == ScheduledTaskType.CUSTOM_EXPORT_ORDER)
                return new CustomExportOrderTaskService(task as customexportordertask);
            else if (task.TaskType == ScheduledTaskType.MARKETPLACE_INVENTORY)
                return new MarketplaceInventoryTaskService(task as marketplaceinventorytask);
            else if (task.TaskType == ScheduledTaskType.VENDOR_PRODUCT_FILE_INVENTORY)
                return new VendorProductFileInventoryTaskService(task as vendorproductfileinventorytask);
            else if (task.TaskType == ScheduledTaskType.CUSTOM_EXPORT_PRODUCT)
                return new CustomExportProductTaskService(task as customexportproducttask);
            else if (task.TaskType == ScheduledTaskType.CUSTOM_IMPORT_ORDER)
                return new CustomImportOrderTaskService(task as customimportordertask);
            else
                throw new ArgumentException("Unknown task type: " + task.TaskType);
        }
    }
}
