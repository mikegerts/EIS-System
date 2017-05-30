using System.Threading.Tasks;
using EIS.DataUploaderApp.Models;

namespace EIS.DataUploaderApp.FileManagers
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
        VendorProduct GetNextProduct();

        /// <summary>
        /// Close the file
        /// </summary>
        void CloseFile();

        /// <summary>
        /// Move the file to the new path define
        /// </summary>
        /// <param name="newPath">The new directory for the file</param>
        /// <returns></returns>
        Task MoveFileAsync(string newPath);
    }
}
