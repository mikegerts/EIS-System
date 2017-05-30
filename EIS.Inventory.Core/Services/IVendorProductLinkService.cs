using System;
using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface IVendorProductLinkService : IDisposable
    {
        /// <summary>
        /// Get the list of EIS product link to the specified eisSupplierSKU
        /// </summary>
        /// <param name="eisSupplierSKU">The EIS supplier SKU of Vendor Product</param>
        /// <returns></returns>
        IEnumerable<ProductLinkDto> GetEisProductLinks(string eisSupplierSKU);

        /// <summary>
        /// Add EIS product link with the vendor product
        /// </summary>
        /// <param name="eisSupplierSKU">The EIS supplier SKU of Vendor Product</param>
        /// <param name="selectedEisSKUs">The list of selected EIS SKU of the product</param>
        /// <returns></returns>
        bool AddEisProductLinks(string eisSupplierSKU, List<string> selectedEisSKUs);

        /// <summary>
        /// Delete the vendor product link with the specified ID
        /// </summary>
        /// <param name="eisSKU">The EIS SKU</param>
        /// <param name="eisSupplierSKU">The EIS supplier SKU of Vendor Product</param>
        /// <returns></returns>
        bool DeleteProductLink(string eisSKU, string eisSupplierSKU);

        /// <summary>
        /// Get the list of vendor products that are link to the specified EIS SKU
        /// </summary>
        /// <param name="eisSKU">The EIS SKU</param>
        /// <returns></returns>
        IEnumerable<VendorProductLinkDto> GetVendorProductLinks(string eisSKU);

        /// <summary>
        /// Add the list of vendor products's SKUs to the specified EIS SKU        
        /// </summary>
        /// <param name="eisSKU">The EIS SKU</param>
        /// <param name="selectedEisSupplierSKUs">The selected EIS vendor product supplier SKUs</param>
        /// <returns></returns>
        bool AddVendorProductLinks(string eisSKU, List<string> selectedEisSupplierSKUs);
    }
}
