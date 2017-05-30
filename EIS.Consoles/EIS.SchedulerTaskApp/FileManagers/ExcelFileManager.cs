using Excel;
using System;
using System.IO;
using EIS.SchedulerTaskApp.Repositories;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.SchedulerTaskApp.FileManagers
{
    public class ExcelFileManager : IFileManager
    {
        private readonly vendorproductfileinventorytask _task;
        private readonly FileInfo _fileInfo;
        private IExcelDataReader _excelReader;
        private int _records = 1;

        public ExcelFileManager(FileInfo fileInfo, vendorproductfileinventorytask task)
        {
            _fileInfo = fileInfo;
            _task = task;
        }

        public void ReadFile(int startRowAt)
        {
            _records = startRowAt;
            var stream = File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read);

            if (_fileInfo.FullName.EndsWith(".xls"))
                _excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            else
                _excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            for (var i = 0; i < startRowAt; i++)
                _excelReader.Read();
        }

        public VendorProduct GetNextVendorProduct()
        {
            var product = new VendorProduct();
            try
            {
                if (!_excelReader.Read() || _excelReader.GetString((int)_task.SKU) == null)
                    return null;

                product.VendorId = _task.VendorId;
                product.SupplierSKU = _excelReader.GetString((int)_task.SKU);
                product.SupplierPrice = _task.SupplierPrice == null ? -1 : _excelReader.GetDecimal(_task.SupplierPrice ?? 0);
                product.Quantity = _task.Quantity == null ? -1 : _excelReader.GetInt16(_task.Quantity ?? 0);
                product.Name = _task.ProductName == null ? null : _excelReader.GetString(_task.ProductName ?? 0);
                product.Description = _task.Description == null ? null : _excelReader.GetString(_task.Description ?? 0);
                product.Category = _task.Category == null ? null : _excelReader.GetString(_task.Category ?? 0);
                product.UPC = _task.UPC == null ? null : _excelReader.GetString(_task.UPC ?? 0);
                product.MinPack = _task.MinPack == null ? -1 : _excelReader.GetInt32(_task.MinPack ?? 0);

                // set the flag to true
                product.HasInvalidData = false;
            }
            catch (Exception ex)
            {
                product.HasInvalidData = true;
                Logger.LogError(LogEntryType.FileInventoryTaskService, string.Format("Error in parsing vendor product file {0} at row number: {1}", _fileInfo.FullName, _records), ex.StackTrace);                
            }
            finally
            {
                // increment the record counter
                _records++;
            }

            return product;
        }

        public bool DeleteDownloadedFile()
        {
            try
            {
                //  Free resources (IExcelDataReader is IDisposable)
                _excelReader.Close();

                File.Delete(_fileInfo.FullName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int TotalRecords
        {
            get { return 1000; /*** _excelReader.RowsCount ***/; }
        }

        public int CurrentRowIndex
        {
            get { return _records; }
        }

        private string extractFileName(string fileName)
        {
            var position = fileName.LastIndexOf('.');

            return position == -1 ? fileName : fileName.Substring(0, position);
        }
    }
}
