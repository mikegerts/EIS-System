using System.IO;
using EIS.Console.DAL.Database;

namespace EIS.Console.Services
{
	public class FileReaderFactory
	{
		private const string Excel = "Excel";
		private const string CSV = "CSV";

		public static IFileReader GetFileReader(FileInfo file, vendor vendor)
		{
			if (vendor.FileType == Excel)
			{
				return new ExcelReader(file, vendor);
			}
			else if (vendor.FileType == CSV)
			{
				var reader = new CsvReader(file, vendor);
				reader.SetDelimiter(';');

				return reader;
			}
			return null;
		}
	}
}
