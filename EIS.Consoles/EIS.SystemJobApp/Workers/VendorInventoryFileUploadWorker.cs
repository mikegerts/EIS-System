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
    public class VendorInventoryFileUploadWorker : JobWorker
    {
        private readonly VendorProductService _service;
        private readonly Dictionary<string, string> supportiveParameters;

        public VendorInventoryFileUploadWorker(SystemJob job)
            : base(job)
        {
            _service = new VendorProductService(new ImageHelper(new PersistenceHelper()), new LogService());
            string[] supportiveParameterArray = job.SupportiveParameters.Split('&');

            supportiveParameters = new Dictionary<string, string>();

            foreach (var item in supportiveParameterArray)
            {
                string[] arrayParamValues = item.Split('=');
                supportiveParameters.Add(arrayParamValues[0], arrayParamValues[1]);
            }
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
            var vendorProducts = new List<VendorProduct>();
            var message = CsvFileDataParser.ParseVendorInventoryFile(_systemJob.Parameters, vendorProducts, _systemJob.HasHeader);

            try
            {
                if (vendorProducts.Count == 0)
                {
                    Console.WriteLine("0 records to update vendor inventory products");
                    _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Completed);
                    return;
                }
                int selectedVendorId = Convert.ToInt32(supportiveParameters["selectedVendor"]);
                if (Convert.ToBoolean(supportiveParameters["inventoryZero"]))
                {
                    var vendorProductsList = _service.GetProductsByVendorId(selectedVendorId).Take(5).ToList();

                    var totalItems = vendorProductsList.Count;
                    setTotalItemsProcessed(totalItems);
                    _logger.LogInfo(LogEntryType.VendorProductFileUploadWorker, string.Format("Uploading {0} vendor inventory products initiated by {1}", totalItems, _systemJob.SubmittedBy));


                    var vendorProductsNotInList = vendorProductsList.Where(x => !vendorProducts.Any(y => y.VendorId == selectedVendorId && y.EisSupplierSKU == x.EisSupplierSKU)).
                        Select(x => new VendorProduct
                        {
                            Category = x.Category,
                            Description = x.Description,
                            EisSupplierSKU = x.EisSupplierSKU,
                            Quantity = 0,
                            VendorId = x.VendorId
                        }).ToList();


                    foreach (var vendorProduct in vendorProducts)
                    {

                        totalProcessed++;
                        var percentage = (((double)totalProcessed) / totalItems) * 100.00;

                        affectedRows += _service.DoUpdateVendorInventoryProduct(vendorProduct, selectedVendorId, _systemJob.SubmittedBy);

                        _bw.ReportProgress(affectedRows);
                        Console.WriteLine(string.Format("{1:#0.00}% Updating vendor products EisSupplierSKU: {0}", vendorProduct.EisSupplierSKU, percentage));
                    }

                    foreach (var vendorProduct in vendorProductsNotInList)
                    {
                        totalProcessed++;
                        var percentage = (((double)totalProcessed) / totalItems) * 100.00;

                        affectedRows += _service.DoUpdateVendorInventoryProduct(vendorProduct, selectedVendorId, _systemJob.SubmittedBy);

                        _bw.ReportProgress(affectedRows);
                        Console.WriteLine(string.Format("{1:#0.00}% Updating vendor products EisSupplierSKU: {0}", vendorProduct.EisSupplierSKU, percentage));
                    }

                }
                else
                {

                    var totalItems = vendorProducts.Count;
                    setTotalItemsProcessed(totalItems);
                    _logger.LogInfo(LogEntryType.VendorProductFileUploadWorker, string.Format("Uploading {0} vendor inventory products initiated by {1}", totalItems, _systemJob.SubmittedBy));

                    foreach (var vendorProduct in vendorProducts)
                    {
                        totalProcessed++;
                        var percentage = (((double)totalProcessed) / totalItems) * 100.00;

                        affectedRows += _service.DoUpdateVendorInventoryProduct(vendorProduct, selectedVendorId, _systemJob.SubmittedBy);

                        _bw.ReportProgress(affectedRows);
                        Console.WriteLine(string.Format("{1:#0.00}% Updating vendor inventory products EisSupplierSKU: {0}", vendorProduct.EisSupplierSKU, percentage));
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker, EisHelper.ParseDbEntityValidationException(ex), ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }
            catch (Exception ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker,
                    string.Format("Error in uploading vendor inventory products. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }

            _logger.LogInfo(LogEntryType.VendorProductFileUploadWorker, string.Format("{0} vendor inventory products have been successfully uploaded by {1}", totalProcessed, _systemJob.SubmittedBy));

        }

        protected override void DoPostWorkCompleted()
        {

        }
    }
}