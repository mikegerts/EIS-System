using System;
using System.Collections.Generic;
using System.Linq;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;

namespace EIS.Inventory.Core.Services
{
    public class VendorProductLinkService : IVendorProductLinkService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public VendorProductLinkService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }

        public IEnumerable<ProductLinkDto> GetEisProductLinks(string eisSupplierSKU)
        {
            return _context.vendorproductlinks
                .Where(x => x.EisSupplierSKU == eisSupplierSKU && x.IsActive)
                .ToList()
                .Select(x => new ProductLinkDto
                {
                    EisSKU = x.EisSKU,
                    EisSupplierSKU = x.EisSupplierSKU,
                    Name = x.product.Name,
                    SellingPrice = x.product.SellerPrice,
                    SkuType = x.product.SkuType.ToString()
                });
        }

        public bool AddEisProductLinks(string eisSupplierSKU, List<string> selectedEisSKUs)
        {
            // iterate to each EIS SKUs and add them
            foreach (var eisSKU in selectedEisSKUs)
            {
                // let's check first if the selected EIS SKU exist
                var existingProductLink = _context.vendorproductlinks
                    .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU && x.EisSKU == eisSKU);
                if (existingProductLink != null)
                {
                    existingProductLink.IsActive = true;
                }
                else
                {
                    // add the product link
                    _context.vendorproductlinks.Add(new vendorproductlink
                    {
                        EisSKU = eisSKU,
                        EisSupplierSKU = eisSupplierSKU,
                        Created = DateTime.UtcNow
                    });
                }
            }

            // save all the changes
            _context.SaveChanges();

            return true;
        }

        public bool DeleteProductLink(string eisSKU, string eisSupplierSKU)
        {
            var productLink = _context.vendorproductlinks
                .FirstOrDefault(x => x.EisSKU == eisSKU && x.EisSupplierSKU == eisSupplierSKU);
            if (productLink == null)
                return true;

            _context.vendorproductlinks.Remove(productLink);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<VendorProductLinkDto> GetVendorProductLinks(string eisSKU)
        {
            return _context.vendorproductlinks
                .Where(x => x.EisSKU == eisSKU && x.IsActive)
                .ToList()
                .Select(x => new VendorProductLinkDto
                {
                    EisSKU = x.EisSKU,
                    EisSupplierSKU = x.EisSupplierSKU,
                    Name = x.vendorproduct.Name,
                    MinPack = x.vendorproduct.MinPack,
                    SupplierPrice = x.vendorproduct.SupplierPrice,
                    Quantity = x.vendorproduct.Quantity,
                    Status = x.vendorproduct.Status
                });
        }

        public bool AddVendorProductLinks(string eisSKU, List<string> selectedEisSupplierSKUs)
        {
            // iterate to each Vendor SKUs and add them
            foreach (var eisSupplierSKU in selectedEisSupplierSKUs)
            {
                // let's check first if the selected EIS SKU exist
                var existingProductLink = _context.vendorproductlinks
                    .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU && x.EisSKU == eisSKU);
                if (existingProductLink != null)
                {
                    existingProductLink.IsActive = true;
                }
                else
                {
                    // add the product link
                    _context.vendorproductlinks.Add(new vendorproductlink
                    {
                        EisSKU = eisSKU,
                        EisSupplierSKU = eisSupplierSKU,
                        IsActive = true,
                        Created = DateTime.UtcNow
                    });
                }
            }

            // save the changes
            _context.SaveChanges();

            return true;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _logger.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
