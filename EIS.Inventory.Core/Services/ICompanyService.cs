using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{

    public interface ICompanyService : IDisposable
    {
        /// <summary>
        /// Get the paged list of the companies
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="searchString">The keyword to search for company's details</param>
        /// <returns></returns>
        IPagedList<CompanyListDto> GetPagedCompanies(int page,
            int pageSize,
            string searchString);

        /// <summary>
        /// Get the list of all companies
        /// </summary>
        /// <returns></returns>
        IEnumerable<CompanyDto> GetAllCompanies();

        /// <summary>
        /// Get ther company with the specified company id
        /// </summary>
        /// <param name="companyId">The company Id</param>
        /// <returns></returns>
        CompanyDto GetCompany(int companyId);

        /// <summary>
        /// Get the default company
        /// </summary>
        /// <returns></returns>
        CompanyDto GetDefaultCompany();

        /// <summary>
        /// Create new company in the database
        /// </summary>
        /// <param name="model">The company to save</param>
        /// <returns></returns>
        bool CreateCompany(CompanyDto model);

        /// <summary>
        /// Update the company with the modified model
        /// </summary>
        /// <param name="model">The updated company</param>
        /// <returns></returns>
        bool UpdateCompany(CompanyDto model);

        /// <summary>
        /// Delete the company with the specified company Id
        /// </summary>
        /// <param name="companyId">The company Id</param>
        /// <returns></returns>
        bool DeleteCompany(int companyId);

        void ResetDefaultCompanies(int companyId);
    }
}
