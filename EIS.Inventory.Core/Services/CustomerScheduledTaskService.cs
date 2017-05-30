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
    public class CustomerScheduledTaskService : ICustomerScheduledTaskService
    {
        private readonly EisInventoryContext _context;

        public CustomerScheduledTaskService()
        {
            _context = new EisInventoryContext();
        }

        public bool CreateScheduledTask(CustomerScheduledTaskDto taskModel)
        {
            try
            {
                // unbox the correct object type for the credential
                var task = Mapper.Map<customerscheduledtask>(taskModel);
                task.StartTime = taskModel.StartTimeDate.TimeOfDay;
                task.CreatedBy = taskModel.ModifiedBy;
                task.Created = DateTime.UtcNow;

                _context.customerscheduledtasks.Add(task);
                _context.SaveChanges();
                taskModel.Id = task.Id;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                return false;
            }

            return true;
        }

        public bool DeleteScheduledTask(int id)
        {
            var task = _context.customerscheduledtasks.FirstOrDefault(x => x.Id == id);
            if (task == null)
                return true;

            _context.customerscheduledtasks.Remove(task);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<CustomerScheduledTaskListDto> GetAllScheduledTasks()
        {
            return _context.customerscheduledtasks.OrderBy(x => x.Id)
                .ProjectTo<CustomerScheduledTaskListDto>();
        }

        public IEnumerable<CustomerScheduledTaskListDto> GetAllScheduledTasksByCustomerId(int customerId)
        {
            return _context.customerscheduledtasks.Where(x => x.CustomerId == customerId).OrderBy(x => x.Id)
                .ProjectTo<CustomerScheduledTaskListDto>();
        }

        public IPagedList<CustomerExportedFileDto> GetPagedExportedFiles(int taskId, int page, int pageSize)
        {
            var results = _context.customerexportedfiles.Where(x => x.ScheduledTaskId == taskId);

            return results
                .OrderByDescending(x => x.Id)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<customerexportedfile, CustomerExportedFileDto>();
        }

        public CustomerScheduledTaskDto GetScheduledTask(int id)
        {
            var result = _context.customerscheduledtasks
               .FirstOrDefault(x => x.Id == id);

            return Mapper.Map<customerscheduledtask, CustomerScheduledTaskDto>(result);
        }

        public void SetTaskToExecuteNow(int taskId)
        {
            var task = _context.customerscheduledtasks.FirstOrDefault(x => x.Id == taskId);
            if (task == null)
                return;

            task.IsRunNow = true;
            _context.SaveChanges();
        }

        public bool UpdateScheduledTask(int id, CustomerScheduledTaskDto taskModel)
        {
            // get the curent scheduled task
            var existingTask = _context.customerscheduledtasks.FirstOrDefault(x => x.Id == id);

            // unbox to the correct object for scheduled task
            var updatedTask = Mapper.Map<customerscheduledtask>(taskModel);
            updatedTask.StartTime = taskModel.StartTimeDate.TimeOfDay;
            updatedTask.ModifiedBy = taskModel.ModifiedBy;
            updatedTask.Modified = DateTime.UtcNow;

            _context.Entry(existingTask).CurrentValues.SetValues(updatedTask);
            _context.SaveChanges();

            return true;
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
    }
}
