using System;
using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface IVendorService : IDisposable
    {
        /// <summary>
        /// Get the list of all vendors
        /// </summary>
        /// <returns></returns>
        IEnumerable<VendorListDto> GetAllVendors();

        /// <summary>
        /// Get ther vendor with the specified vendor id
        /// </summary>
        /// <param name="vendorId">The vendor Id</param>
        /// <returns></returns>
        VendorDto GetVendor(int vendorId);

        /// <summary>
        /// Get the list of vendors with the specified company id
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        IEnumerable<VendorDto> GetVendorsByCompany(int companyId);

        /// <summary>
        /// Create new vendor in the database
        /// </summary>
        /// <param name="model">The vendor to save</param>
        /// <returns></returns>
        VendorDto CreateVendor(VendorDto model);

        /// <summary>
        /// Update the vendor with the modified model
        /// </summary>
        /// <param name="model">The updated vendor</param>
        /// <returns></returns>
        VendorDto UpdateVendor(VendorDto model);

        /// <summary>
        /// Delete the vendor with the specified vendor Id
        /// </summary>
        /// <param name="vendorId">The vendor Id</param>
        /// <returns></returns>
        bool DeleteVendor(int vendorId);

        /// <summary>
        /// Get the EIS stating SKU code for the specified vendor
        /// </summary>
        /// <param name="vendorId">The id of the vendor</param>
        /// <returns></returns>
        string GetVendorStartSku(int vendorId);

        /// <summary>
        /// Get the vendor's email address for the specified vendor id
        /// </summary>
        /// <param name="vendorId">The id of the vendor</param>
        /// <returns></returns>
        string GetVendorEmail(int vendorId);

        /// <summary>
        /// Get the list of departments
        /// </summary>
        /// <returns></returns>
        IEnumerable<DeparmentDto> GetDepartments ();
    }
}
