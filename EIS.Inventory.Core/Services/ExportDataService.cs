using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using MySql.Data.MySqlClient;

namespace EIS.Inventory.Core.Services
{
    public class ExportDataService : IExportDataService
    {
        private readonly string _connectionString;
        private readonly string _exportFolder;
        private readonly ILogService _logger;
        private readonly IImageHelper _imageHelper;
        private readonly EisInventoryContext _context;

        public ExportDataService(IImageHelper imageHelper, ILogService logger)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            _exportFolder = ConfigurationManager.AppSettings["ExportedFilesRoot"].ToString();
            _context = new EisInventoryContext();
            _imageHelper = imageHelper;
            _logger = logger;
        }

        public string CustomExportVendorProducts(ExportVendorProduct model)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", model.VendorId == null ? -1 : model.VendorId},
                {"@CompanyId", model.CompanyId == null ? -1 : model.CompanyId},
                {"@QuantityFrom", model.QuantityFrom == null ? -1 : model.QuantityFrom},
                {"@QuantityTo", model.QuantityTo == null ? -1 : model.QuantityTo},
            };

            var filePath = string.Format("{1}\\{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportVendorProducts.csv", model.RequestedDate, _exportFolder);
            try
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, createExportVendorProductSqlQuery(model), parameters);
                        var eisSKULinks = getVendorProductLinks(model);

                        var productImages = getVendorProductImages(model);

                        var config = new CsvConfiguration { Delimiter = model.Delimiter };
                        var csvWriter = new CsvWriter(streamWriter, config);

                        // write the header text for the CSV files
                        foreach (var field in model.ProductFieldsArr)
                        {
                            var headerName = removePrefixTable(field, true);
                            csvWriter.WriteField(headerName.FileHeaderName);
                        }

                        // write the column headers for the EisSKU links; ATLEAST 5 - NEED TO REFACTOR THIS ONE
                        if (model.IsIncludeEisSKULinks)
                        {
                            for (var i = 1; i <= 5; i++)
                                csvWriter.WriteField(string.Format("EisSKU_{0}", i));
                        }
                        // write the column headers for the product's images
                        if (model.IsIncludeImages)
                        {
                            for (var i = 1; i <= 5; i++)
                                csvWriter.WriteField(string.Format("ImageUrl{0}", i));
                        }
                        csvWriter.NextRecord();

                        while (reader.Read())
                        {
                            // write the vendor product results
                            foreach (var field in model.ProductFieldsArr)
                            {
                                var headerName = removePrefixTable(field);
                                csvWriter.WriteField(reader[headerName.DbColumnName]);
                            }

                            // write the EisSKU links
                            if (model.IsIncludeEisSKULinks)
                            {
                                // get the EIS SKU
                                var eisSupplierSKU = reader["EisSupplierSKU"].ToString();
                                var links = eisSKULinks.Where(x => x.EisSupplierSKU == eisSupplierSKU).ToList();
                                foreach (var link in links)
                                    csvWriter.WriteField(link.EisSKU);
                            }
                            if (model.IsIncludeImages)
                            {
                                var eisSku = reader["EisSupplierSKU"].ToString();
                                var images = productImages.Where(x => x.EisSupplierSKU == eisSku).ToList();

                                for (var i = 0; i < images.Count && i < 5; i++)
                                {
                                    csvWriter.WriteField(_imageHelper.GetVendorProductImageUri(eisSku, images[i].FileName));
                                }
                            }
                            csvWriter.NextRecord();
                        }
                    }
                }

                _logger.Add(LogEntrySeverity.Information, LogEntryType.ExportDataService, "Vendor products have been successfully exported -> " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.ExportDataService,
                    string.Format("Error in custom export vendor product file. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                throw ex;
            }
        }

        public string CustomExportProducts(ExportProduct model)
        {
            var filePath = string.Format("{1}\\{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportProducts.csv", model.RequestedDate, _exportFolder);
            try
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, createProductQuery(model), null);
                        var productImages = getProductImages(model);

                        var config = new CsvConfiguration();
                        config.Delimiter = model.Delimiter;
                        var csvWriter = new CsvWriter(streamWriter, config);

                        // write the header text for the CSV files
                        foreach (var field in model.ProductFieldsArr)
                        {
                            var headerName = removePrefixTable(field);
                            csvWriter.WriteField(headerName.FileHeaderName);
                        }

                        // write the column headers for the product's images
                        if (model.IsIncludeImages)
                        {
                            for (var i = 1; i <= 5; i++)
                                csvWriter.WriteField(string.Format("ImageUrl{0}", i));
                        }

                        csvWriter.NextRecord();

                        while (reader.Read())
                        {
                            foreach (var field in model.ProductFieldsArr)
                            {
                                var headerName = removePrefixTable(field);
                                csvWriter.WriteField(reader[headerName.DbColumnName]);
                            }

                            // write the image URLs if selected
                            if (model.IsIncludeImages)
                            {
                                // get the EIS SKU
                                var eisSku = reader["EisSKU"].ToString();
                                var images = productImages.Where(x => x.EisSKU == eisSku).ToList();

                                for (var i = 0; i < images.Count && i < 5; i++)
                                {
                                    csvWriter.WriteField(_imageHelper.GetProductImageUri(eisSku, images[i].FileName));
                                }
                            }

                            csvWriter.NextRecord();
                        }
                    }
                }

                _logger.Add(LogEntrySeverity.Information, LogEntryType.ExportDataService, "Products have been successfully exported -> " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.ExportDataService,
                    string.Format("Error in custom export product file. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                throw ex;
            }
        }

        private ColumnHeader removePrefixTable(string columnName, bool isUseFriendlyName = false)
        {
            var result = columnName.Split('.');

            return new ColumnHeader(result[0], result[1], isUseFriendlyName);
        }

        private string createExportVendorProductSqlQuery(ExportVendorProduct model)
        {
            var sqlQuery = string.Empty;

            if (model.IsAllProductItems)
            {
                var whereSb = new StringBuilder();
                if (!string.IsNullOrEmpty(model.SearchString))
                {
                    whereSb.AppendFormat("(vendorproducts.EisSupplierSKU LIKE \'%{0}%\' OR vendorproducts.SupplierSKU LIKE \'%{0}%\' OR vendorproducts.Category LIKE \'%{0}%\' OR vendorproducts.Name LIKE \'%{0}%\') AND ", model.SearchString.Replace("_", "\\_"));
                }

                whereSb.Append("(@VendorId = -1 OR  vendorproducts.VendorId = @VendorId) ");
                whereSb.Append("AND (@CompanyId = -1 OR  vendors.CompanyId = @CompanyId) ");
                whereSb.Append("AND ((@QuantityFrom = -1 OR @QuantityTo = -1) OR (vendorproducts.Quantity >= @QuantityFrom AND vendorproducts.Quantity <= @QuantityTo)) ");

                // add criteria if we want to include EisSKU links
                if (model.IsWithEisSKULink)
                    whereSb.AppendFormat("AND vendorproducts.EisSupplierSKU {0} IN (SELECT DISTINCT EisSupplierSKU FROM vendorproductlinks WHERE IsActive = 1) ", (model.WithEisSKULink == 1 ? "" : "NOT"));

                // add criteria if it's we want to include images
                if (model.IsWithImages)
                    whereSb.Append(string.Format(" AND vendorproducts.EisSupplierSKU {0} IN (SELECT DISTINCT EisSupplierSKU FROM vendorproductimages) ", model.WithImages == 1 ? "" : "NOT"));


                sqlQuery = string.Format(@"SELECT vendorproducts.EisSupplierSKU,vendorproducts.VendorId, {0} 
                                FROM vendorproducts vendorproducts 
                                INNER JOIN vendors vendors ON vendors.Id = vendorproducts.VendorId
                                WHERE {1} {2}",
                    string.Join(",", model.ProductFieldsArr),
                    whereSb.ToString(),
                    (string.IsNullOrEmpty(model.SortBy) ? string.Empty : ("ORDER BY " + model.SortBy)));

            }
            else // only selected vendor product's EisSupplierSKUs
            {
                sqlQuery = string.Format(@"SELECT vendorproducts.EisSupplierSKU,vendorproducts.VendorId,{0}
                                FROM vendorproducts vendorproducts
                                INNER JOIN vendors vendors ON vendors.Id = vendorproducts.VendorId
                                WHERE vendorproducts.EisSupplierSKU IN ('{1}') {2}",
                    string.Join(",", model.ProductFieldsArr),
                    string.Join("','", model.SelectedEisSKUsArr),
                    (string.IsNullOrEmpty(model.SortBy) ? string.Empty : ("ORDER BY " + model.SortBy)));
            }

            return sqlQuery;
        }

        private string createProductQuery(ExportProduct model)
        {
            var sb = new StringBuilder(@"
                SELECT 
                    {0}
                FROM products p 
                LEFT JOIN shadows s ON s.ShadowSKU = p.EisSKU AND s.IsConnected = 1
                LEFT JOIN productamazons pa ON pa.EisSKU = p.EisSKU
                LEFT JOIN productebays pe ON pe.EisSKU = p.EisSKU
                LEFT JOIN productbigcommerces pbc ON pbc.EisSKU = p.EisSKU
                WHERE 1 = 1 ");

            if (!string.IsNullOrEmpty(model.SearchString))
                sb.AppendFormat(@" AND (p.EisSKU LIKE '%{0}%' OR p.UPC LIKE '%{0}%' OR p.Name LIKE '%{0}%' OR p.Category LIKE '%{0}%')", model.SearchString.Replace("_", "\\_"));

            if (model.ProductGroupId != -1 && model.ProductGroupId != null)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT EisSKU FROM productgroups WHERE Id = {0})", model.ProductGroupId);

            if (model.VendorId != -1 && model.VendorId != null)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT DISTINCT EisSKU FROM vw_availablevendorproducts WHERE VendorId = {0})", model.VendorId);

            if (model.IsQuantityFromSet && model.IsQuantityToSet)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT DISTINCT EisSKU FROM vw_availablevendorproducts WHERE Quantity >= {0} AND Quantity <= {1})", model.QuantityFrom ?? -1, model.QuantityTo ?? -1);

            if (model.IsWithImages)
                sb.AppendFormat(" AND p.EisSKU {0} IN (SELECT DISTINCT EisSKU FROM productimages)", model.WithImages == 1 ? "" : "NOT");

            if (model.IsSKULinked.HasValue)
                sb.AppendFormat(" AND p.EisSKU {0} IN (SELECT DISTINCT EisSKU FROM vendorproductlinks)", model.IsSKULinked.Value ? "" : "NOT");

            if (model.CompanyId != -1 && model.CompanyId != null)
                sb.AppendFormat(" AND p.CompanyId = {0}", model.CompanyId);

            if (model.SkuType.HasValue)
                sb.AppendFormat(" AND p.SkuType = {0}", (int)model.SkuType);

            if (model.IsKit.HasValue)
                sb.AppendFormat(" AND p.IsKit = {0}", model.IsKit.Value ? 1 : 0);

            if (model.IsAmazonEnabled.HasValue)
                sb.AppendFormat(" AND pa.IsEnabled = {0}", model.IsAmazonEnabled.Value ? 1 : 0);

            if (model.HasASIN != null)
                sb.AppendFormat(" AND pa.ASIN IS {0} NULL", model.HasASIN.Value ? "NOT" : "");

            // apply the order if there's any
            if (!string.IsNullOrEmpty(model.SortBy))
                sb.AppendFormat(" ORDER BY {0}", getParsedOrderBy(model.SortBy));

            return string.Format(sb.ToString(), getParsedProductColumns(model.ProductFieldsArr));
        }

        private List<productimage> getProductImages(ExportProduct model)
        {
            var images = new List<productimage>();
            if (!model.IsIncludeImages)
                return null;

            if (model.IsAllProductItems)
            {
                images = _context.productimages
                    .Where(x => x.ImageType == "CUSTOM" || x.ImageType == "LARGE")
                    .ToList();
            }
            else
            {
                images = _context.productimages.Where(x => model.SelectedEisSKUsArr.Contains(x.EisSKU)
                    && (x.ImageType == "CUSTOM" || x.ImageType == "LARGE"))
                    .ToList();
            }

            return images;
        }

        private List<vendorproductimage> getVendorProductImages(ExportVendorProduct model)
        {
            var images = new List<vendorproductimage>();
            //if (!model.IsIncludeImages)
            //    return null;

            if (model.IsAllProductItems)
            {
                images = _context.vendorproductimages.ToList();
            }
            else
            {
                images = _context.vendorproductimages.Where(x => model.SelectedEisSKUsArr.Contains(x.EisSupplierSKU)).ToList();
            }

            return images;
        }

        private List<vendorproductlink> getVendorProductLinks(ExportVendorProduct model)
        {
            if (!model.IsIncludeEisSKULinks)
                return null;

            if (model.IsAllProductItems)
                return _context.vendorproductlinks.Where(x => x.IsActive).ToList();
            else
                return _context.vendorproductlinks
                    .Where(x => model.SelectedEisSKUsArr.Contains(x.EisSupplierSKU) && x.IsActive)
                    .ToList();
        }

        private string getParsedProductColumns(List<string> selectedProductFields)
        {
            var sb = new StringBuilder();
            foreach (var field in selectedProductFields)
            {
                if (field == "vendor_product.EisSupplierSKU")
                    sb.Append("(SELECT vwa.EisSupplierSKU FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS EisSupplierSKU");
                else if (field == "vendor_product.Quantity")
                    sb.Append("(SELECT CASE WHEN p.SkuType = 1 THEN FLOOR((vwa.Quantity * vwa.MinPack) / s.FactorQuantity) ELSE vwa.Quantity END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS Quantity");
                else if (field == "vendor_product.SupplierPrice")
                    sb.Append("(SELECT CASE WHEN p.SkuType = 1 THEN ROUND((vwa.SupplierPrice * (s.FactorQuantity / vwa.MinPack)), 2) ELSE vwa.SupplierPrice END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS SupplierPrice");
                else
                    sb.Append(field);

                // add comma at the end
                sb.Append(",");
            }
            return sb.ToString().TrimEnd(',');
        }

        public string CustomExportShippingRates(string selectedIds, DateTime currentDateTime)
        {
            var filePath = string.Format("{1}\\{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportShippingRates.csv", currentDateTime, _exportFolder);
            try
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, createExportShippingRatesSqlQuery(selectedIds), null);

                        var config = new CsvConfiguration();
                        var csvWriter = new CsvWriter(streamWriter, config);

                        var headers = "ID,WeightFrom,WeightTo,Unit,Rate".Split(',');

                        // write the header text for the CSV files
                        foreach (var field in headers)
                        {
                            //var headerName = removePrefixTable(field);
                            csvWriter.WriteField(field);
                        }

                        csvWriter.NextRecord();

                        while (reader.Read())
                        {
                            foreach (var field in headers)
                            {
                                //var headerName = removePrefixTable(field);
                                csvWriter.WriteField(reader[field]);
                            }

                            csvWriter.NextRecord();
                        }
                    }
                }

                _logger.Add(LogEntrySeverity.Information, LogEntryType.ExportDataService, "Shipping rates have been successfully exported -> " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.ExportDataService,
                    string.Format("Error in custom export shipping rate file. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                throw ex;
            }



            throw new NotImplementedException();

        }


        public string ExportBigCommerceCategories()
        {
            var filePath = string.Format("{1}\\{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportBigCommerceCategories.csv", DateTime.Now, _exportFolder);

            try
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    var config = new CsvConfiguration();
                    config.Delimiter = ",";
                    var csvWriter = new CsvWriter(streamWriter, config);

                    // write the header text for the CSV files
                    csvWriter.WriteField("Category ID");
                    csvWriter.WriteField("Category Name");

                    csvWriter.NextRecord();

                    var bcService = new BigCommerceService();
                    var bcCategories = bcService.GetEISCategoryList().ToList();
                    var bcCategoriesOrderedList = bcService.GetCategoryOrderedList(bcCategories);

                    foreach (var category in bcCategoriesOrderedList)
                    {
                        csvWriter.WriteField(category.Id);
                        csvWriter.WriteField(category.Name);

                        csvWriter.NextRecord();
                    }
                }

                _logger.Add(LogEntrySeverity.Information, LogEntryType.ExportDataService, "BigCommerece Categories have been successfully exported -> " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.ExportDataService,
                    string.Format("Error in export BigCommerce Categories file. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                throw ex;
            }
        }
              

        private string createExportShippingRatesSqlQuery(string selectedIds)
        {
            var sqlQuery = string.Empty;


            sqlQuery = string.Format(@"SELECT ID,WeightFrom,WeightTo,Unit,Rate
                                FROM shippingrates
                                WHERE ID IN ({0})", selectedIds);
            return sqlQuery;
        }

        private string getParsedOrderBy(string sortBy)
        {
            if (sortBy == "vendor_product.EisSupplierSKU")
                return "EisSupplierSKU";
            else if (sortBy == "vendor_product.Quantity")
                return "Quantity";
            else if (sortBy == "vendor_product.SupplierPrice")
                return "SupplierPrice";
            else
                return sortBy;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.Dispose();
                _context.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }

        #endregion
    }
}