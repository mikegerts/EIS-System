using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface IFileSettingService
    {
        /// <summary>
        /// Get the list of eisProduct file settings
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileSettingViewModel> GetProductFileSettings();

        /// <summary>
        /// Get the eisProduct file setting with the specified vendor id
        /// </summary>
        /// <param name="vendorId">The vendor id</param>
        /// <returns></returns>
        FileSettingViewModel GetProductFileSettingByVendor(long vendorId);

        /// <summary>
        /// Create new eisProduct file setting with the specified model data
        /// </summary>
        /// <param name="viewModel">The view model to save</param>
        /// <returns></returns>
        bool CreateProductFileSetting(FileSettingViewModel viewModel);

        /// <summary>
        /// Update the eisProduct file setting with the specified vendor id and updated view model
        /// </summary>
        /// <param name="vendorId">The vendor id to update its eisProduct file setting</param>
        /// <param name="viewModel">The viewmodel contains the updated file settings</param>
        /// <returns></returns>
        bool UpdateProductFileSetting(long vendorId, FileSettingViewModel viewModel);

        /// <summary>
        /// Delete the eisProduct file setting with the specified vendor id
        /// </summary>
        /// <param name="vendorId">The vendor id to delete</param>
        /// <returns></returns>
        bool DeleteProductFileSetting(long vendorId);

        /// <summary>
        /// Get the list of eisProduct file settings
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileSettingViewModel> GetInventoryFileSettings();

        /// <summary>
        /// Get the intventory file setting with the specified vendor id
        /// </summary>
        /// <param name="vendorId">The vendor id</param>
        /// <returns></returns>
        FileSettingViewModel GetInventoryFileSettingByVendor(long vendorId);

        /// <summary>
        /// Create new inventory file setting with the specified model data
        /// </summary>
        /// <param name="viewModel">The view model to save</param>
        /// <returns></returns>
        bool CreateInventoryFileSetting(FileSettingViewModel viewModel);

        /// <summary>
        /// Update the inventory file setting with the specified vendor id and updated view model
        /// </summary>
        /// <param name="vendorId">The vendor id to update its inventory file setting</param>
        /// <param name="viewModel">The viewmodel contains the updated file settings</param>
        /// <returns></returns>
        bool UpdateInventoryFileSetting(long vendorId, FileSettingViewModel viewModel);

        /// <summary>
        /// Delete the inventory file setting with the specified vendor id
        /// </summary>
        /// <param name="vendorId">The vendor id to delete</param>
        /// <returns></returns>
        bool DeleteInventoryFileSetting(long vendorId);
    }
}
