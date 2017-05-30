using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Core.Models;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public interface IScheduledTaskService : IDisposable
    {
        /// <summary>
        /// Get the list of all scheduled task
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScheduledTaskListDto> GetAllScheduledTasks();

        /// <summary>
        /// Get the scheduled task with the specified id
        /// </summary>
        /// <param name="id">The scheduled task ID</param>
        /// <returns></returns>
        ScheduledTaskDto GetScheduledTask(int id);

        /// <summary>
        /// Create new scheduled task with the specified data
        /// </summary>
        /// <param name="taskModel">The task to insert to database</param>
        /// <returns></returns>
        bool CreateScheduledTask(ScheduledTaskDto taskModel);

        /// <summary>
        /// Update the scheduled task with the specified task id and updated task data
        /// </summary>
        /// <param name="id">The id of the scheduled task to update</param>
        /// <param name="taskModel">The updated scheduled task</param>
        /// <returns></returns>
        bool UpdateScheduledTask(int id, ScheduledTaskDto taskModel);

        /// <summary>
        /// Delete the scheduled task with the specified id
        /// </summary>
        /// <param name="id">The id of the scheduled task</param>
        bool DeleteScheduledTask(int id);

        /// <summary>
        /// Get the list of exported files of the specified scheduled task id
        /// </summary>
        /// <param name="taskId">The id of the scheduled task</param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<ExportedFileDto> GetPagedExportedFiles(int taskId, int page, int pageSize);

        /// <summary>
        /// Set the flag to the schedule task to execute right away
        /// </summary>
        /// <param name="taskId"></param>
        void SetTaskToExecuteNow(int taskId);
    }
}
