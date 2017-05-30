using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using System;

namespace EIS.Inventory.Core.Services
{
    public interface IExportDataService : IDisposable
    {
        /// <summary>
        /// Export the vendor products into a file with the specified criteria
        /// </summary>
        /// <param name="model">The user search criteria</param>
        /// <returns></returns>
        string CustomExportVendorProducts(ExportVendorProduct model);
        
        /// <summary>
        /// Export products with the selected criteria define in the model
        /// </summary>
        /// <param name="model">The user's criteria</param>
        /// <returns>Returns the file name</returns>
        string CustomExportProducts(ExportProduct model);


        /// <summary>
        /// Export shipping rates with the selected criteria define in the model
        /// </summary>
        /// <param name="selectedIds">The user's criteria</param>
        /// <returns>Returns the file name</returns>
        string CustomExportShippingRates(string selectedIds, DateTime currentDateTime);

        /// <summary>
        /// Export BigCommerce Categories with the selected criteria define in the model
        /// </summary>
        /// <returns>Returns the file name</returns>
        string ExportBigCommerceCategories();
    }
}
