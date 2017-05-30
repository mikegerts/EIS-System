using EIS.Inventory.Shared.ViewModels;

namespace EIS.SchedulerTaskApp.FileManagers
{
    public interface IFileManager
    {
        /// <summary>
        /// Get the number of total records from the file
        /// </summary>
        int TotalRecords { get; }

        /// <summary>
        /// Get the current row index being processed
        /// </summary>
        int CurrentRowIndex { get; }

        /// <summary>
        /// Read the file and start at start row
        /// </summary>
        /// <param name="startRowAt">The starting row to read at</param>
        void ReadFile(int startRowAt);

        /// <summary>
        /// Get the next vendor product from the parsed file line
        /// </summary>
        /// <returns></returns>
        VendorProduct GetNextVendorProduct();

        /// <summary>
        /// Close the file
        /// </summary>
        bool DeleteDownloadedFile();
    }
}
