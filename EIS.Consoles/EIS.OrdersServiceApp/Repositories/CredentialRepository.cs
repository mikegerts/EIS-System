using System;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.OrdersServiceApp.Models;

namespace EIS.OrdersServiceApp.Repositories
{
    public class CredentialRepository
    {
        public Credential GetDefaultCredential(string marketplaceType, string mode)
        {
            Credential credential = null;
            using (var context = new EisInventoryContext())
            {
                var result = context.credentials
                    .FirstOrDefault(x => x.MarketplaceType == marketplaceType && x.Mode == mode && x.IsDefault);
                if (result == null)
                    return null;

                credential = convertToModel(result);
            }

            return credential;
        }

        private Credential convertToModel(credential credential)
        {
            Credential model = null;
            if (credential is amazoncredential)
            {
                var item = credential as amazoncredential;
                model = new AmazonCredential
                {
                    AccessKeyId = item.AccessKeyId,
                    AssociateId = item.AssociateId,
                    CompanyId = item.CompanyId,
                    Id = item.Id,
                    IsDefault = item.IsDefault,
                    IsEnabled = item.IsEnabled,
                    MarketplaceId = item.MarketplaceId,
                    MarketplaceType = item.MarketplaceType,
                    MerchantId = item.MerchantId,
                    Mode = item.Mode,
                    Name = item.Name,
                    SearchAccessKeyId = item.SearchAccessKeyId,
                    SearchSecretKey = item.SearchSecretKey,
                    SecretKey = item.SecretKey,
                    ServiceEndPoint = item.ServiceEndPoint
                };
            }
            else if (credential is ebaycredential)
            {
                var item = credential as ebaycredential;
                model = new eBayCredential
                {
                    ApplicationId = item.ApplicationId,
                    DeveloperId = item.DeveloperId,
                    CertificationId = item.CertificationId,
                    UserToken = item.UserToken,
                    CompanyId = item.CompanyId,
                    Id = item.Id,
                    IsDefault = item.IsDefault,
                    IsEnabled = item.IsEnabled,
                    MarketplaceId = item.MarketplaceId,
                    MarketplaceType = item.MarketplaceType,
                    Mode = item.Mode,
                    Name = item.Name,
                    ServiceEndPoint = item.ServiceEndPoint
                };
            }
            else if (credential is bigcommercecredential)
            {
                var item = credential as bigcommercecredential;
                model = new BigCommerceCredential
                {
                    Username = item.Username,
                    ApiKey = item.ApiKey,
                    CompanyId = item.CompanyId,
                    Id = item.Id,
                    IsDefault = item.IsDefault,
                    IsEnabled = item.IsEnabled,
                    MarketplaceId = item.MarketplaceId,
                    MarketplaceType = item.MarketplaceType,
                    Mode = item.Mode,
                    Name = item.Name,
                    ServiceEndPoint = item.ServiceEndPoint
                };
            }
            else
                throw new InvalidCastException(string.Format("Unknown credential type \'{0}\' for casting!", credential.MarketplaceType));

            return model;
        }
    }
}
