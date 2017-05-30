using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using X.PagedList;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class SavedSearchFilterService : ISavedSearchFilterService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public SavedSearchFilterService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }

        public SavedSearchFilterDto CreateSavedSearchFilter(SavedSearchFilterDto model)
        {
            try
            {
                var savedsearchfilter = Mapper.Map<savedsearchfilter>(model);
                savedsearchfilter.Created = DateTime.UtcNow;
                savedsearchfilter.CreatedBy = model.CreatedBy;
                

                _context.savedsearchfilters.Add(savedsearchfilter);
                _context.SaveChanges();
                model.Id = savedsearchfilter.Id;

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public bool DeleteSavedSearchFilter(int Id)
        {
            var savedsearchfilters = _context.savedsearchfilters.SingleOrDefault(x => x.Id == Id);
            if (savedsearchfilters == null)
                return true;

            _context.savedsearchfilters.Remove(savedsearchfilters);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<SavedSearchFilterDto> GetAllSavedSearchFilterDto(EnumSavedSearchFilters SavedSearchFilterId, string createdby )
        {
            var filterId = Convert.ToInt32(SavedSearchFilterId);
            var savedsearchfilterslist = _context.savedsearchfilters.Where(x => x.SavedSearchFilterId  == filterId && x.CreatedBy.ToLower() == createdby.ToLower()).ToList();

            return Mapper.Map<IEnumerable<savedsearchfilter>, IEnumerable<SavedSearchFilterDto>>(savedsearchfilterslist);
        }

        public SavedSearchFilterDto GetSavedSearchFilter(int Id)
        {
            var savedSearchFilter = _context.savedsearchfilters.FirstOrDefault(x => x.Id == Id);
            return Mapper.Map<savedsearchfilter, SavedSearchFilterDto>(savedSearchFilter);
        }

        public bool IsFilterExist(int Id, EnumSavedSearchFilters SavedSearchFilterId, string SavedSearchFilterName, string CreatedBy)
        {
            var filterId = Convert.ToInt32(SavedSearchFilterId);
            if (Id > 0)
                return _context.savedsearchfilters.Any(x => x.SavedSearchFilterId == filterId && x.SavedSearchFilterName.ToLower() == SavedSearchFilterName.ToLower() && x.Id != Id && x.CreatedBy.ToLower() == CreatedBy.ToLower());
            else
                return _context.savedsearchfilters.Any(x => x.SavedSearchFilterId == filterId && x.SavedSearchFilterName.ToLower() == SavedSearchFilterName.ToLower() && x.CreatedBy.ToLower() == CreatedBy.ToLower());
        }

        public SavedSearchFilterDto UpdateSavedSearchFilter(SavedSearchFilterDto model)
        {
            try
            {
                var oldSavedFilterSearch = _context.savedsearchfilters.FirstOrDefault(x => x.Id == model.Id);
                var updatedSavedFilterSearch = Mapper.Map<savedsearchfilter>(model);
                
                updatedSavedFilterSearch.Created = DateTime.Now;
                updatedSavedFilterSearch.CreatedBy = model.CreatedBy;

                _context.Entry(oldSavedFilterSearch).CurrentValues.SetValues(updatedSavedFilterSearch);
                _context.SaveChanges();

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorProductService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }
    }
}
