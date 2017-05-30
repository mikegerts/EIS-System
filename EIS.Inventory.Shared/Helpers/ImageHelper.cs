using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.Helpers
{
    public class ImageHelper : IImageHelper
    {
        private readonly string _imageFolder;
        private readonly IPersistenceHelper _persistenceHelper;
        private string _root = string.Empty;
        private string _urlRoot = string.Empty;

        private const string ProductSubFolder = "Product";
        private const string VendorProductSubFolder = "VendorProduct";
        private const string CustomerSubFolder = "Customer";

        public ImageHelper(IPersistenceHelper persistenceHelper)
        {
            _imageFolder = ConfigurationManager.AppSettings["ImagesRoot"].ToString();
            _persistenceHelper = persistenceHelper;

            try
            {
                _urlRoot = ConfigurationManager.AppSettings["ImagesUrlRoot"].ToString();
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
                    var path = _imageFolder;
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
                    _urlRoot = String.Format("http://{0}:{1}/Images/", uri.Host, uri.Port);
                }

                return _urlRoot;
            }
        }

        #region Product images
        public string SaveProductImage(string eisSKU, string url)
        {            
            var folderId = getFolderId(eisSKU);
            return downloadImageFromUrl(ProductSubFolder, folderId, url);
         }       

        public string SaveProductImage(string eisSKU, byte[] imageData)
        {
            var folderId = getFolderId(eisSKU);
            return saveImage(ProductSubFolder, folderId, imageData);
        }

        public void RemoveProductImage(string eisSKU, string fileName)
        {
            var folderId = getFolderId(eisSKU);
            removeImage(ProductSubFolder, folderId, fileName);
        }

        public string GetProductImageUri(string eisSKU, string fileName)
        {
            var folderId = getFolderId(eisSKU);
            return getImageUri(ProductSubFolder, folderId, fileName);
        }
        #endregion

        #region Vendor Product images
        public string SaveVendorProductImage(string eisSupplierSKU, string url)
        {
            var folderId = getFolderId(eisSupplierSKU);
            return downloadImageFromUrl(VendorProductSubFolder, folderId, url);
        }

        public string SaveVendorProductImage(string eisSupplierSKU, byte[] imageData)
        {
            var folderId = getFolderId(eisSupplierSKU);
            return saveImage(VendorProductSubFolder, folderId, imageData);
        }

        public void RemoveVendorProductImage(string eisSupplierSKU, string fileName)
        {
            var folderId = getFolderId(eisSupplierSKU);
            removeImage(VendorProductSubFolder, folderId, fileName);
        }

        public string GetVendorProductImageUri(string eisSupplierSKU, string fileName)
        {
            var folderId = getFolderId(eisSupplierSKU);
            return getImageUri(VendorProductSubFolder, folderId, fileName);
        }
        #endregion

        #region Customer Images

        public string SaveCustomerImage(int customerId, string url)
        {
            var folderId = getFolderId(customerId.ToString());
            return downloadImageFromUrl(CustomerSubFolder, folderId, url);
        }

        public string SaveCustomerImage(int customerId, byte[] imageData)
        {
            var folderId = getFolderId(customerId.ToString());
            return saveImage(CustomerSubFolder, folderId, imageData);
        }

        public void RemoveCustomerImage(int customerId, string fileName)
        {
            var folderId = getFolderId(customerId.ToString());
            removeImage(CustomerSubFolder, folderId, fileName);
        }

        public string GetCustomerImageUri(int customerId, string fileName)
        {
            var folderId = getFolderId(customerId.ToString());
            return getImageUri(CustomerSubFolder, folderId, fileName);
        }
        #endregion

        #region helper methods
        private string saveImage(string subFolder, int folderId, byte[] imageData)
        {
            // make sure the subfolder has been created
            _persistenceHelper.CreateDirectoryIfNotExists(string.Format("{0}{1}\\", root, subFolder));

            var folderPath = String.Format("{0}{1}\\{2}\\", root, subFolder, folderId);
            _persistenceHelper.CreateDirectoryIfNotExists(folderPath);

            //Create a unique file name
            var index = DateTime.Now.Millisecond;
            var fileName = String.Format("{0}-{1}.jpg", folderId, index);
            var fullPath = String.Format("{0}{1}", folderPath, fileName);
            while (File.Exists(fullPath))
            {
                index++;
                fileName = String.Format("{0}-{1}.jpg", folderId, index);
                fullPath = String.Format("{0}{1}", folderPath, fileName);
            }

            //Write the image to the folder
            _persistenceHelper.WriteImageData(fullPath, imageData);

            return fileName;
        }

        private void removeImage(string subFolder, int folderId, string fileName)
        {
            var folderPath = String.Format("{0}{1}\\{2}\\", root, subFolder, folderId);
            var fullPath = String.Format("{0}{1}", folderPath, fileName);

            // delete the file
            _persistenceHelper.RemoveFileIfItExists(fullPath);
        }

        private string getImageUri(string subFolder, int folderId, string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return "";

            var folderPath = String.Format("{0}\\{1}\\{2}\\", root, subFolder, folderId);
            var fullPath = String.Format("{0}\\{1}", folderPath, fileName);
                       
            return (String.IsNullOrEmpty(fileName)) ? "" : String.Format("{0}{1}/{2}/{3}", urlRoot, subFolder, folderId, fileName);
        }

        private string downloadImageFromUrl(string subFolder, int folderId, string url)
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
                            fileName = saveImage(subFolder, folderId, ms.ToArray());
                        }
                    }
                }
            }
            catch{ }

            return fileName;
        }

        private int getFolderId(string eisSKU)
        {
            return eisSKU.Trim().Length;
        }

        #endregion
    }
}
