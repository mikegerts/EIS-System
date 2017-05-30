using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class VendorService : IVendorService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public VendorService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }

        public IEnumerable<VendorListDto> GetAllVendors()
        {
            var vendors = _context.vendors.ToList();

            return Mapper.Map<IEnumerable<vendor>, IEnumerable<VendorListDto>>(vendors);
        }

        public VendorDto GetVendor(int vendorId)
        {
            var vendor = _context.vendors.SingleOrDefault(x=>x.Id == vendorId);
            return Mapper.Map<vendor, VendorDto>(vendor);
        }

        public IEnumerable<VendorDto> GetVendorsByCompany(int companyId)
        {
            var vendors = _context.vendors.Where(x => x.CompanyId == companyId);

            return Mapper.Map<IEnumerable<VendorDto>>(vendors);
        }

        public VendorDto CreateVendor(VendorDto model)
        {
            try
            {
                var vendor = Mapper.Map<VendorDto, vendor>(model);
                vendor.CreatedBy = model.ModifiedBy;
                vendor.Created = DateTime.UtcNow;

                _context.vendors.Add(vendor);
                _context.SaveChanges();

                // update all of its vendor products if it is configured as always in stock
                if (vendor.IsAlwaysInStock)
                    upateVendorProductsQuantity(vendor.Id, vendor.AlwaysQuantity ?? 0);

                return Mapper.Map<vendor, VendorDto>(vendor);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorService, errorMsg, ex.StackTrace);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return model;
            }
        }

        public VendorDto UpdateVendor(VendorDto model)
        {
            var oldVendor = _context.vendors.FirstOrDefault(x => x.Id == model.Id);
            var vendor = Mapper.Map<VendorDto, vendor>(model);

            vendor.ModifiedBy = model.ModifiedBy;
            vendor.Modified = DateTime.UtcNow;

            // update vendor departments
            foreach(var vendorDepartment in model.VendorDepartments)
            {
                var vd = Mapper.Map<VendorDepartmentDto, vendordepartment>(vendorDepartment);
                
                vd.Modified = DateTime.Now;

                if (vendorDepartment.Id == -1)
                {
                    vd.Create = DateTime.Now;
                    oldVendor.vendordepartments.Add(vd);
                }
                else
                {
                    var oldVendorDepartment = oldVendor.vendordepartments.Where(o => o.Id == vd.Id).First();
                    _context.Entry(oldVendorDepartment).CurrentValues.SetValues(vd);
                }
            }

            _context.Entry(oldVendor).CurrentValues.SetValues(vendor);            
            _context.SaveChanges();

            // reload the object to init init its company object
            var updatedVendor = _context.vendors.FirstOrDefault(x => x.Id == model.Id);

            // update all of its vendor products if it is configured as always in stock
            if (updatedVendor.IsAlwaysInStock)
                upateVendorProductsQuantity(updatedVendor.Id, updatedVendor.AlwaysQuantity ?? 0);

            return Mapper.Map<vendor, VendorDto>(updatedVendor);
        }

        public bool DeleteVendor(int vendorId)
        {
            var vendor = _context.vendors.SingleOrDefault(x => x.Id == vendorId);
            if (vendor == null)
                return true;

            // delete the vendor department first
            var departments = vendor.vendordepartments.ToList();
            _context.vendordepartments.RemoveRange(departments);

            // delete the vendor products assigned to this vendor
            var vendorProducts = vendor.vendorproducts.ToList();
            _context.vendorproducts.RemoveRange(vendorProducts);
            
            _context.vendors.Remove(vendor);
            _context.SaveChanges();

            return true;
        }

        public string GetVendorStartSku(int vendorId)
        {            
            var vendor = _context.vendors.SingleOrDefault(x => x.Id == vendorId);
            if (vendor == null)
                return string.Empty;

            return vendor.SKUCodeStart.Trim();
        }

        public string GetVendorEmail(int vendorId)
        {
            var vendor = _context.vendors.FirstOrDefault(x => x.Id == vendorId);
            if (vendor == null)
                return null;

            return vendor.Email;
        }

        public IEnumerable<DeparmentDto> GetDepartments ()
        {
            var departments = _context.departments.OrderBy(x => x.DepartmentName).ToList();

            return Mapper.Map<IEnumerable<department>, IEnumerable<DeparmentDto>>(departments);
        }

        private void upateVendorProductsQuantity(int vendorId, int alwaysQuantity)
        {
            // get the list of vendor products to update
            var vendorProducts = _context.vendorproducts
                .Where(x => x.VendorId == vendorId)
                .ToList();
            foreach (var product in vendorProducts)
                product.Quantity = alwaysQuantity;

            _context.SaveChanges();
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
