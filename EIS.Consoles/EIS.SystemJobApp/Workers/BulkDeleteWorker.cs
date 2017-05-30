using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CsvHelper;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Managers;
using EIS.Inventory.Shared.Helpers;

namespace EIS.SystemJobApp.Workers
{
    public class BulkDeleteWorker: JobWorker
    {
        private readonly IBulkDeleteManager _deleteManager;

        public BulkDeleteWorker(SystemJob job)
            : base(job)
        {
            if (job.JobType == JobType.BulkDeleteProduct)
                _deleteManager = new ProductDeleteManager(_logger);
            else if (job.JobType == JobType.BulkDeleteVendorProduct)
                _deleteManager = new VendorProductDeleteManager(_logger);
            else
                throw new ArgumentException("Invalid job type for delete worker: " + job.JobType);               
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;
            var modelIDs = getALineOfStringFromFile(_systemJob.Parameters);

            // set the total items to be proccessed
            setTotalItemsProcessed(modelIDs.Count);
            _logger.LogInfo(LogEntryType.BulkDeleteWorker, string.Format(" for \'{0}\' items is started...", modelIDs.Count));

            try
            {
                var affectedRows = 0;
                var totalProcessed = 0;

                // iterate to each and delete its data
                foreach (var modelId in modelIDs)
                {
                    totalProcessed++;
                    var percentage = (((double)totalProcessed) / modelIDs.Count) * 100.00;

                    // do delete the product
                    affectedRows += _deleteManager.DeleteItem(modelId);

                    // report the progress
                    _bw.ReportProgress(affectedRows);
                    Console.WriteLine(string.Format("{1:#0.00}% Deleting model ID for {2}: {0}", modelId, percentage, _systemJob.JobType));
                }

                // log info that the Bulk Delete is complete
                _logger.LogInfo(LogEntryType.BulkDeleteWorker, string.Format("{0}/{1} has been successfully deleted.",
                    affectedRows, modelIDs.Count));
            }
            catch (Exception ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.BulkDeleteWorker, "Error in bulk products -> " + EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }

        private List<string> getALineOfStringFromFile(string filePath)
        {
            var records = new List<string>();
            using (var streamReader = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(streamReader);
                csvReader.Configuration.HasHeaderRecord = false;
                while (csvReader.Read())
                    records.Add(csvReader.GetField(0));
            }
            return records;
        }
    }
}
