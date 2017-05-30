using System;
using System.IO;
using System.Threading.Tasks;
using EIS.DataUploaderApp.Models;
using EIS.DataUploaderApp.Repositories;
using Excel;

namespace EIS.DataUploaderApp.FileManagers
{
    public class ExcelFileManager : IFileManager
    {
        private readonly FileSetting _config;
        private readonly FileInfo _fileInfo;
        private IExcelDataReader _excelReader;
        private int _records = 1;

        public ExcelFileManager(FileInfo fileInfo, FileSetting config)
        {
            _fileInfo = fileInfo;
            _config = config;
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

        public VendorProduct GetNextProduct()
        {
            var product = new VendorProduct();
            var hasData = false;

            while (!hasData)
            {
                try
                {
                    if (!_excelReader.Read() || _excelReader.GetString(_config.SKU) == null)
                        return null;

                    product.VendorId = _config.VendorId;
                    product.SKU = _excelReader.GetString(_config.SKU);
                    product.Name = _config.Name == null ? null : _excelReader.GetString(_config.Name ?? 0);
                    product.Description = _config.Name == null ? null : _excelReader.GetString(_config.Description ?? 0);
                    product.ShortDescription = _config.Name == null ? null : _excelReader.GetString(_config.ShortDescription ?? 0);
                    product.Category = _config.Name == null ? null : _excelReader.GetString(_config.Category ?? 0);
                    product.UPCCode = _config.Name == null ? null : _excelReader.GetString(_config.UPCCode ?? 0);
                    product.Cost = _config.Name == null ? -1 : _excelReader.GetDecimal(_config.Cost ?? 0);
                    product.Quantity = _config.Name == null ? -1 : _excelReader.GetInt16(_config.Quantity ?? 0);
                    product.ResultDate = _config.ResultDate;

                    // set the flag to true
                    hasData = true;
                }
                catch (Exception ex)
                {
                    hasData = false;
                    Console.Error.WriteLine("Error in parsing file {0} at row number: {1}", _fileInfo.FullName, _records);
                    Logger.LogError(this.GetType().Name, string.Format("Error in parsing file {0} at row number: {1}", _fileInfo.FullName, _records), ex.StackTrace);
                    throw new Exception(ex.Message);
                }
                finally
                {
                    // increment the record counter
                    _records++;
                }
            }

            return product;
        }

        public void CloseFile()
        {
            //  Free resources (IExcelDataReader is IDisposable)
            _excelReader.Close();
        }

        public async Task MoveFileAsync(string newPath)
        {
            var destinationFile = string.Format("{0}\\{1}_{2}{3}",
                newPath,
                extractFileName(_fileInfo.Name),
                DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss"),
                _fileInfo.Extension);
            
            using (var source = File.Open(_fileInfo.FullName, FileMode.Open))
            {
                using (var destination = File.Create(destinationFile))
                {
                    await source.CopyToAsync(destination);
                }
            }

            // delete the file
            File.Delete(_fileInfo.FullName);
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
