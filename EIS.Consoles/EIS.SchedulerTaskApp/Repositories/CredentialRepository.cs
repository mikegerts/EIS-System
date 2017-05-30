using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using AutoMapper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;
using EIS.SchedulerTaskApp.Models;
using MySql.Data.MySqlClient;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class CredentialRepository
    {
        private string _inventoryConnectionString;

        public CredentialRepository()
        {
            _inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public List<MarketplaceInventoryUpdateItem> GetProductsByCompany(int companyId)
        {
            var results = new List<MarketplaceInventoryUpdateItem>();
            var parameters = new Dictionary<string, object>
            {
                {"@CompanyId", companyId }
            };

            using (var conn = new MySqlConnection(_inventoryConnectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                      @"SELECT  
	                        (@RowNumber := CASE WHEN @EisSKU = p.EisSKU THEN @RowNumber + 1 ELSE 1 END) AS RowNumber,
                            (@EisSKU := p.EisSKU) AS EisSKU,
                            vp.Quantity,
                            pa.LeadtimeShip,
                            v.SafeQty
                        FROM products p
                        INNER JOIN productamazONs pa ON pa.EisSKU = p.EisSKU
                        INNER JOIN companies c	ON c.Id = p.CompanyId
                        INNER JOIN vendorproductlinks l	ON l.EisSKU = p.EisSKU
                        INNER JOIN vendorproducts vp ON vp.EisSupplierSKU = l.EisSupplierSKU                        
                        INNER JOIN vendors v ON v.Id = vp.VendorId
                        WHERE c.Id = @CompanyId AND vp.Quantity > 0 AND p.IsBlacklisted = 0 AND pa.IsEnabled = 1
                        HAVING RowNumber = 1
                        ORDER BY p.EisSKU", parameters);

                while (reader.Read())
                {
                    var item = new MarketplaceInventoryUpdateItem();
                    item.SKU = reader[1].ToString();
                    item.Quantity = (int)reader[2];
                    item.LeadtimeShip = reader[3] == DBNull.Value ? 3 : Convert.ToInt32(reader[3]);
                    item.SafeQty = reader[4] as int?;
                    results.Add(item);
                }
            }

            return results;
        }

        public CredentialDto GetCredentialsByCompany(int companyId, string credentialType, string mode)
        {
            var credential = new CredentialDto();
            using(var context = new EisInventoryContext())
            {
                var result = context.credentials
                    .FirstOrDefault(x => x.CompanyId == companyId && x.MarketplaceType == credentialType && x.Mode == mode);
                if (result == null)
                    return null;

                credential = convertToModel(result);                
            }

            return credential;
        }

        private CredentialDto convertToModel(credential credential)
        {
            CredentialDto model = null;
            if (credential is amazoncredential)
                model = Mapper.Map<amazoncredential, AmazonCredentialDto>(credential as amazoncredential);
            else if (credential is ebaycredential)
                model = Mapper.Map<ebaycredential, eBayCredentialDto>(credential as ebaycredential);
            else if (credential is shipstationcredential)
                model = Mapper.Map<shipstationcredential, ShipStationCredentialDto>(credential as shipstationcredential);
            else if (credential is bigcommercecredential)
                model = Mapper.Map<bigcommercecredential, BigCommerceCredentialDto>(credential as bigcommercecredential);
            else
                throw new InvalidCastException(string.Format("Unknown credential type \'{0}\' for casting!", credential.MarketplaceType));

            return model;
        }
    }
}
