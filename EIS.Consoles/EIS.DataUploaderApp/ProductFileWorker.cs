using System;
using System.IO;
using System.Threading.Tasks;
using EIS.DataUploaderApp.FileManagers;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Repositories;

namespace EIS.DataUploaderApp
{
    public class ProductFileWorker
    {
        private readonly string _connectionString;
        private readonly ProductSettingRepository _settingRepository;

        public ProductFileWorker(string connectionString)
        {
            _connectionString = connectionString;
            _settingRepository = new ProductSettingRepository(connectionString);
        }

        public void ReadAndParsedFile()
        {
            // use the System's date
            var systemDateTime = DateTime.Now;
            var productFileSettings = _settingRepository.GetProductFileSettings(systemDateTime);

            foreach (var fileSetting in productFileSettings)
            {
                try
                {
                    // continue; if it's not reading time yet
                    if (!fileSetting.IsRunNow && !isWithInReadingTime(systemDateTime, fileSetting.ReadTime.TimeOfDay))
                        continue;

                    if (isFileSettingAlreadyExecuted(fileSetting.LastExecutedOn))
                        continue;

                    // let's update file setting execution
                    _settingRepository.UpdateFileSettingLastExecution(fileSetting.VendorId);

                    // download the file from the FTP and get the downloaded file path
                    var ftpWebRequestor = new FtpWebRequestor(fileSetting);
                    var downloadedFilePath = ftpWebRequestor.DownloadFtpFile();

                    // there should be an error if the downloaded file path is null and let's continue
                    if (downloadedFilePath == null)
                    {
                        Logger.LogWarn(this.GetType().Name, string.Format("The file for vendor ID {0} was not successfully downloaded, set for the next day reading!", fileSetting.VendorId));
                        _settingRepository.SetProductNextUploadDate(fileSetting.VendorId, systemDateTime.AddDays(1));
                        continue;
                    }

                    // delete the file from FTP server first
                    ftpWebRequestor.DeleteFileFromFtpAsync();

                    // let's create file manager
                    fileSetting.ResultDate = DateTime.UtcNow.Date;
                    var fileManager = FileManagerFactory.CreateFileManager(new FileInfo(downloadedFilePath), fileSetting, fileSetting.FileType);
                    
                    // create the task for managing file for this file setting and start the parsing file
                    var task = Task.Factory.StartNew(() =>
                    {
                        var dataService = new FileDataService(fileManager, fileSetting, _connectionString);
                        dataService.DeleteVendorProducts();
                        return dataService.ParsedDataFile();

                    }).ContinueWith(t =>
                    {
                        // set the next read date for the product file  
                        _settingRepository.SetProductNextUploadDate(t.Result.OwnerId, systemDateTime.AddDays(1));

                        // if it has no error update the main products table with the parsed product data file
                        if (!t.Result.HasError)
                        {
                            var productService = new ProductDataService(t.Result, _connectionString);
                            productService.UpdateProducts();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(string.Format("Error in parsing product file for vendor ID: {0} Error message: {1}", fileSetting.VendorId, ex.Message));
                    Logger.LogError(this.GetType().Name, string.Format("Error in parsing product file for vendor ID: {0} Error message: {1}", fileSetting.VendorId, ex.Message), ex.StackTrace);
                }
            }
        }

        private bool isFileSettingAlreadyExecuted(DateTime? lastExecution)
        {
            if (lastExecution == null)
                return false;

            var today = DateTime.Now;
            return (today.Date == lastExecution.Value.Date
                && isTimeEqual(today.TimeOfDay, lastExecution.Value.TimeOfDay));
        }

        private bool isTimeEqual(TimeSpan today, TimeSpan taskReadTime)
        {
            return (today.Hours == taskReadTime.Hours && today.Minutes == taskReadTime.Minutes);
        }

        private bool isWithInReadingTime(DateTime systemTime, TimeSpan readingTime)
        {
            return readingTime >= systemTime.TimeOfDay && readingTime <= systemTime.AddMinutes(1).TimeOfDay;
        }
    }
}
