using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;
namespace EIS.Inventory.Core.Services
{
    public interface ISystemEmailsService : IDisposable
    {
        /// <summary>
        /// Get the paged list of the systememail
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="searchString">The keyword to search for systememail's details</param>
        /// <returns></returns>
        IPagedList<SystemEmailsListDto> GetPagedSystemEmails(int page,
            int pageSize,
            string searchString);

        /// <summary>
        /// Get the list of all systememails
        /// </summary>
        /// <returns></returns>
        IEnumerable<SystemEmailDto> GetAllSystemEmails();

        /// <summary>
        /// Get ther systememail with the specified systememail id
        /// </summary>
        /// <param name="companyId">The systememail Id</param>
        /// <returns></returns>
        SystemEmailDto GetSystemEmail(int systemEmailId);

        /// <summary>
        /// Create new systememail in the database
        /// </summary>
        /// <param name="model">The systememail to save</param>
        /// <returns></returns>
        SystemEmailDto CreateSystemEmail(SystemEmailDto model);

        /// <summary>
        /// Update the systememail with the modified model
        /// </summary>
        /// <param name="model">The updated systememail</param>
        /// <returns></returns>
        SystemEmailDto UpdateSystemEmail(SystemEmailDto model);

        /// <summary>
        /// Delete the systememail with the specified systememail Id
        /// </summary>
        /// <param name="Id">The systememail Id</param>
        /// <returns></returns>
        bool DeleteSystemEmail(int Id);

        /// <summary>
        /// Check the systememail address exist
        /// </summary>
        /// <param name="systemEmailId">The systememail Id</param>
        /// <param name="emailAddress">The email address</param>
        /// <returns></returns>
        bool IsEmailExist(int systemEmailId, string emailAddress);


        /// <summary>
        /// Check the message template exist with this systememail
        /// </summary>
        /// <param name="Id">The systememail Id</param>
        /// <returns></returns>
        bool IsMessageTemplateFound(int systemEmailId);
    }
}
