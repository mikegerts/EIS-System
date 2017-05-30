using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;

namespace EIS.Inventory.Shared.Helpers
{
    public class FileHelper : IFileHelper
    {
        private readonly string _fileFolder;
        private readonly IPersistenceHelper _persistenceHelper;
        private string _root = string.Empty;
        private string _urlRoot = string.Empty;

        public FileHelper(IPersistenceHelper persistenceHelper)
        {
            _fileFolder = ConfigurationManager.AppSettings["FilesRoot"].ToString();
            _persistenceHelper = persistenceHelper;

            try
            {
                _urlRoot = ConfigurationManager.AppSettings["FilesUrlRoot"].ToString();
                char last = _urlRoot[_urlRoot.Length - 1];
                if (last != '/') _urlRoot = String.Format("{0}/", _urlRoot);
            }
            catch (NullReferenceException) { _urlRoot = string.Empty; }
        }

        private string root
        {
            get
            {
                if (string.IsNullOrEmpty(_root))
                {
                    var path = _fileFolder;
                    var last = path[path.Length - 1];
                    if (last != '\\')
                        path = string.Format("{0}\\", path);

                    _root = path;
                }

                return _root;
            }
        }

        private string urlRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_urlRoot))
                {
                    var uri = HttpContext.Current.Request.Url;
                    _urlRoot = String.Format("http://{0}:{1}/Files/", uri.Host, uri.Port);
                }

                return _urlRoot;
            }
        }

        public string GetFileUri(string subfolderName, string eisSKU, string fileName)
        {
            var folderId = getFolderId(eisSKU);
            return getFileUri(subfolderName, folderId, fileName);
        }

        public void RemoveFile(string subfolderName, string eisSKU, string fileName)
        {
            var folderId = getFolderId(eisSKU);
            removeFile(subfolderName, folderId, fileName);
        }

        public string SaveFile(string subfolderName, string eisSKU, string fileExtension, byte[] fileData)
        {
            var folderId = getFolderId(eisSKU);
            return saveFile(subfolderName, folderId, fileExtension, fileData);
        }

        public string SaveFile(string subfolderName, string eisSKU, string fileExtension, string url)
        {
            var folderId = getFolderId(eisSKU);
            return downloadFileFromUrl(subfolderName, folderId, fileExtension, url);
        }


        #region helper methods
        private string saveFile(string subFolder, int folderId, string fileExtension, byte[] fileData)
        {
            // make sure the subfolder has been created
            _persistenceHelper.CreateDirectoryIfNotExists(string.Format("{0}{1}\\", root, subFolder));

            var folderPath = String.Format("{0}{1}\\{2}\\", root, subFolder, folderId);
            _persistenceHelper.CreateDirectoryIfNotExists(folderPath);

            //Create a unique file name
            var index = DateTime.Now.Millisecond;
            var fileName = String.Format("{0}-{1}"+ fileExtension, folderId, index);
            var fullPath = String.Format("{0}{1}", folderPath, fileName);
            while (File.Exists(fullPath))
            {
                index++;
                fileName = String.Format("{0}-{1}" + fileExtension, folderId, index);
                fullPath = String.Format("{0}{1}", folderPath, fileName);
            }

            //Write the image to the folder
            _persistenceHelper.WriteFileData(fullPath, fileData);

            return fileName;
        }

        private void removeFile(string subFolder, int folderId, string fileName)
        {
            var folderPath = String.Format("{0}{1}\\{2}\\", root, subFolder, folderId);
            var fullPath = String.Format("{0}{1}", folderPath, fileName);

            // delete the file
            _persistenceHelper.RemoveFileIfItExists(fullPath);
        }

        private string getFileUri(string subFolder, int folderId, string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return "";

            var folderPath = String.Format("{0}\\{1}\\{2}\\", root, subFolder, folderId);
            var fullPath = String.Format("{0}\\{1}", folderPath, fileName);

            return (String.IsNullOrEmpty(fileName)) ? "" : String.Format("{0}{1}/{2}/{3}", urlRoot, subFolder, folderId, fileName);
        }

        private string downloadFileFromUrl(string subFolder, int folderId, string fileExtension, string url)
        {
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpWebRequest.AllowWriteStreamBuffering = true;
            httpWebRequest.Timeout = 50000;
            var fileName = string.Empty;

            try
            {
                using (var webResponse = httpWebRequest.GetResponse())
                {
                    using (var stream = webResponse.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            fileName = saveFile(subFolder, folderId, fileExtension, ms.ToArray());
                        }
                    }
                }
            }
            catch { }

            return fileName;
        }

        private int getFolderId(string eisSKU)
        {
            return int.Parse(eisSKU);
        }

        #endregion
    }
}