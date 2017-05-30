
namespace EIS.Inventory.Shared.Helpers
{
    public interface IPersistenceHelper
    {
        /// <summary>
        /// Check if the path of the directory exists
        /// </summary>
        /// <param name="path">The path to chect</param>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Check if the file exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool FileExists(string path);

        /// <summary>
        /// Create directory if not exists
        /// </summary>
        /// <param name="path"></param>
        void CreateDirectoryIfNotExists(string path);

        /// <summary>
        /// Write the image data into file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="imageData"></param>
        void WriteImageData(string fullPath, byte[] imageData);

        /// <summary>
        /// Write the file data into file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="fileData"></param>
        void WriteFileData(string fullPath, byte[] fileData);

        /// <summary>
        /// Remove the file with the specified file name
        /// </summary>
        /// <param name="fileName">The file name of the file to remove</param>
        void RemoveFileIfItExists(string fileName);
    }
}
