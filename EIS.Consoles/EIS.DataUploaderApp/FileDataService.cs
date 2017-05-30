using System;
using EIS.DataUploaderApp.FileManagers;
using EIS.DataUploaderApp.Models;
using EIS.DataUploaderApp.Repositories;
using EIS.DataUploaderApp.Helpers;

namespace EIS.DataUploaderApp
{
    public class FileDataService
    {
        private readonly IFileManager _fileManager;
        private readonly FileSetting _vendorSetting;
        private readonly ProductRepository _productRepo;

        public FileDataService(IFileManager fileManager, FileSetting setting, string connectionString)
        {
            _fileManager = fileManager;
            _vendorSetting = setting;
            _productRepo = new ProductRepository(connectionString);
        }

        public FileResult ParsedDataFile()
        {
            var fileResult = new FileResult
            {
                OwnerId = _vendorSetting.VendorId,
                ResultDate = _vendorSetting.ResultDate,
                HasError = false,
            };

            try
            {
                _fileManager.ReadFile(_vendorSetting.RowAt);
                fileResult.TotalRecords = _fileManager.TotalRecords;

                var product = _fileManager.GetNextProduct();

                while (product != null)
                {
                    var percentage = (((double)_fileManager.CurrentRowIndex) / fileResult.TotalRecords) * 100.00;
                    Console.WriteLine("{0:#0.00}% Processing product for vendor \'{1}\' - {2}", percentage, _vendorSetting.VendorId, product.SKU);

                    // set the result date of the product
                    product.ResultDate = _vendorSetting.ResultDate;

                    // insert the product into the database
                    _productRepo.CreateVendorProduct(product);

                    // read and parsed the next product from the file
                    product = _fileManager.GetNextProduct();
                }

                Logger.LogInfo("FileDataService", "Successfully parsed the inventory file. Total records: " + _fileManager.TotalRecords);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(string.Format("Error Message:{0} \n{1}", ex.Message, ex.StackTrace));
                Logger.LogError("FileDataService", "Error in parsing the downloaded inventory file! Message: " + ex.Message, ex.StackTrace);
                fileResult.HasError = true;
                fileResult.TotalRecords = _fileManager.CurrentRowIndex;
                fileResult.ErrorMessage = ex.Message;
            }
            finally
            {
                // close the file and db connection
                _fileManager.CloseFile();
                _productRepo.CloseDbConnection();

                try
                {

                    // if success, move the file
                    if(!fileResult.HasError)
                        _fileManager.MoveFileAsync(_vendorSetting.TransferPath);
                }
                catch { }
            }

            return fileResult;
        }

        public void DeleteVendorProducts()
        {
            _productRepo.DeleteVendorProducts(_vendorSetting.VendorId, _vendorSetting.ResultDate);
        }
    }
}
