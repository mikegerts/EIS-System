using System;
using System.Collections.Generic;
using System.Linq;
using EIS.SchedulerTaskApp.Repositories;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.FileManagers
{
    public class DataFileParser
    {
        private readonly IFileManager _fileManager;
        private readonly vendorproductfileinventorytask _task;
        private readonly ProductRepository _productRepo;
        private List<string> _updatedVendorSKUs;

        public DataFileParser(IFileManager fileManager, vendorproductfileinventorytask task)
        {
            _fileManager = fileManager;
            _task = task;
            _updatedVendorSKUs = new List<string>();
            _productRepo = new ProductRepository();
        }

        public object ParsedDataFile()
        {
            var hasError = false;
            try
            {
                _fileManager.ReadFile(_task.HasHeader ? 1 : 0);
                var product = _fileManager.GetNextVendorProduct();
                var totalRecords = _fileManager.TotalRecords;

                while (product != null)
                {
                    var percentage = (((double)_fileManager.CurrentRowIndex) / totalRecords) * 100.00;
                    Console.WriteLine("{0:#0.00}% Processing product for vendor \'{1}\' - {2}", percentage, _task.VendorId, product.SupplierSKU);

                    // insert the product into the database
                    _productRepo.CreateVendorProduct(product);

                    // read and parsed the next product from the file
                    product = _fileManager.GetNextVendorProduct();
                }

                Logger.LogInfo(LogEntryType.FileInventoryTaskService, "Successfully parsed the vendor product inventory file. Total records: " + _fileManager.TotalRecords);
            }
            catch (Exception ex)
            {
                hasError = true;
                Logger.LogError(LogEntryType.FileInventoryTaskService, "Error in parsing the downloaded vendor product inventory file! Message: " + ex.Message, ex.StackTrace);
            }
            finally
            {
                // close the file and db connection
                _fileManager.DeleteDownloadedFile();
            }

            return hasError;
        }

        public void DeleteInventoryProducts()
        {
            _productRepo.DeleteInventoryVendorProductsByVendor(_task.VendorId, DateTime.UtcNow.Date);
        }

        public void DoUpdateEisProducts()
        {
            // get the new upload products from the file
            var inventoryProducts = _productRepo.GetInventoryVendorProducts(_task.VendorId, DateTime.UtcNow.Date);
            var totalProducts = inventoryProducts.Count();
            var counter = 0;

            try
            {
                foreach (var inventoryProduct in inventoryProducts)
                {
                    var percentage = (((double)counter++) / totalProducts) * 100.00;
                    Console.WriteLine("{0:#0.00}% Inventory File Serive: processing \'{1}\' - {2}",
                        percentage, inventoryProduct.VendorId, inventoryProduct.SupplierSKU);

                    // update the product information
                    _productRepo.ManageVendorProductInventoryUpdate(inventoryProduct);

                    // add the vendor's sku to the list
                    _updatedVendorSKUs.Add(inventoryProduct.SupplierSKU);
                }
                Logger.LogInfo(LogEntryType.FileInventoryTaskService, totalProducts + " has been successfully update from inventory file.");
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.FileInventoryTaskService, "Error in updating EIS products! Message: " + ex.Message, ex.StackTrace);
            }
        }
    }
}
