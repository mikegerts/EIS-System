using System;
using System.Data.Entity.Validation;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.SystemJobApp.Repositories
{
    public class VendorProductRepository
    {
        protected readonly LoggerRepository _logger;
        private readonly ProductRepository _productRepository;

        public VendorProductRepository(LoggerRepository logger)
        {
            _logger = logger;
            _productRepository = new ProductRepository(_logger);
        }

        public int DeleteVendorProduct(string eisSupplierSKU)
        {
            var retValue = 0;
            using(var context = new EisInventoryContext())
            {
                using(var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var vendorProduct = context.vendorproducts
                            .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU);
                        if (vendorProduct == null)
                            return 1;

                        // remove the product link first
                        context.vendorproductlinks.RemoveRange(vendorProduct.vendorproductlinks.ToList());

                        // lastly, the vendor product
                        context.vendorproducts.Remove(vendorProduct);

                        transaction.Commit();
                        context.SaveChanges();
                        retValue = 1;
                    }
                    catch
                    {
                        transaction.Rollback();
                        retValue = 0;
                    }
                }
            }

            return retValue;
        }
    }
}
