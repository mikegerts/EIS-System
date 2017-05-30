using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public interface ISavedSearchFilterService
    {
        /// <summary>
        /// Get the list of all systememails
        /// </summary>
        /// <returns></returns>
        IEnumerable<SavedSearchFilterDto> GetAllSavedSearchFilterDto(EnumSavedSearchFilters SavedSearchFilterId, string created);

        /// <summary>
        /// Get ther SavedSearchFilter with the specified SavedSearchFilter id
        /// </summary>
        /// <param name="Id">The SavedSearchFilter Id</param>
        /// <returns></returns>
        SavedSearchFilterDto GetSavedSearchFilter(int Id);

        /// <summary>
        /// Create new SavedSearchFilter in the database
        /// </summary>
        /// <param name="model">The SavedSearchFilter to save</param>
        /// <returns></returns>
        SavedSearchFilterDto CreateSavedSearchFilter(SavedSearchFilterDto model);

        /// <summary>
        /// Update the savedsearchfilter with the modified model
        /// </summary>
        /// <param name="model">The updated SavedSearchFilter</param>
        /// <returns></returns>
        SavedSearchFilterDto UpdateSavedSearchFilter(SavedSearchFilterDto model);

        /// <summary>
        /// Delete the systememail with the specified systememail Id
        /// </summary>
        /// <param name="Id">The savedfilter Id</param>
        /// <returns></returns>
        bool DeleteSavedSearchFilter(int Id);

        /// <summary>
        /// Check if filter already exist 
        /// </summary>
        /// <param name="systemEmailId">systememail Id</param>
        /// /// <param name="emailAddress">email address</param>
        /// <returns></returns>
        bool IsFilterExist(int Id, EnumSavedSearchFilters SavedSearchFilterId, string SavedSearchFilterName,string CreatedBy);
    }
}
