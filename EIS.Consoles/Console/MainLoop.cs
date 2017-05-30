using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EIS.Console.DAL.Database;
using EIS.Console.DAL.Repository;
using EIS.Console.Services;

namespace EIS.Console
{
	public class MainLoop
	{
		private bool _loop = true;
		private int _interval = 10; // 10 seconds

		public MainLoop(int interval)
		{
			_interval = interval;
		}

		public async Task StartAsync()
		{
				while (_loop)
				{
						var errorMessage = string.Empty;
						try
						{
								System.Console.WriteLine("Searching for scheduled vendors to upload files.");
								foreach (var vendor in await VendorsConfig.GetScheduledVendors())
								{
									var hasError = false;

									uploadstatu status;
									if (vendor.uploadstatus.Count > 0)
									{
										status = vendor.uploadstatus.First();
										await VendorsConfig.DeleteCurrentProductsByVendorId(vendor.Id);
									}
									else
									{
										status = await VendorsConfig.CreateUploadStatus(vendor.Id);
									}

									try
									{
											var file = GetUploadedFile(vendor);
											if (file != null)
											{
													IFileReader reader = FileReaderFactory.GetFileReader(file, vendor);

													await reader.Upload();
											}
											else
											{
													await Task.Run(() => Logger.LogDisplay(String.Format("No products were uploaded for {0}", vendor.VendorName), "Info"));
											}

											await VendorsConfig.UpdateUploadStatus(status, true);
									}
									catch (Exception ex)
									{
											System.Console.WriteLine(ex);
											Logger.LogException(ex);
											hasError = true;
									}

									//When error occured set uploadstatus
									if (hasError)
									{
											await VendorsConfig.UpdateUploadStatus(status, false);
											await Task.Run(() => Logger.LogError(String.Format("Error uploading {0} products", vendor.VendorName)));
									}
								}
						}
						catch (Exception ex)
						{
								System.Console.WriteLine(ex);
								errorMessage = ex.Message;
						}

						if (@String.IsNullOrEmpty(errorMessage))
								await Task.Run(() => Logger.LogError(errorMessage));

						await Task.Delay(GetInterval());
				}
		}

		private FileInfo GetUploadedFile(vendor vendor)
		{
			var filePath = Directory.GetFiles(vendor.FilePath).FirstOrDefault(m => m.Contains(vendor.FileName));
			if (filePath != null)
			{
				var file = new FileInfo(filePath);

				if (!String.IsNullOrEmpty(file.Name))
				{
					if (String.Equals(vendor.FileType, FileType.CSV.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						if (String.Equals(file.Extension, ".csv", StringComparison.CurrentCultureIgnoreCase))
							return file;
					}
					else if (String.Equals(vendor.FileType, FileType.Excel.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						if (String.Equals(file.Extension, ".xls", StringComparison.CurrentCultureIgnoreCase) || String.Equals(file.Extension, ".xlsx", StringComparison.CurrentCultureIgnoreCase))
							return file;
					}
				}
			}

			return null;
		}

		private int GetInterval()
		{
			return _interval * 1000;
		}
	}
}
