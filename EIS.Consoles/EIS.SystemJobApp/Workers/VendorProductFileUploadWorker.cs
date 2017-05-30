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
    public class VendorProductFileUploadWorker : JobWorker
    {
        private readonly VendorProductRepository _repository;
        private readonly VendorProductService _service;
        private readonly VendorProductUploadResult _uploadResult;

        public VendorProductFileUploadWorker(SystemJob job) : base(job)
        {
            _repository = new VendorProductRepository(_logger);
            _service = new VendorProductService(new ImageHelper(new PersistenceHelper()), new LogService());
            _uploadResult = new VendorProductUploadResult();
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
            var message = CsvFileDataParser.ParseVendorProductFile(_systemJob.Parameters, vendorProducts, _systemJob.HasHeader);

            // get the total items and update the system job status
            var totalItems = vendorProducts.Count;
            setTotalItemsProcessed(totalItems);
            _logger.LogInfo(LogEntryType.VendorProductFileUploadWorker, string.Format("Uploading {0} products initiated by {1}", totalItems, _systemJob.SubmittedBy));

            try
            {
                // iterate and do insert or update vendor product data
                foreach (var vendorProduct in vendorProducts)
                {
                    totalProcessed++;
                    var percentage = (((double)totalProcessed) / totalItems) * 100.00;

                    // get first the vendor product SKU
                    var eisSupplierSKU = _service.GetVendorProductSKU(vendorProduct);
                    vendorProduct.EisSupplierSKU = eisSupplierSKU;

                    // continue if it is not to create new item and its eisSupplierSKU is NULL
                    if (!_systemJob.IsAddNewItem && string.IsNullOrEmpty(eisSupplierSKU))
                    {
                        Console.WriteLine(string.Format("{1:#0.00}% No vendor product updated!", vendorProduct.EisSupplierSKU, percentage));
                        continue;
                    }

                    var isToUpdate = true;
                    if (string.IsNullOrEmpty(eisSupplierSKU))
                    {
                        // get the start SKU code for this vendor
                        var startSkuCode = _service.GetVendorStartSku(vendorProduct.VendorId);
                        vendorProduct.EisSupplierSKU = string.Format("{0}{1}", startSkuCode, vendorProduct.SupplierSKU.Trim());
                        isToUpdate = false;
                    }

                    // do insert or update vendor product 
                    affectedRows += _service.DoUpadateOrInsertVendorProduct(vendorProduct, isToUpdate, _systemJob.SubmittedBy);
                    var uploadResultType = UploadResultType.NoChanges;
                    var newEisSKU = string.Empty;

                    // the IsAutoLinkToEisSKU field from the vendor product is the highest precedendens over on the HasPostAction_2 and HasPostAction_1
                    if (vendorProduct.IsAutoLinkToEisSKUSet)
                    {
                        if (vendorProduct.IsAutoLinkToEisSKU)
                        {
                            // update the current link of this vendor product
                            uploadResultType = _service.UpdateEisProductLinks(vendorProduct.EisSupplierSKU, vendorProduct.UPC, vendorProduct.MinPack);
                        }
                        else
                        {
                            // delete the existing product links if there's any
                            _service.DeleteOldVendorProductLinks(vendorProduct.EisSupplierSKU, new List<string>());
                            uploadResultType = UploadResultType.DeletedLink;
                        }
                    }
                    else
                    {
                        // check first if we want to auto-link and create new EIS product if it doesn't exist
                        if (_systemJob.HasPostAction_2 && !string.IsNullOrEmpty(vendorProduct.UPC))
                        {
                            newEisSKU = _service.AddLinkAndCreateEisProductIfNoMatchWithUPC(vendorProduct);
                            uploadResultType = string.IsNullOrEmpty(newEisSKU) ? UploadResultType.UpdatedItem : UploadResultType.NewItemCreated;
                        }

                        // check if there's a need to auto-link this vendor product with EIS product
                        if (_systemJob.HasPostAction_1 && !_systemJob.HasPostAction_2 && !string.IsNullOrEmpty(vendorProduct.UPC))
                            uploadResultType = _service.UpdateEisProductLinks(vendorProduct.EisSupplierSKU, vendorProduct.UPC, vendorProduct.MinPack);

                    }

                    // add the EisSupplierSKU for the upload result tracking
                    addUploadFileResult(vendorProduct.EisSupplierSKU, newEisSKU, uploadResultType);

                    _bw.ReportProgress(affectedRows);
                    Console.WriteLine(string.Format("{1:#0.00}% Updating vendor products EisSupplierSKU: {0}", vendorProduct.EisSupplierSKU, percentage));
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
                    string.Format("Error in uploading vendor products. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }

            _logger.LogInfo(LogEntryType.VendorProductFileUploadWorker, string.Format("{0} vendor products have been successfully uploaded by {1}", totalProcessed, _systemJob.SubmittedBy));
        }

        protected override void DoPostWorkCompleted()
        {
            // write the upload file result into CSV file

            // create the file to save the product categories
            var resultFilePath = string.Format("{0}\\VendorProductUploadResults_{1}.csv", _resultFileDirecctory, _systemJob.Id);
            
            // write into the file
            using (var streamWriter = new StreamWriter(resultFilePath))
            {
                var writer = new CsvHelper.CsvWriter(streamWriter);

                // write first the list of vendor SKUs that have updated its links
                writer.WriteField(string.Format("EisSupplierSKUs successfully updated its EIS product links: {0} items", _uploadResult.UpdatedEisSupplierSKUs.Count));
                writer.NextRecord();
                if (_uploadResult.UpdatedEisSupplierSKUs.Any())
                {
                    writer.WriteField("EisSupplierSKU");
                    writer.NextRecord();
                    foreach (var item in _uploadResult.UpdatedEisSupplierSKUs)
                    {
                        writer.WriteField(item);
                        writer.NextRecord();
                    }
                }
                writer.NextRecord();

                // then, the EisSupplierSKUs and newly created EIS products
                writer.WriteField(string.Format("EisSupplierSKUs successfully created new EIS product and its links: {0} items", _uploadResult.NewCreatedEisSKUs.Count));
                writer.NextRecord();
                if (_uploadResult.NewCreatedEisSKUs.Any())
                {
                    writer.WriteField("EisSupplierSKU");
                    writer.WriteField("EisSKU");
                    writer.NextRecord();
                    foreach (var item in _uploadResult.NewCreatedEisSKUs)
                    {
                        writer.WriteField(item.Key);
                        writer.WriteField(item.Value);
                        writer.NextRecord();
                    }
                }
                writer.NextRecord();

                // then, the EisSupplierSKUs and newly created EIS products
                writer.WriteField(string.Format("EisSupplierSKUs successfully deleted its EIS product links: {0} items", _uploadResult.DeletedEisSupplierSKULinks.Count));
                writer.NextRecord();
                if (_uploadResult.DeletedEisSupplierSKULinks.Any())
                {
                    writer.WriteField("EisSupplierSKU");
                    writer.NextRecord();
                    foreach (var sku in _uploadResult.DeletedEisSupplierSKULinks)
                    {
                        writer.WriteField(sku);
                        writer.NextRecord();
                    }
                }
                writer.NextRecord();

                // lastly, the EisSupplierSKUs with no changes at all, it might be its UPC code is NULL
                writer.WriteField(string.Format("EisSupplierSKUs with no EIS products and no links created or changed: {0} items", _uploadResult.NoChangedEisSupplierSKUs.Count));
                writer.NextRecord();
                if (_uploadResult.NoChangedEisSupplierSKUs.Any())
                {
                    writer.WriteField("EisSupplierSKU");
                    writer.NextRecord();
                    foreach (var item in _uploadResult.NoChangedEisSupplierSKUs)
                    {
                        writer.WriteField(item);
                        writer.NextRecord();
                    }
                }
                writer.NextRecord();
            }

            // update the system job parameters out for the file path of the result file
            _jobRepository.UpdateSystemJobParametersOut(_systemJob.Id, resultFilePath);
        }

        private void addUploadFileResult(string eisSupplierSKU, string newEisSKU, UploadResultType resultType)
        {
            if (resultType == UploadResultType.NewItemCreated)
                _uploadResult.NewCreatedEisSKUs.Add(eisSupplierSKU, newEisSKU);
            else if (resultType == UploadResultType.UpdatedItem)
                _uploadResult.UpdatedEisSupplierSKUs.Add(eisSupplierSKU);
            else if (resultType == UploadResultType.DeletedLink)
                _uploadResult.DeletedEisSupplierSKULinks.Add(eisSupplierSKU);
            else
                _uploadResult.NoChangedEisSupplierSKUs.Add(eisSupplierSKU);
        }
    }
}
