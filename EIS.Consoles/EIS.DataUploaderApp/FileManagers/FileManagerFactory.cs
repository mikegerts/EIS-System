using System.IO;
using EIS.DataUploaderApp.Models;
using EIS.DataUploaderApp.Helpers;

namespace EIS.DataUploaderApp.FileManagers
{
    public class FileManagerFactory
    {
        public static IFileManager CreateFileManager(FileInfo fileInfo, FileSetting fileConfig, FileType fileType)
        {
            if (fileType == FileType.Excel)
                return new ExcelFileManager(fileInfo, fileConfig);
            else
                return new CsvFileManager(fileInfo, fileConfig);
        }
    }
}
