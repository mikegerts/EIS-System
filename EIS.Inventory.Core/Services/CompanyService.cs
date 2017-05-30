using System;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using System.Data.Entity.Validation;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public CompanyService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }

        public IPagedList<CompanyListDto> GetPagedCompanies(int page,
            int pageSize,
            string searchString)
        {
            return _context.companies           
                .OrderBy(x => x.Id)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<company, CompanyListDto>();
        }

        public IEnumerable<CompanyDto> GetAllCompanies()
        {
            var companies = _context.companies.OrderBy(x => x.Name).ToList();

            return Mapper.Map<IEnumerable<company>, IEnumerable<CompanyDto>>(companies);
        }

        public CompanyDto GetCompany(int companyId)
        {
            var company = _context.companies.SingleOrDefault(x => x.Id == companyId);
            return Mapper.Map<company, CompanyDto>(company);
        }

        public CompanyDto GetDefaultCompany()
        {
            var company = _context.companies.FirstOrDefault(x => (bool)x.IsDefault);

            return Mapper.Map<company, CompanyDto>(company);
        }

        public bool CreateCompany(CompanyDto model)
        {
            try
            {
                var company = Mapper.Map<CompanyDto, company>(model);
                company.Created = DateTime.UtcNow;
                company.CreatedBy = model.ModifiedBy;
                company.ModifiedBy = null;

                _context.companies.Add(company);
                _context.SaveChanges();
                model.Id = company.Id;

                return true;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorService, errorMsg, ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return false;
            }
        }

        public bool UpdateCompany(CompanyDto model)
        {
            var existingCompany = _context.companies.FirstOrDefault(x => x.Id == model.Id);
            
            // reflect the changes from model
            Mapper.Map(model, existingCompany);
            existingCompany.ModifiedBy = model.ModifiedBy;
            existingCompany.Modified = DateTime.UtcNow;

            _context.SaveChanges();

            return true;
        }

        public bool DeleteCompany(int companyId)
        {
            var company = _context.companies.SingleOrDefault(x => x.Id == companyId);
            if (company == null)
                return true;

            _context.companies.Remove(company);
            _context.SaveChanges();

            return true;
        }

        public void ResetDefaultCompanies(int companyId)
        {
            var companies = _context.companies.Where(x => x.Id != companyId).ToList();

            foreach (var company in companies)
            {
                company.IsDefault = false;
            }

            _context.SaveChanges();
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
                _context.Dispose();
                _logger.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
