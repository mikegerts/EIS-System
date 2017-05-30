using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using EIS.DataUploaderApp.Models;
using EIS.DataUploaderApp.Repositories;

namespace EIS.DataUploaderApp.FileManagers
{
    public class CsvFileManager : IFileManager
    {
        private readonly FileSetting _fileSetting;
        private readonly FileInfo _fileInfo;
        private StreamReader _reader;
        private CsvReader _csvReader;
        private int _records = 1;

        public CsvFileManager(FileInfo fileInfo, FileSetting config)
        {
            _fileInfo = fileInfo;
            _fileSetting = config;
        }

        public void ReadFile(int startRowAt)
        {
            _reader = new StreamReader(File.OpenRead(_fileInfo.FullName));
            _csvReader = new CsvReader(_reader);

            // let's tell the reader that there is a header row
            _csvReader.Configuration.HasHeaderRecord = startRowAt == 1;
        }

        public VendorProduct GetNextProduct()
        {
            var product = new VendorProduct();
            var hasData = false;

            while(!hasData)
            {
                try
                {
                    // check if it is still has record to read
                    if (!_csvReader.Read())
                        return null;

                    product.VendorId = _fileSetting.VendorId;
                    product.SKU = _csvReader.GetField<string>(_fileSetting.SKU);
                    product.Name = _fileSetting.Name == null ? null : _csvReader.GetField<string>(_fileSetting.Name ?? 0);
                    product.Description = _fileSetting.Description == null ? null : _csvReader.GetField<string>(_fileSetting.Description ?? 0);
                    product.ShortDescription = _fileSetting.ShortDescription == null ? null : _csvReader.GetField<string>(_fileSetting.ShortDescription ?? 0);
                    product.Category = _fileSetting.Category == null ? null : _csvReader.GetField<string>(_fileSetting.Category ?? 0);
                    product.UPCCode = _fileSetting.UPCCode == null ? null : _csvReader.GetField<string>(_fileSetting.UPCCode ?? 0);
                    product.Cost = _fileSetting.Cost == null ? -1 : toDecimal(_csvReader.GetField<string>(_fileSetting.Cost ?? 0));
                    product.Quantity = _fileSetting.Quantity == null ? -1 : toInt(_csvReader.GetField<string>(_fileSetting.Quantity ?? 0));
                    product.ResultDate = _fileSetting.ResultDate;

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
            if (_reader != null)
                _reader.Close();
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
            return string.IsNullOrEmpty(value) ? 0 : decimal.Parse(value);
        }

        private int toInt(string value)
        {
            return string.IsNullOrEmpty(value) ? 0 : Int32.Parse(value);
        }
    }
}
