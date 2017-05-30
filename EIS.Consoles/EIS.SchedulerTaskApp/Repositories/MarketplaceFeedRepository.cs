using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Repositories
{
    public class MarketplaceFeedRepository
    {

        public List<MarketplaceInventoryFeed> GetProductsByCompany(int companyId)
        {
            var results = new List<MarketplaceInventoryFeed>();
            using(var context = new EisInventoryContext())
            {
                // get all the company EIS products
                var products = context.products
                    .Where(x => x.CompanyId == companyId
                       // && (x.productamazon != null && x.productamazon.IsEnabled)
                        && !x.IsBlacklisted)
                .ToList();

                results = Mapper.Map<List<MarketplaceInventoryFeed>>(products);
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
