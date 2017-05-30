using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using EIS.Inventory.DAL.Database;
using AutoMapper.QueryableExtensions;
using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class TaskRepository
    {
        /// <summary>
        /// Get the list of all scheduled task tha are enabled
        /// </summary>
        /// <returns></returns>
        public List<ScheduledTask> GetCurrentScheduledTasks()
        {
            var todayDate = DateTime.Now.Date;
            List<ScheduledTask> results;

            using (var context = new EisInventoryContext())
            {
                results = context.scheduledtasks
                .Where(x => (x.IsRunNow || DbFunctions.TruncateTime(x.StartDate) <= todayDate)
                    && x.IsEnabled).Select(data => new ScheduledTask
                    {
                        Days = data.Days,
                        Id = data.Id,
                        IsRunNow = data.IsRunNow,
                        LastExecutedOn = data.LastExecutedOn,
                        Name = data.Name,
                        OccurrAt = data.OccurrAt,
                        Recurrence = data.Recurrence,
                        StartDate = data.StartDate,
                        StartTime = data.StartTime,
                        TaskType = data.TaskType
                    })
                    .Union(context.customerscheduledtasks
                .Where(x => (x.IsRunNow || DbFunctions.TruncateTime(x.StartDate) <= todayDate)
                    && x.IsEnabled).Select(data => new ScheduledTask
                    {
                        Days = data.Days,
                        Id = data.Id,
                        IsRunNow = data.IsRunNow,
                        LastExecutedOn = data.LastExecutedOn,
                        Name = data.Name,
                        OccurrAt = data.OccurrAt,
                        Recurrence = data.Recurrence,
                        StartDate = data.StartDate,
                        StartTime = data.StartTime,
                        TaskType = data.TaskType
                    }))
                .ToList();
            }

            return results;
        }

        public scheduledtask GetScheduledTask(int id)
        {
            using (var context = new EisInventoryContext())
            {
                return context.scheduledtasks.FirstOrDefault(x => x.Id == id);
            }
        }

        public customerscheduledtask GetCustomerScheduledTask(int id)
        {
            using (var context = new EisInventoryContext())
            {
                return context.customerscheduledtasks.FirstOrDefault(x => x.Id == id);
            }
        }

        public void UpdateScheduledTaskLastExecution(int id)
        {
            using (var context = new EisInventoryContext())
            {
                var task = context.scheduledtasks.FirstOrDefault(x => x.Id == id);
                if (task == null)
                    return;

                task.History = task.History + 1;
                task.IsRunNow = false;
                task.LastExecutedOn = DateTime.Now;

                // save the changes
                context.SaveChanges();
            }
        }

        public void UpdateCustomerScheduledTaskLastExecution(int id)
        {
            using (var context = new EisInventoryContext())
            {
                var task = context.customerscheduledtasks.FirstOrDefault(x => x.Id == id);
                if (task == null)
                    return;

                task.History = task.History + 1;
                task.IsRunNow = false;
                task.LastExecutedOn = DateTime.Now;

                // save the changes
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Get the list of all scheduled task tha are enabled
        /// </summary>
        /// <returns></returns>
        public List<ScheduleTaskImportFiles> GetCurrentScheduleAlreadyImportFiles(int scheduleTaskId)
        {
            using (var context = new EisInventoryContext())
            {
                var results = context.scheduletaskimportfiles
                  .Where(x => x.ScheduledTaskId == scheduleTaskId)
                  .ProjectTo<ScheduleTaskImportFiles>()
                  .ToList();

                return results;
            }

        }

        public void ManageImportFileSchedule(List<string> importedFiles, int scheduleId)
        {
            using (var context = new EisInventoryContext())
            {
                foreach (var fileItem in importedFiles)
                {
                    var importFileSchedule = new scheduletaskimportfile()
                    {
                        Created = DateTime.Now,
                        FileName = fileItem,
                        ScheduledTaskId = scheduleId
                    };
                    context.scheduletaskimportfiles.Add(importFileSchedule);
                    context.SaveChanges();

                }
            }
        }
    }
}