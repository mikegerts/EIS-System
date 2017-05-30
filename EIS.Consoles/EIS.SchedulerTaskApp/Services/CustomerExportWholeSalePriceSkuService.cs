using System;
using System.Collections.Generic;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using System.Linq;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.SchedulerTaskApp.Helpers;
using System.IO;
using CsvHelper;
using MySql.Data.MySqlClient;
using System.Data;

namespace EIS.SchedulerTaskApp.Services
{
    public class CustomerExportWholeSalePriceSkuService : TaskService
    {
        private readonly customerscheduledtask _task;
        private readonly List<string> _exportedEisSupplierSKUs;
        private readonly IFileHelper _fileHelper;
        private const string _subFolderName = "Customer";

        public CustomerExportWholeSalePriceSkuService(customerscheduledtask task)
        {
            _task = task;
            _exportedEisSupplierSKUs = new List<string>();
            _generatedFile = string.Format("{0}\\{1}\\{2}",
                _exportedFileFolder,
                TaskType,
                Guid.NewGuid() + "_" + _task.ImportFileName);
        }

        public override string TaskType
        {
            get { return CustomerScheduledTaskType.CUSTOMER_EXPORT_SKU; }
        }

        public override void DoPostExecution()
        {
            // write the file into task's FTP define
            if (!string.IsNullOrEmpty(_task.FtpServer) &&
                    !string.IsNullOrEmpty(_task.FtpUser) &&
                    !string.IsNullOrEmpty(_task.FtpPassword) &&
                    _task.FtpPort != null &&
                    !string.IsNullOrEmpty(_task.RemoteFolder)
                )
            {
                var ftpRequestor = new FtpWebRequestor(_task.FtpServer,
                    _task.FtpUser,
                    _task.FtpPassword,
                    _task.FtpPort,
                    _task.RemoteFolder);
                ftpRequestor.WriteFtpFile(_generatedFile);
            }
            else
            {
                // get the message template
                var customer = GetCustomer(_task.CustomerId);
                string[] customerEmail = { customer.EmailAddress };
                string[] ccEmails = null;

                if (!string.IsNullOrEmpty(_task.ConfirmationEmailTos))
                    ccEmails = _task.ConfirmationEmailTos.Split(',');

                EmailSender.SendConfirmationMessage(customerEmail, ccEmails, _generatedFile);
            }

            // create export file record for this taks for the generated file
            createCustomerExportedFilesRecord(_task.Id);
        }

        public override bool Execute()
        {
            var product = new List<Product>();

            string fileUrl = string.Format("{0}\\{1}\\{2}\\{3}",
                _exportedFileFolder,
                _subFolderName,
                _task.CustomerId,
                _task.ImportFileName);

            var streamFileRead = new StreamReader(fileUrl);
            CsvFileDataParser.ParseCustomerWholeSalePriceSkuFile(streamFileRead, product, _task.HasHeader, _task.FileHeaders, _task.CustomFields);

            if (product.Count == 0)
                return true;

            var skuList = product.Select(x => x.EisSKU).ToList();

            var customer = GetCustomer(_task.CustomerId);

            var productList = GetProductFromEisSkuList(skuList);

            Console.WriteLine("Creating {0} file -> {1}", TaskType, _generatedFile);

            using (var streamWriter = new StreamWriter(_generatedFile))
            {
                var csvWriter = new CsvWriter(streamWriter);
                string[] fileHeaders = { "EisSku", "Price" };

                foreach (var header in fileHeaders)
                    csvWriter.WriteField(header);

                csvWriter.NextRecord();

                foreach (var item in productList)
                {
                    string eisSku = item.EisSKU;
                    decimal? price = 0;

                    if (customer.AccountType == (int)CustomerAccountTypeEnum.Retail)
                    {
                        price = item.SellerPrice;
                    }
                    else
                    {
                        if (customer.CostPlusBasedWholeSalePriceType == (int)CostPlusBasedWholeSalePriceTypeEnum.SellingPrice)
                        {
                            price = CalculatePrice(item.SellerPrice, (int)customer.AmountType, (decimal)customer.CostPlusBasedWholeSalePrice);

                        }
                        else if (customer.CostPlusBasedWholeSalePriceType == (int)CostPlusBasedWholeSalePriceTypeEnum.SupplierPrice)
                        {
                            price = CalculatePrice(item.SupplierPrice, (int)customer.AmountType, (decimal)customer.CostPlusBasedWholeSalePrice);
                        }
                    }

                    csvWriter.WriteField(eisSku);
                    csvWriter.WriteField(price);

                    csvWriter.NextRecord();

                    SaveValuesInCustomerWholeSalePriceHistoryTable(item.EisSKU, customer.CustomerId, _task.Id, price.Value);
                }
            }
            Console.WriteLine("Done creating {0} file -> {1}", TaskType, _generatedFile);
            return true;
        }

        private void SaveValuesInCustomerWholeSalePriceHistoryTable(string eisSKU, int customerId, int id, decimal price)
        {

            using (var context = new EisInventoryContext())
            {
                var model = new customerwholesalepricehistory
                {
                    Created = DateTime.Now,
                    CustomerId = customerId,
                    EisSKU = eisSKU,
                    SkuCalculatedPrice = price,
                    CustomerScheduleId = id
                };

                context.customerwholesalepricehistories.Add(model);
                context.SaveChanges();
            }
        }

        private decimal CalculatePrice(decimal? productPrice, int amountType, decimal costPlusBasedWholeSalePrice)
        {
            if (productPrice == null) return 0;

            decimal price = 0;
            if (amountType == (int)AmountTypeEnum.Amount)
            {
                price = productPrice.Value + costPlusBasedWholeSalePrice;
            }
            else
            {
                var percentageValue = costPlusBasedWholeSalePrice / 100;
                percentageValue = productPrice.Value * percentageValue;

                price = productPrice.Value + percentageValue;
            }
            return price;
        }

        private customer GetCustomer(int customerId)
        {
            using (var context = new EisInventoryContext())
            {
                return context.customers.First(x => x.CustomerId == customerId);
            }
        }

        private List<productList> GetProductFromEisSkuList(List<string> skuList)
        {
            var collection = new List<productList>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                string query = string.Format(@"SELECT 
                                p.EisSKU
                                ,(SELECT CASE WHEN p.SkuType = 1 THEN ROUND((vwa.SupplierPrice * (s.FactorQuantity / vwa.MinPack)), 2) ELSE 
                                vwa.SupplierPrice END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS SupplierPrice
                                ,p.SellerPrice
                                FROM products p 
                                LEFT JOIN shadows s ON s.ShadowSKU = p.EisSKU AND s.IsConnected = 1
                                WHERE p.EisSKU in ({0})", string.Join(",", skuList.Select(x => string.Format("'{0}'", x))));

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, query.ToString(), null);
                while (reader.Read())
                {
                    collection.Add(new productList
                    {
                        EisSKU = reader["EisSKU"].ToString(),
                        SellerPrice = reader["SellerPrice"] as decimal?,
                        SupplierPrice = reader["SupplierPrice"] as decimal?
                    });
                }
            }
            return collection;
        }
    }

    public class productList
    {
        public string EisSKU { get; set; }
        public decimal? SupplierPrice { get; set; }
        public decimal? SellerPrice { get; set; }
    }
}