using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using EIS.DataUploaderApp.Helpers;
using EIS.DataUploaderApp.Models;
using MySql.Data.MySqlClient;

namespace EIS.DataUploaderApp.Repositories
{
    public class ProductRepository
    {
        private readonly string _vendorConnectionString;
        private readonly string _inventoryConnectionString;
        private MySqlConnection _vendorConn;
        private MySqlConnection _inventoryConn;

        public ProductRepository(string connectionString)
        {
            _vendorConnectionString = connectionString;
            _inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            _vendorConn = new MySqlConnection(_vendorConnectionString);            
        }

        public ProductRepository(string connectionString, bool initInventoryConn) : this(connectionString)
        {
            if(initInventoryConn)
                _inventoryConn = new MySqlConnection(_inventoryConnectionString);
        }

        public void CloseDbConnection()
        {
            if (_vendorConn != null)
                _vendorConn.CloseAsync();

            if (_inventoryConn != null)
                _inventoryConn.CloseAsync();
        }

        public void CreateVendorProduct(VendorProduct product)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", product.VendorId},
                {"@SKU", product.SKU},
                {"@Name", product.Name},
                {"@Description", product.Description},
                {"@ShortDescription", product.ShortDescription},
                {"@Category",product.Category},
                {"@UPCCode",product.UPCCode},
                {"@Quantity",product.Quantity},
                {"@Cost", product.Cost},
                {"@ResultDate", product.ResultDate}
            };

            MySqlHelper.ExecuteNonQuery(_vendorConn, @"INSERT INTO vendorproducts(VendorId,SKU,Name,Description,ShortDescription,Category,UPCCode,Quantity,Cost,ResultDate)
                VALUES(@VendorId,@SKU,@Name,@Description,@ShortDescription,@Category,@UPCCode,@Quantity,@Cost,@ResultDate)", parameters);
        }

        public IQueryable<VendorProduct> GetVendorProducts(long vendorId, DateTime resultDate)
        {
            var products = new List<VendorProduct>();            
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", vendorId},
                {"@ResultDate", resultDate.Date}
            };
            var reader = MySqlHelper.ExecuteReader(_vendorConn, CommandType.Text,
                @"SELECT Id,VendorId,SKU,Name,Description,ShortDescription,Category,UPCCode,Quantity,Cost,ResultDate FROM vendorproducts
                WHERE VendorId=@VendorId and ResultDate=@ResultDate", parameters);

            while (reader.Read())
            {
                var product = new VendorProduct();
                product.Id = (long)reader[0];
                product.VendorId = (int)reader[1];
                product.SKU = reader[2].ToString();
                product.Name = reader[3].ToString();
                product.Description = reader[4].ToString();
                product.ShortDescription = reader[5].ToString();
                product.Category = reader[6].ToString();
                product.UPCCode = reader[7].ToString();
                product.Quantity = (int)reader[8];
                product.Cost = (decimal)reader[9];
                product.ResultDate = reader[10] == DBNull.Value ? default(DateTime) : (DateTime)reader[10];

                products.Add(product);
            }

            return products.AsQueryable();
        }

