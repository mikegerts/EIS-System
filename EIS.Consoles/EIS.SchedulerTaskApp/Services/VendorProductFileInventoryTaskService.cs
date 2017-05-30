using System;
using System.Collections.Generic;
using System.IO;
using EIS.Inventory.Core.Services;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.SchedulerTaskApp.FileManagers;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Repositories;

namespace EIS.SchedulerTaskApp.Services
{
    public class VendorProductFileInventoryTaskService : TaskService
    {
        private string _ftpFileFullPath;
        private List<string> _updatedEisSupplierSKUs;
        private readonly vendorproductfileinventorytask _task;
        private readonly VendorProductService _service;
        private readonly FtpWebRequestor _ftpRequestor;

        public VendorProductFileInventoryTaskService(vendorproductfileinventorytask task)
        {
            _task = task;
            _service = new VendorProductService(new ImageHelper(new PersistenceHelper()), new LogService());
            _ftpRequestor = new FtpWebRequestor(task.FtpServer,
                                task.FtpUser,
                                task.FtpPassword,
                                task.FtpPort,
                                task.RemoteFolder);
            _ftpFileFullPath = task.FtpFileFullPath;
            _updatedEisSupplierSKUs = new List<string>();
        }

        public override string TaskType
        {
            get { return ScheduledTaskType.VENDOR_PRODUCT_FILE_INVENTORY; }
        }

        public override bool Execute()
        {
            _generatedFile = downloadFileFromFtp();

            // there should be an error if the downloaded file path is null and let's continue
            if (_generatedFile == null)
            {
                Logger.LogWarning(LogEntryType.FileInventoryTaskService, string.Format("The vendor product file for vendor ID {0} was not successfully downloaded! No file found at {1}!", _task.VendorId, _task.FtpFileFullPath));
                return false;
            }
            
            try
            {
                // get the vendor info first
                var vendor = _service.GetVendorById(_task.VendorId);

                // let's create file manager
                var fileManager = FileManagerFactory.CreateFileManager(new FileInfo(_generatedFile), _task, FileType.Csv);
                fileManager.ReadFile(_task.HasHeader ? 1 : 0);

                // read the first vendor product record from the file
                var vendorProduct = fileManager.GetNextVendorProduct();
                var totalRecords = fileManager.TotalRecords;

                while (vendorProduct != null)
                {
                    var percentage = (((double)fileManager.CurrentRowIndex) / totalRecords) * 100.00;
                    Console.WriteLine("{0:#0.00}% Processing vendor product \'{1}\' - {2}", percentage, _task.VendorId, vendorProduct.SupplierSKU);
                    
                    // get first the vendor product SKU
                    var eisSupplierSKU = _service.GetVendorProductSKU(vendorProduct);
                    vendorProduct.EisSupplierSKU = eisSupplierSKU;

                    // continue if it is not to create new item and its eisSupplierSKU is NULL or it has invalid data
                    if ((!_task.IsAddNewItem && string.IsNullOrEmpty(eisSupplierSKU)) || vendorProduct.HasInvalidData)
                    {
                        Console.WriteLine(string.Format("{1:#0.00}% No vendor product updated!", vendorProduct.EisSupplierSKU, percentage));

                        // read and parsed the next product from the file
                        vendorProduct = fileManager.GetNextVendorProduct();
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

                    // let's override its quantity if its vendor is configured as IsAlwasyInStock
                    if (vendor.IsAlwaysInStock && vendorProduct.IsQuantitySet)
                        vendorProduct.Quantity = vendor.AlwaysQuantity ?? 0;

                    // do insert or update vendor product 
                    vendorProduct.IsAutoLinkToEisSKU = _task.IsCreateEisSKUAndLink || _task.IsLinkEisSKU;
                    vendorProduct.IsAutoLinkToEisSKUSet = _task.IsCreateEisSKUAndLink || _task.IsLinkEisSKU;
                    _service.DoUpadateOrInsertVendorProduct(vendorProduct, isToUpdate, Constants.APP_NAME);
                    _updatedEisSupplierSKUs.Add(vendorProduct.EisSupplierSKU);

                    // check first if we want to auto-link and create new EIS product if it doesn't exist
                    if (_task.IsCreateEisSKUAndLink && !string.IsNullOrEmpty(vendorProduct.UPC))
                        _service.AddLinkAndCreateEisProductIfNoMatchWithUPC(vendorProduct);

                    // check if there's a need to auto-link this vendor product with EIS product
                    if (_task.IsLinkEisSKU && !_task.IsCreateEisSKUAndLink && !string.IsNullOrEmpty(vendorProduct.UPC))
                        _service.UpdateEisProductLinks(vendorProduct.EisSupplierSKU, vendorProduct.UPC, vendorProduct.MinPack);

                    // read and parsed the next product from the file
                    vendorProduct = fileManager.GetNextVendorProduct();
                }

                Logger.LogInfo(LogEntryType.FileInventoryTaskService, "Successfully parsed the vendor product inventory file. Total records: " + fileManager.TotalRecords);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FileInventoryTaskService,
                    "Error in parsing the downloaded vendor product inventory file! Message: " + EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return false;
            }
        }

        public override void DoPostExecution()
        {
            if (_task.IsZeroOutQty)
                _service.ZeroOutVendorProductQuantity(_task.VendorId, _updatedEisSupplierSKUs, Constants.APP_NAME);

            if (_task.IsDeleteFile)
                _ftpRequestor.DeleteFileFromFtpAsync(_ftpFileFullPath);

            // delete the downloaded file
            try { File.Delete(_generatedFile); }
            catch (Exception) { }
        }

        private string downloadFileFromFtp()
        {
            var formattedFileName = _task.FormattedFileName;
            if (_task.FileName.Contains("*"))
            {
                _ftpFileFullPath = _ftpRequestor.FindFileFromFtp(string.Format("{0}{1}", _task.FileName, _task.FileType));
                formattedFileName = Path.GetFileName(_ftpFileFullPath);
            }

            // download the file from the FTP and get the downloaded file path
            return _ftpRequestor.DownloadFtpFile(_ftpFileFullPath, formattedFileName);
        }
    }
}
