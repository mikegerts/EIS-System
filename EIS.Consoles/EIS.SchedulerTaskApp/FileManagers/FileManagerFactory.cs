using System.IO;
using EIS.SchedulerTaskApp.Helpers;
using EIS.Inventory.DAL.Database;

namespace EIS.SchedulerTaskApp.FileManagers
{
    public class FileManagerFactory
    {
        public static IFileManager CreateFileManager(FileInfo fileInfo, vendorproductfileinventorytask fileSetting, FileType fileType)
        {
            if (fileType == FileType.Excel)
                return new ExcelFileManager(fileInfo, fileSetting);
            else
                return new CsvFileManager(fileInfo, fileSetting);
        }
    }
}