        public void DeleteVendorProducts(long vendorId, DateTime resultDate)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorId", vendorId},
                {"@ResultDate", resultDate.Date}
            };

            MySqlHelper.ExecuteNonQuery(_vendorConn, @"DELETE FROM vendorproducts WHERE VendorId=@VendorId and ResultDate=@ResultDate", parameters);
        }

        public void UpdateMasterProduct(VendorProduct product)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@SupplierSKU", product.SKU},
                {"@VendorId", product.VendorId},
                {"@Name", product.Name},
                {"@Description", product.Description},
                {"@ShortDescription", product.ShortDescription},
                {"@Category",product.Category },
                {"@UPCCode",product.UPCCode },
                {"@Quantity",product.Quantity},
                {"@SupplierPrice", product.Cost},
                {"@UserName", "EIS Uploader"},
                {"@Modified", DateTime.Now}
            };

            MySqlHelper.ExecuteNonQuery(_inventoryConn,
                @"UPDATE products
                SET Name=@Name,Description=@Description,ShortDescription=@ShortDescription,Category=@Category,UPCCode=@UPCCode,Quantity=@Quantity,SupplierPrice=@SupplierPrice,UserName=@UserName,Modified=@Modified
                WHERE SupplierSKU=@SupplierSKU AND VendorId=@VendorId", parameters);
        }

        public void UpdateMasterProductFromInventory(VendorProduct product)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@SupplierSKU", product.SKU},
                {"@VendorId", product.VendorId},
                {"@Name", product.Name},
                {"@Description", product.Description},
                {"@ShortDescription", product.ShortDescription},
                {"@Category",product.Category },
                {"@UPCCode",product.UPCCode },
                {"@Quantity",product.Quantity },
                {"@SupplierPrice", product.Cost},
                {"@UserName", "EIS Uploader"},
                {"@Modified", DateTime.Now}
            };

            var sb = new StringBuilder("UPDATE products SET ");

            if (!string.IsNullOrEmpty(product.Name))
                sb.Append("Name=@Name,");

            if (!string.IsNullOrEmpty(product.Description))
                sb.Append("Description=@Description,");

            if (!string.IsNullOrEmpty(product.ShortDescription))
                sb.Append("ShortDescription=@ShortDescription,");

            if (!string.IsNullOrEmpty(product.Category))
                sb.Append("Category=@Category,");

            if (!string.IsNullOrEmpty(product.UPCCode))
                sb.Append("UPCCode=@UPCCode,");

            if (product.Quantity != -1)
                sb.Append("Quantity=@Quantity,");

            if (product.Cost != -1)
                sb.Append("SupplierPrice=@SupplierPrice,");

            // remove the ',' at the end of string builder
            var fieldsStr = sb.ToString();
            var query = string.Format("{0}  WHERE SupplierSKU=@SupplierSKU AND VendorId=@VendorId",
                fieldsStr.Remove(fieldsStr.Length - 1));

            MySqlHelper.ExecuteNonQuery(_inventoryConn, query, parameters);
        }

        public void CreateMasterProduct(VendorProduct product, string startSkuCode)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@EisSKU", string.Format("{0}{1}", startSkuCode, product.SKU)},
                {"@SupplierSKU", product.SKU},
                {"@VendorId", product.VendorId},
                {"@Name", product.Name},
                {"@Description", product.Description},
                {"@ShortDescription", product.ShortDescription},
                {"@Category",product.Category},
                {"@UPCCode",product.UPCCode},
                {"@Quantity",product.Quantity},
                {"@SupplierPrice", product.Cost},
                {"@UserName", "EIS Uploader"},
                {"@Created", DateTime.Now}
            };

            MySqlHelper.ExecuteNonQuery(_inventoryConn,
                @"INSERT INTO products(EisSKU,SupplierSKU,VendorId,Name,Description,ShortDescription,Category,UPCCode,Quantity,SupplierPrice,SellerPrice,UserName,Created)
                VALUES(@EisSKU,@SupplierSKU,@VendorId,@Name,@Description,@ShortDescription,@Category,@UPCCode,@Quantity,@SupplierPrice,@SupplierPrice,@UserName,@Created)", parameters);
        }

        public void CreateUnMappedProduct(VendorProduct product)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@SupplierSKU", product.SKU},
                {"@VendorId", product.VendorId},
                {"@Name", product.Name},
                {"@Description", product.Description},
                {"@ShortDescription", product.ShortDescription},
                {"@Category",product.Category },
                {"@UPCCode",product.UPCCode},
                {"@Quantity", product.Quantity},
                {"@SupplierPrice", product.Cost},
                {"@ResultDate", product.ResultDate}
            };

            MySqlHelper.ExecuteNonQuery(_inventoryConn,
                @"INSERT INTO unmappedproducts(SupplierSKU,VendorId,Name,Description,ShortDescription,Category,UPCCode,Quantity,SupplierPrice,SellerPrice,ResultDate)
                VALUES(@SupplierSKU,@VendorId,@Name,@Description,@ShortDescription,@Category,@UPCCode,@Quantity,@SupplierPrice,@SupplierPrice,@ResultDate)", parameters);
        }

        public string GetNextEisSku(long vendorId)
        {
            var maxEisSku = string.Empty;
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@VendorId", vendorId},
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT max(EisSKU) FROM products WHERE VendorId=@VendorId", parameters);
                
                while (reader.Read())
                    maxEisSku = reader[0].ToString();
            }

            return CodeGenerator.GetNextCode(maxEisSku);
        }

        public string GetVendorStartEisSkuCode(long id)
        {
            var startSkuCode = string.Empty;
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Id", id},
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT SKUCodeStart FROM vendors WHERE Id=@Id", parameters);

                while (reader.Read())
                    startSkuCode = reader[0].ToString();
            }

            return startSkuCode;
        }

        public bool IsProductExist(int vendorId, string supplierSKU)
        {
            var isExist = false;
            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@SupplierSKU", supplierSKU},
                    {"@VendorId", vendorId},
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT 1 FROM products WHERE SupplierSKU=@SupplierSKU AND VendorId=@VendorId", parameters);
                
                while (reader.Read())
                    isExist = reader[0] != DBNull.Value;
            }

            return isExist;
        }
    }
}
