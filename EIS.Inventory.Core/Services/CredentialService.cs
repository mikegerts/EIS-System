using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly EisInventoryContext _context;

        public CredentialService()
        {
            _context = new EisInventoryContext();
        }

        public IEnumerable<CredentialDto> GetCredentials()
        {
            var results = _context.credentials
                .ProjectTo<CredentialDto>()
                .OrderBy(x => x.Id);

            return results;
        }

        public IEnumerable<CredentialDto> GetCredentialsByCompany(int companyId)
        {
            var results = _context.credentials
                .Where(x => x.CompanyId == companyId);

            return Mapper.Map<IEnumerable<CredentialDto>>(results);
        }

        public CredentialDto GetCredential(int id)
        {
            var result = _context.credentials
                .FirstOrDefault(x => x.Id == id);

            return convertToModel(result);
        }

        public CredentialDto GetCredential(string crendentialType, string mode)
        {
            var result = _context.credentials
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault(x => x.MarketplaceType.Equals(crendentialType)
                    && mode.Equals(x.Mode)
                    && x.IsEnabled);
            if (result == null)
                return null;

            return convertToModel(result);
        }

        public CredentialDto GetCredential(string crendentialType, int companyId, string mode)
        {
            var result = _context.credentials
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault(x => x.MarketplaceType.Equals(crendentialType)
                    && x.CompanyId == companyId
                    && x.Mode.Equals(mode)
                    && x.IsEnabled);
            if (result == null)
                return null;

            return convertToModel(result);
        }

        public bool CreateCredential(CredentialDto model)
        {
            // unbox the correct object type for the credential
            var credential = convertToDomainObject(model);
            credential.CreatedBy = model.ModifiedBy;
            credential.Created = DateTime.UtcNow;

            _context.credentials.Add(credential);
            _context.SaveChanges();
            model.Id = credential.Id;

            return true;
        }

        public bool UpdateCredential(int id, CredentialDto model)
        {
            // get the curent credential
            var existingCredential = _context.credentials.FirstOrDefault(x => x.Id == id);

            // unbox to the correct object for crendential
            var updatedCredential = convertToDomainObject(model);
            updatedCredential.ModifiedBy = model.ModifiedBy;
            updatedCredential.Modified = DateTime.UtcNow;

            _context.Entry(existingCredential).CurrentValues.SetValues(updatedCredential);
            _context.SaveChanges();

            return true;
        }

        public bool DeleteCredential(int id)
        {
            var crendential = _context.credentials.FirstOrDefault(x => x.Id == id);
            if (crendential == null)
                return true;

            _context.credentials.Remove(crendential);
            _context.SaveChanges();

            return true;
        }

        private credential convertToDomainObject(CredentialDto credentialModel)
        {
            credential credential = null;
            if (credentialModel is AmazonCredentialDto)
                credential = Mapper.Map<AmazonCredentialDto, amazoncredential>(credentialModel as AmazonCredentialDto);
            else if (credentialModel is eBayCredentialDto)
                credential = Mapper.Map<eBayCredentialDto, ebaycredential>(credentialModel as eBayCredentialDto);
            else if (credentialModel is ShipStationCredentialDto)
                credential = Mapper.Map<ShipStationCredentialDto, shipstationcredential>(credentialModel as ShipStationCredentialDto);
            else if (credentialModel is BigCommerceCredentialDto)
                credential = Mapper.Map<BigCommerceCredentialDto, bigcommercecredential>(credentialModel as BigCommerceCredentialDto);
            else
                throw new InvalidCastException(string.Format("Unknown credential type \'{0}\' for casting!", credentialModel.MarketplaceType));

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

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            // Dispose other managed resources.
            _context.Dispose();
        }
        #endregion
    }
}
