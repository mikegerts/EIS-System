using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EIS.Console.DAL.Database;
using EIS.Console.DAL.Repository;

namespace EIS.Console.Services
{
	public class CsvReader : IFileReader
	{
		private FileInfo _file;
		private vendor _vendor;
		private char _delimiter;
		private DateTime _date;
		private productfileconfig _productfileconfig;
		private IList<imagefileconfig> _imagefileconfig;

		public CsvReader(FileInfo file, vendor vendor)
		{
			_file = file;
			_vendor = vendor;
			_date = DateTime.Now;
		}

        public async Task Upload()
        {
            var fileReader = new StreamReader(_file.FullName);
            var count = 0;

            _productfileconfig = await VendorsConfig.GetProductFileConfigByVendorId(_vendor.Id);
            _imagefileconfig = await VendorsConfig.GetImageFileConfigByVendorId(_vendor.Id);

            for (var i = 1; i < _vendor.RowAt; ++i)
                await fileReader.ReadLineAsync();

            var totalLines = File.ReadAllLines(_file.FullName).Count();
            var line = await fileReader.ReadLineAsync();
            while (!String.IsNullOrEmpty(line))
            {
                await InsertVendorProduct(line);

                line = await fileReader.ReadLineAsync();
                count++;

                var percentage = (((double)count + 1) / totalLines - _vendor.RowAt) * 100.00;
                System.Console.WriteLine("{1:#0.00}% Inserting product for vendor: {0}", _vendor.VendorName, percentage);
            }

            System.Console.WriteLine("Done uploading {0} products for {1}", count, _vendor.VendorName);
            fileReader.Close();

            await MoveFile();
        }

		private async Task MoveFile()
		{
			var destinationFile = string.Format("{0}{1}_{2}{3}", _vendor.TransferPath, _vendor.FileName, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"), _file.Extension);
			using (var source = File.Open(_file.FullName, FileMode.Open))
			{
				using (var destination = File.Create(destinationFile))
				{
					await source.CopyToAsync(destination);
				}
			}
			_file.Delete();
		}


		private async Task InsertVendorProduct(string unSplitRowValues)
		{
			var rowValues = unSplitRowValues.Split(_delimiter).ToList();

			var product = new vendorproduct
			{
				VendorId = _vendor.Id,
				SKU = rowValues[(int)_productfileconfig.SKU],
				Name = rowValues[(int)_productfileconfig.Name],
				Description = rowValues[(int)_productfileconfig.Description],
				ShortDescription = rowValues[(int)_productfileconfig.ShortDescription],
				Category = rowValues[(int)_productfileconfig.Category],
				UPCCode = rowValues[(int)_productfileconfig.UPCCode],
				SupplierCost = Convert.ToDecimal(rowValues[(int)_productfileconfig.SupplierCost]),
				ResultDate = _date
			};

			var vendorProductId = await VendorsConfig.AddVendorProduct(product);

			await InsertProductImages(rowValues, vendorProductId);
		}

		private async Task InsertProductImages(IList<string> rowValues, long vendorProductId)
		{
			foreach (var imageConfig in _imagefileconfig)
			{
				var image = new productimage
				{
					ImagePath = rowValues[(int) imageConfig.ColumnNo],
					ImageType = imageConfig.ImageType,
					VendorProductId = vendorProductId
				};

				await VendorsConfig.AddProductImage(image);
			}
		}

		public void SetDelimiter(char delimiter)
		{
			_delimiter = delimiter;
		}
	}
}
