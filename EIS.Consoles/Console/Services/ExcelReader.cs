using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EIS.Console.DAL.Database;
using EIS.Console.DAL.Repository;
using Microsoft.Office.Interop.Excel;

namespace EIS.Console.Services
{
	public class ExcelReader : IFileReader
	{
		private FileInfo _file;
		private Range _range;
		private vendor _vendor;
		private DateTime _date;
		private productfileconfig _productfileconfig;
		private IList<imagefileconfig> _imagefileconfig;

		public ExcelReader(FileInfo file, vendor vendor)
		{
			_file = file;
			_vendor = vendor;
			_date = DateTime.Now;
		}

        public async Task Upload()
        {
            //			System.Console.WriteLine("Uploading ");
            _productfileconfig = await VendorsConfig.GetProductFileConfigByVendorId(_vendor.Id);
            _imagefileconfig = await VendorsConfig.GetImageFileConfigByVendorId(_vendor.Id);

            var xlApplication = new Application();
            xlApplication.DisplayAlerts = false;

            var xlWorkbook = xlApplication.Workbooks.Open(_file.FullName);
            var xlWorksheet = (Worksheet)xlWorkbook.Worksheets.Item[1];

            _range = xlWorksheet.UsedRange;

            await InsertVendorProducts();

            //Cleanup
            _range.Clear();
            Marshal.FinalReleaseComObject(_range);
						_range = null;

            Marshal.FinalReleaseComObject(xlWorksheet);
						xlWorksheet = null;

            xlWorkbook.Close(Type.Missing, Type.Missing, Type.Missing);
            Marshal.FinalReleaseComObject(xlWorkbook);
						xlWorkbook = null;

						xlApplication.Quit();
						Marshal.FinalReleaseComObject(xlApplication);
						xlApplication = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            await MoveFile();
        }

		private async Task MoveFile()
		{
      var destinationFile = string.Format("{0}{1}_{2}{3}", _vendor.TransferPath, _vendor.FileName, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"), _file.Extension);

			Logger.LogInfo(destinationFile);

			using (var source = File.Open(_file.FullName, FileMode.Open))
			{
				using (Stream destination = File.Create(destinationFile))
				{
					await source.CopyToAsync(destination);
				}
			}

			_file.Delete();
		}

		private async Task InsertVendorProducts()
		{
			var count = 0;
			try
			{
				for (var rCnt = (int)_vendor.RowAt; rCnt <= _range.Rows.Count; rCnt++)
				{
					var SKU = (_productfileconfig.SKU != -1) ? (_range.Cells[rCnt, (_productfileconfig.SKU + 1)] as Range).Value2 : null;
					var Name = (_productfileconfig.Name != -1) ? (_range.Cells[rCnt, (_productfileconfig.Name + 1)] as Range).Value2 : null;
					var Description = (_productfileconfig.Description != -1) ? (_range.Cells[rCnt, (_productfileconfig.Description + 1)] as Range).Value2 : null;
					var ShortDescription = (_productfileconfig.ShortDescription != -1) ? (_range.Cells[rCnt, (_productfileconfig.ShortDescription + 1)] as Range).Value2 : null;
					var Category = (_productfileconfig.Category != -1) ? (_range.Cells[rCnt, (_productfileconfig.Category + 1)] as Range).Value2 : null;
					var UPCCode = (_productfileconfig.UPCCode != -1) ? (_range.Cells[rCnt, (_productfileconfig.UPCCode + 1)] as Range).Value2 : null;
					var SupplierCost = (_productfileconfig.SupplierCost != -1) ? (_range.Cells[rCnt, (_productfileconfig.SupplierCost + 1)] as Range).Value2 : null;

					var product = new vendorproduct
					{
						VendorId = _vendor.Id,
						SKU = (SKU != null) ? Convert.ToString(SKU) : "",
						Name = (Name != null) ? Convert.ToString(Name) : "",
						Description = (Description != null) ? Convert.ToString(Description) : "",
						ShortDescription = (ShortDescription != null) ? Convert.ToString(ShortDescription) : "",
						Category = (Category != null) ? Convert.ToString(Category) : "",
						UPCCode = (UPCCode != null) ? Convert.ToString(UPCCode) : "",
						SupplierCost = (SupplierCost != null) ? Convert.ToDecimal(SupplierCost) : 0,
						ResultDate = _date
					};

					var productId = await VendorsConfig.AddVendorProduct(product);

					await InsertProductImages(productId, rCnt);
					count++;

					var percentage = (((double)count + 1) / _range.Rows.Count) * 100.00;
					System.Console.WriteLine("{1:#0.00}% Inserted product: {0}", product.SKU, percentage);
				}

				System.Console.WriteLine("Uploaded {0} products for {1}", count, _vendor.VendorName);
			}
			catch (Exception ex)
			{
                System.Console.WriteLine(ex.StackTrace);
			}
		}

		private async Task InsertProductImages(long vendorProductId, int row)
		{
				foreach (var imageConfig in _imagefileconfig)
				{
					var image = new productimage
					{
						ImagePath = Convert.ToString((_range.Cells[row, (imageConfig.ColumnNo + 1)] as Range).Value2),
						ImageType = imageConfig.ImageType,
						VendorProductId = vendorProductId
					};

					await VendorsConfig.AddProductImage(image);
				}
		}
	}
}