using System;
using System.Configuration;
using System.IO;
using System.Linq;
using EIS.Inventory.DAL.Database;

namespace EIS.SchedulerTaskApp.Services
{
    public abstract class TaskService
    {
        protected readonly string _exportedFileFolder;
        protected readonly string _connectionString;
        protected string _generatedFile;

        public TaskService()
        {
            _exportedFileFolder = ConfigurationManager.AppSettings["ExportedFilesRoot"].ToString();
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        /// <summary>
        /// Gets the type of the task
        /// </summary>
        public abstract string TaskType { get; }

        /// <summary>
        /// Exexute the task service for the task
        /// </summary>
        public abstract bool Execute();

        /// <summary>
        /// Execute any cleanups or updates after executing the task
        /// </summary>
        public abstract void DoPostExecution();

        protected void createExportedFilesRecord(int taskId)
        {
            if (string.IsNullOrEmpty(_generatedFile))
                return;

            var fileInfo = new FileInfo(_generatedFile);
            using (var context = new EisInventoryContext())
            {
                var exportedFile = new exportedfile
                {
                    ScheduledTaskId = taskId,
                    FileName = Path.GetFileName(_generatedFile),
                    FileSize = fileInfo.Length,
                    Created = DateTime.UtcNow
                };

                context.exportedfiles.Add(exportedFile);
                context.SaveChanges();
            }
        }

        protected void createCustomerExportedFilesRecord(int taskId)
        {
            if (string.IsNullOrEmpty(_generatedFile))
                return;

            var fileInfo = new FileInfo(_generatedFile);
            using (var context = new EisInventoryContext())
            {
                var exportedFile = new customerexportedfile
                {
                    ScheduledTaskId = taskId,
                    FileName = Path.GetFileName(_generatedFile),
                    FileSize = fileInfo.Length,
                    Created = DateTime.UtcNow
                };

                context.customerexportedfiles.Add(exportedFile);
                context.SaveChanges();
            }
        }

        protected messagetemplate getMessageTemplate(string taskType)
        {
            messagetemplate template = null;

            using (var context = new EisInventoryContext())
            {
                template = context.messagetemplates
                    .FirstOrDefault(x => x.MessageType.ToString() == taskType && x.IsEnabled);
            }

            return template;
        }
    }
}
