using System;
using System.IO;
using CsvHelper;
using EIS.SchedulerTaskApp.Repositories;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using System.Text.RegularExpressions;

namespace EIS.SchedulerTaskApp.FileManagers
{
    public class CsvFileManager : IFileManager
    {
        private readonly vendorproductfileinventorytask _task;
        private readonly FileInfo _fileInfo;
        private StreamReader _reader;
        private CsvReader _csvReader;
        private int _records = 1;

        public CsvFileManager(FileInfo fileInfo, vendorproductfileinventorytask task)
        {
            _fileInfo = fileInfo;
            _task = task;
        }

        public void ReadFile(int startRowAt)
        {
            _reader = new StreamReader(File.OpenRead(_fileInfo.FullName));
            _csvReader = new CsvReader(_reader);

            // let's tell the reader that there is a header row
            _csvReader.Configuration.HasHeaderRecord = startRowAt == 1;
        }

        public VendorProduct GetNextVendorProduct()
        {
            var product = new VendorProduct();

            try
            {
                // check if it is still has record to read
                if (!_csvReader.Read())
                    return null;

                product.VendorId = _task.VendorId;
                product.SupplierSKU = _csvReader.GetField<string>((int)_task.SKU);
                product.Quantity = _task.Quantity == null ? -1 : toInt(_csvReader.GetField<string>(_task.Quantity ?? 0));
                product.IsQuantitySet = _task.Quantity != null;
                product.SupplierPrice = _task.SupplierPrice == null ? -1 : toDecimal(_csvReader.GetField<string>(_task.SupplierPrice ?? 0));
                product.IsSupplierPriceSet = _task.SupplierPrice != null;
                product.Name = _task.ProductName == null ? null : _csvReader.GetField<string>(_task.ProductName ?? 0);
                product.Description = _task.Description == null ? null : _csvReader.GetField<string>(_task.Description ?? 0);
                product.Category = _task.Category == null ? null : _csvReader.GetField<string>(_task.Category ?? 0);
                product.UPC = _task.UPC == null ? null : _csvReader.GetField<string>(_task.UPC ?? 0);
                product.MinPack = _task.MinPack == null ? -1 : _csvReader.GetField<int>(_task.MinPack ?? 0);
                product.IsMinPackSet = _task.MinPack != null;

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
                if (_reader != null)
                    _reader.Close();

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
            get { return File.ReadAllLines(_fileInfo.FullName).Length; }
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

        private decimal toDecimal(string value)
        {
            // remove unwanted space
            value = Regex.Replace(value, @"\s+", "");
            return string.IsNullOrEmpty(value) ? 0 : decimal.Parse(value);
        }

        private int toInt(string value)
        {
            // remove unwanted space
            value = Regex.Replace(value, @"\s+", "");
            return string.IsNullOrEmpty(value) ? 0 : Int32.Parse(value);
        }
    }
}
