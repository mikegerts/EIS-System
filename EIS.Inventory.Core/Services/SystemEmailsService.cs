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
    public class SystemEmailsService : ISystemEmailsService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public SystemEmailsService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }
        public SystemEmailDto CreateSystemEmail(SystemEmailDto model)
        {
            try
            {
                var systemEmail = Mapper.Map<systememail>(model);
                systemEmail.Created = DateTime.UtcNow;
                systemEmail.CreatedBy = model.ModifiedBy;
                systemEmail.ModifiedBy = null;

                _context.systememails.Add(systemEmail);
                _context.SaveChanges();
                model.Id = systemEmail.Id;

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

        public bool DeleteSystemEmail(int Id)
        {
            var systememails = _context.systememails.SingleOrDefault(x => x.Id == Id);
            if (systememails == null)
                return false;

            _context.systememails.Remove(systememails);
            _context.SaveChanges();

            return true;
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

        public IEnumerable<SystemEmailDto> GetAllSystemEmails()
        {
            var systememaillist = _context.systememails.OrderBy(x => x.EmailAddress).ToList();

            return Mapper.Map<IEnumerable<systememail>, IEnumerable<SystemEmailDto>>(systememaillist);
        }

        public IPagedList<SystemEmailsListDto> GetPagedSystemEmails(int page, int pageSize, string searchString)
        {
            return _context.systememails.Where(x => string.IsNullOrEmpty(searchString) || x.EmailAddress.Contains(searchString))
               .OrderBy(x => x.Id)
               .ToPagedList(page, pageSize)
               .ToMappedPagedList<systememail, SystemEmailsListDto>();
        }

        public SystemEmailDto GetSystemEmail(int systemEmailId)
        {
            var systemEmail = _context.systememails.FirstOrDefault(x => x.Id == systemEmailId);

            return Mapper.Map<systememail, SystemEmailDto>(systemEmail);
        }

        public bool IsEmailExist(int systemEmailId, string emailAddress)
        {
            if (systemEmailId > 0)
            {
                return _context.systememails.Any(x => x.EmailAddress == emailAddress && x.Id != systemEmailId);
            }
            else
            {
                return _context.systememails.Any(x => x.EmailAddress == emailAddress);
            }
        }

        public SystemEmailDto UpdateSystemEmail(SystemEmailDto model)
        {
            try
            {
                var oldSystemEmail = _context.systememails.FirstOrDefault(x => x.Id == model.Id);
                var updatedSystemEmail = Mapper.Map<systememail>(model);
                updatedSystemEmail.Modified = DateTime.Now;
                updatedSystemEmail.ModifiedBy = model.ModifiedBy;
                updatedSystemEmail.Created = oldSystemEmail.Created;
                updatedSystemEmail.CreatedBy = oldSystemEmail.CreatedBy;



                _context.Entry(oldSystemEmail).CurrentValues.SetValues(updatedSystemEmail);
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

        public bool IsMessageTemplateFound(int systemEmailId)
        {
            return _context.messagetemplates.Any(x => x.SystemEmailId == systemEmailId);
        }
    }
}
