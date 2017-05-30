using System;
using System.Drawing;
using System.IO;

namespace EIS.Inventory.Shared.Helpers
{
    public class PersistenceHelper : IPersistenceHelper
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
        public void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public void WriteImageData(string fullPath, byte[] imageData)
        {
            var ms = new MemoryStream(imageData);
            var bmp = new Bitmap(ms);
            bmp.Save(fullPath);
        }
        public void RemoveFileIfItExists(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public void WriteFileData(string fullPath, byte[] fileData)
        {
            var ms = new MemoryStream(fileData);
            File.WriteAllBytes(fullPath, fileData);
        }
    }
}
