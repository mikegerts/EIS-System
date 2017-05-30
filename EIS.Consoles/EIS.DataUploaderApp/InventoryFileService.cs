using System;
using System.Linq;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Repositories;

namespace EIS.DataUploaderApp
{
    public class InventoryFileService
    {
        private readonly ProductRepository _productRepo;
        private readonly FileResult _fileResult;

        public InventoryFileService(FileResult fileResult, string connectionString)
        {
            _fileResult = fileResult;
            _productRepo = new ProductRepository(connectionString, true);
        }

        public void UpdateProducts()
        {
            // get the new upload products from the file
            var vendorProducts = _productRepo.GetVendorProducts(_fileResult.OwnerId, _fileResult.ResultDate);
            var totalProducts = vendorProducts.Count();
            var counter = 0;

            foreach (var p in vendorProducts)
            {
                var percentage = (((double)counter++) / totalProducts) * 100.00;
                Console.WriteLine("{0:#0.00}% Inventory File Serive: processing \'{1}\' - {2}", percentage, p.VendorId, p.SKU);

                if (_productRepo.IsProductExist(p.VendorId, p.SKU))
                {
                    // if exist, update some product details
                    _productRepo.UpdateMasterProductFromInventory(p);
                }
                else
                {
                    // otherwise, add new product to the new products table
                    _productRepo.CreateUnMappedProduct(p);
                }
            }

            Logger.LogInfo(this.GetType().Name, totalProducts + " has been successfully update from inventory file.");

            // close the db connection
            _productRepo.CloseDbConnection();
        }
    }
}
