using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using X.PagedList;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class ScheduledTaskService : IScheduledTaskService
    {
        private readonly EisInventoryContext _context;

        public ScheduledTaskService()
        {
            _context = new EisInventoryContext();
        }

        public IEnumerable<ScheduledTaskListDto> GetAllScheduledTasks()
        {
            return _context.scheduledtasks.OrderBy(x => x.Id)
                .ProjectTo<ScheduledTaskListDto>();
        }

        public ScheduledTaskDto GetScheduledTask(int id)
        {
            var result = _context.scheduledtasks
                .FirstOrDefault(x => x.Id == id);

            return convertToModel(result);
        }

        public bool CreateScheduledTask(ScheduledTaskDto model)
        {
            try
            {
                // unbox the correct object type for the credential
                var task = convertToDomainObject(model);
                task.StartTime = model.StartTimeDate.TimeOfDay;
                task.CreatedBy = model.ModifiedBy;
                task.Created = DateTime.UtcNow;

                _context.scheduledtasks.Add(task);
                _context.SaveChanges();
                model.Id = task.Id;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                return false;
            }

            return true;
        }

        public bool UpdateScheduledTask(int id, ScheduledTaskDto model)
        {
            // get the curent scheduled task
            var existingTask = _context.scheduledtasks.FirstOrDefault(x => x.Id == id);

            // unbox to the correct object for scheduled task
            var updatedTask = convertToDomainObject(model);
            updatedTask.StartTime = model.StartTimeDate.TimeOfDay;
            updatedTask.ModifiedBy = model.ModifiedBy;
            updatedTask.Modified = DateTime.UtcNow;

            _context.Entry(existingTask).CurrentValues.SetValues(updatedTask);
            _context.SaveChanges();

            return true;
        }

        public bool DeleteScheduledTask(int id)
        {
            var task = _context.scheduledtasks.FirstOrDefault(x => x.Id == id);
            if (task == null)
                return true;

            _context.scheduledtasks.Remove(task);
            _context.SaveChanges();

            return true;
        }

        public IPagedList<ExportedFileDto> GetPagedExportedFiles(int taskId, int page, int pageSize)
        {
            var results = _context.exportedfiles.Where(x => x.ScheduledTaskId == taskId);

            return results
                .OrderByDescending(x => x.Id)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<exportedfile, ExportedFileDto>();
        }

        public void SetTaskToExecuteNow(int taskId)
        {
            var task = _context.scheduledtasks.FirstOrDefault(x => x.Id == taskId);
            if (task == null)
                return;

            task.IsRunNow = true;
            _context.SaveChanges();
        }
        
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose other managed resources.
                    _context.Dispose();
                }
                //release unmanaged resources.
            }
            _disposed = true;
        }
        #endregion

        private ScheduledTaskDto convertToModel(scheduledtask task)
        {
            ScheduledTaskDto model = null;
            if (task is customexportordertask)
                model = Mapper.Map<CustomExportOrderTaskDto>(task as customexportordertask);
            else if (task is generatepotask)
                model = Mapper.Map<GeneratePoTaskDto>(task as generatepotask);
            else if (task is marketplaceinventorytask)
                model = Mapper.Map<MarketplaceInventoryTaskDto>(task as marketplaceinventorytask);
            else if (task is vendorproductfileinventorytask)
                model = Mapper.Map<VendorProductFileInventoryTaskDto>(task as vendorproductfileinventorytask);
            else if (task is customexportproducttask)
                model = Mapper.Map<CustomExportProductTaskDto>(task as customexportproducttask);
            else if (task is customimportordertask)
                model = Mapper.Map<CustomImportOrderTaskDto>(task as customimportordertask);
            else
                throw new InvalidCastException(string.Format("Unknown credential type \'{0}\' for casting!", task.TaskType));

            // create the datetime object for the StartTime
            var today = DateTime.Now;
            model.StartTimeDate = new DateTime(today.Year, today.Month, today.Day, task.StartTime.Hours, task.StartTime.Minutes, task.StartTime.Seconds);
            
            return model;
        }
        
        private scheduledtask convertToDomainObject(ScheduledTaskDto taskModel)
        {
            scheduledtask task = null;
            if (taskModel is CustomExportOrderTaskDto)
                task = Mapper.Map<customexportordertask>(taskModel as CustomExportOrderTaskDto);
            else if (taskModel is GeneratePoTaskDto)
                task = Mapper.Map<generatepotask>(taskModel as GeneratePoTaskDto);
            else if (taskModel is MarketplaceInventoryTaskDto)
                task = Mapper.Map<marketplaceinventorytask>(taskModel as MarketplaceInventoryTaskDto);
            else if (taskModel is VendorProductFileInventoryTaskDto)
                task = Mapper.Map<vendorproductfileinventorytask>(taskModel as VendorProductFileInventoryTaskDto);
            else if (taskModel is CustomExportProductTaskDto)
                task = Mapper.Map<customexportproducttask>(taskModel as CustomExportProductTaskDto);
            else if (taskModel is CustomImportOrderTaskDto)
                task = Mapper.Map<customimportordertask>(taskModel as CustomImportOrderTaskDto);
            else
                throw new InvalidCastException(string.Format("Unknown credential type \'{0}\' for casting!", task.TaskType));

            return task;
        }
    }
}
