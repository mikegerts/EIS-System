using System.Collections.Generic;
using EIS.ConsoleApp.Models;
using MySql.Data.MySqlClient;

namespace EIS.ConsoleApp.Repositories
{
    public class ProductImageRepository
    {
        private readonly string _connectionString;
        private MySqlConnection _conn;

        public ProductImageRepository(string connectionString)
        {
            _connectionString = connectionString;
            _conn = new MySqlConnection(_connectionString);
        }

        public void CloseDbConnection()
        {
            if (_conn != null)
                _conn.CloseAsync();
        }

        public void InsertImage(ProductImage image)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@VendorProductId", image.VendorProductId},
                {"@ImagePath", image.ImagePath},
                {"@ImageType", image.ImageType}
            };
        }
    }
}
