using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Core.Services;


namespace EIS.SystemJobApp.Workers
{
    public class ShippingRateFileUploadWorker : JobWorker
    {
        private readonly ShippingRateRepository _repository;
        private readonly ShippingRateService _service;
        //private readonly VendorProductUploadResult _uploadResult;

        public ShippingRateFileUploadWorker(SystemJob job) : base(job)
        {
            _repository = new ShippingRateRepository(_logger);
            _service = new ShippingRateService();
            //_uploadResult = new VendorProductUploadResult();
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;
            var totalProcessed = 0;
            var affectedRows = 0;

            // parsed the vendor product data from the file
            var shippingRates = new List<ShippingRateDB>();
            var message = CsvFileDataParser.ParseShippingRateFile(_systemJob.Parameters, shippingRates, _systemJob.HasHeader);

            // get the total items and update the system job status
            var totalItems = shippingRates.Count;
            setTotalItemsProcessed(totalItems);
            _logger.LogInfo(LogEntryType.ShippingRateFileUploadWorker, string.Format("Uploading {0} shipping rates initiated by {1}", totalItems, _systemJob.SubmittedBy));

            foreach (var item in shippingRates)
            {
                totalProcessed++;
                var percentage = (((double)totalProcessed) / totalItems) * 100.00;

                affectedRows += _repository.DoUpadateOrInsertShippingRate(item, _systemJob.IsAddNewItem);

                _bw.ReportProgress(affectedRows);
                Console.WriteLine(string.Format("{1:#0.00}% Updating/Inserting shipping rate ID: {0}", item.Id, percentage));
            }
        }

        protected override void DoPostWorkCompleted()
        {

        }
    }
}