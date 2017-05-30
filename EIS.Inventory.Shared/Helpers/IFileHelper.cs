using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Helpers
{
    public interface IFileHelper
    {
        /// <summary>
        /// Save the FILE
        /// </summary>
        /// <param name="subfolderName">The sub folder name to save a file</param>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="url">The URL of the FILE</param>
        /// <returns>Returns the file name</returns>
        string SaveFile(string subfolderName, string eisSKU, string fileExtension, string url);

        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="subfolderName">The sub folder name to save a file</param>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="fileData">The byte stream of the file to save</param>
        /// <returns>Returns the file name</returns>
        string SaveFile(string subfolderName, string eisSKU, string fileExtension, byte[] fileData);

        /// <summary>
        /// Delete the file with the specified name
        /// </summary>
        /// <param name="subfolderName">The sub folder name to save a file</param>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="fileName">The file name</param>
        void RemoveFile(string subfolderName, string eisSKU, string fileName);

        /// <summary>
        /// Get the eisProduct file with the specified id and file name
        /// </summary>
        /// <param name="subfolderName">The sub folder name to save a file</param>
        /// <param name="eisSKU">The id of product or the EIS SKU</param>
        /// <param name="fileName">The file name</param>
        /// <returns></returns>
        string GetFileUri(string subfolderName, string eisSKU, string fileName);
    }
}
