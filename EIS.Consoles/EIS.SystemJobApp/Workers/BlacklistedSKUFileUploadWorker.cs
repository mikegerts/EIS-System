using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Workers
{

    public class BlacklistedSKUFileUploadWorker : JobWorker
    {
        private readonly ProductRepository _repository;

        public BlacklistedSKUFileUploadWorker(SystemJob job)
            : base(job)
        {
            _repository = new ProductRepository(_logger);
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;

            // parsed and get the blacklisted skus info from the file
            var blackListedProducts = new List<BlacklistedSkuDto>();
            var message = CsvFileDataParser.ParsedSKUFile(_systemJob.Parameters, blackListedProducts, _systemJob.HasHeader);
            var affectedRows = 0;

            // log the total records to be processed
            var totalRecords = blackListedProducts.Count;
            setTotalItemsProcessed(totalRecords);
            _logger.LogInfo(LogEntryType.BlacklistedSKUFileUploadWorker, string.Format("Uploading {0} blacklisted sku initiated by {1}", totalRecords, _systemJob.SubmittedBy));

            // let's do the update the products first which is IsBlacklisted is TRUE
            var blacklistedProducts = blackListedProducts
                .Where(x => x.IsBlacklisted)
                .Select(x => x.EisSKU)
                .ToList();

            // update it first 
            affectedRows += _repository.UpdateProductsBlacklistedStatus(blacklistedProducts, true, _systemJob.SubmittedBy);
            reportProgress(affectedRows, totalRecords);

            // then the unblacklisted products
            var unBlacklistedProducts = blackListedProducts
                .Where(x => !x.IsBlacklisted)
                .Select(x => x.EisSKU)
                .ToList();

            // update it
            affectedRows += _repository.UpdateProductsBlacklistedStatus(unBlacklistedProducts, false, _systemJob.SubmittedBy);
            reportProgress(affectedRows, totalRecords);
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }

        private void reportProgress(int affectedRows, int totalRecords)
        {
            var percentage = (((double)affectedRows) / totalRecords) * 100.00;
            _bw.ReportProgress(affectedRows);
            Console.WriteLine(string.Format("{1:#0.00}% Done updating for UNblacklisting products: {0}", affectedRows, percentage));
        }
    }
}
