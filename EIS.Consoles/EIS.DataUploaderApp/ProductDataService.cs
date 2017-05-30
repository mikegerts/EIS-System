using System;
using System.Linq;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Repositories;

namespace EIS.DataUploaderApp
{
    public class ProductDataService
    {
        private readonly ProductRepository _productRepo;
        private readonly FileResult _fileResult;

        public ProductDataService(FileResult fileResult, string connectionString)
        {
            _fileResult = fileResult;
            _productRepo = new ProductRepository(connectionString, true);
        }

        public void UpdateProducts()
        {
            // get the new upload products from the file
            var vendorProducts = _productRepo.GetVendorProducts(_fileResult.OwnerId, _fileResult.ResultDate);

            // let's get the start EIS SKU code for this vendor's products
            var startSkuCode = _productRepo.GetVendorStartEisSkuCode(_fileResult.OwnerId);
            var totalProducts = vendorProducts.Count();
            var counter = 0;

            foreach (var p in vendorProducts)
            {
                var percentage = (((double)counter++) / totalProducts) * 100.00;
                Console.WriteLine("{0:#0.00}% Product File Serive: processing \'{1}\' - {2}", percentage, p.VendorId, p.SKU);

                if (_productRepo.IsProductExist(p.VendorId, p.SKU))
                {
                    // if exist, update the product details
                    _productRepo.UpdateMasterProduct(p);
                }
                else
                {
                    // otherwise, create new inventory product
                    _productRepo.CreateMasterProduct(p, startSkuCode);
                }
            }

            // close the db connection
            _productRepo.CloseDbConnection();
        }
    }
}
