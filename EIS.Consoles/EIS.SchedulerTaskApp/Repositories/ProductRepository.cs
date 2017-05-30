using System;
using System.Collections.Generic;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class ProductRepository
    {
        public void CreateVendorProduct(VendorProduct vendorProduct)
        {
            using (var context = new EisInventoryContext())
            {
                context.inventoryvendorproducts.Add(new inventoryvendorproduct
                {
                    SupplierSKU = vendorProduct.SupplierSKU,
                    VendorId = vendorProduct.VendorId,
                    Name = vendorProduct.Name,
                    Description = vendorProduct.Description,
                    Category = vendorProduct.Category,
                    UPC = vendorProduct.UPC,
                    Quantity = vendorProduct.Quantity,
                    SupplierPrice = vendorProduct.SupplierPrice,
                    MinPack = vendorProduct.MinPack,
                    ResultDate = DateTime.Now.Date
                });

                context.SaveChanges();
            }
        }

        public List<inventoryvendorproduct> GetInventoryVendorProducts(long? vendorId, DateTime resultDate)
        {
            var products = new List<inventoryvendorproduct>();
            using (var context = new EisInventoryContext())
            {
                products = context.inventoryvendorproducts
                    .Where(x => x.ResultDate == resultDate && x.VendorId == vendorId)
                    .ToList();
            }

            return products;
        }

        public void DeleteInventoryVendorProductsByVendor(long? vendorId, DateTime resultDate)
        {
            using (var context = new EisInventoryContext())
            {
                var vendorProducts = context.inventoryvendorproducts.Where(x => x.VendorId == vendorId);
                if (!vendorProducts.Any())
                    return;

                context.inventoryvendorproducts.RemoveRange(vendorProducts);
                context.SaveChanges();
            }
        }
        
        public void ManageVendorProductInventoryUpdate(inventoryvendorproduct inventoryProduct)
        {
            using(var context = new EisInventoryContext())
            {
                // get the information
                var vendorProduct = context.vendorproducts
                    .FirstOrDefault(x => x.SupplierSKU == inventoryProduct.SupplierSKU && x.VendorId == inventoryProduct.VendorId);
                if (vendorProduct == null)
                    return;

                if (inventoryProduct.Quantity != -1)
                    vendorProduct.Quantity = (int)inventoryProduct.Quantity;

                if (inventoryProduct.SupplierPrice != -1)
                    vendorProduct.SupplierPrice = (decimal)inventoryProduct.SupplierPrice;

                if (!string.IsNullOrEmpty(inventoryProduct.Name))
                    vendorProduct.Name = inventoryProduct.Name;

                if (!string.IsNullOrEmpty(inventoryProduct.Description))
                    vendorProduct.Description = inventoryProduct.Description;
                
                if (!string.IsNullOrEmpty(inventoryProduct.Category))
                    vendorProduct.Category = inventoryProduct.Category;

                if (!string.IsNullOrEmpty(inventoryProduct.UPC))
                    vendorProduct.UPC = inventoryProduct.UPC;

                if (inventoryProduct.MinPack != -1)
                    vendorProduct.MinPack = (int)inventoryProduct.MinPack;

                vendorProduct.Modified = DateTime.UtcNow;
                vendorProduct.ModifiedBy = Constants.APP_NAME;

                // save the changes
                context.SaveChanges();
            }
        }
    }
}
