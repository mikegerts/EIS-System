using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Linq;
using AutoMapper;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Services
{
    public class ReportTemplateService : IReportTemplateService
    {
        private readonly ILogService _logger;
        private readonly string _connectionString;
        private readonly EisInventoryContext _context;

        public ReportTemplateService(ILogService logger)
        {
            _context = new EisInventoryContext();
            _logger = logger;
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }
        public ReportTemplateViewModel SaveTemplate(ReportTemplateViewModel model)
        {
            try
            {
                var reportTemplate = Mapper.Map<ReportTemplateViewModel, reporttemplate>(model);

                _context.reporttemplates.Add(reportTemplate);
                _context.SaveChanges();

                return Mapper.Map<reporttemplate, ReportTemplateViewModel>(reportTemplate);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return model;
            }
        }

        public void DeleteTemplate(int id)
        {
            try
            {
                var templateToDelete = _context.reporttemplates.FirstOrDefault(x => x.Id == id);
                _context.reporttemplates.Remove(templateToDelete);
                _context.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }

        public ReportTemplateViewModel GetReportTemplateById(int id)
        {
            var reportTemplate = _context.reporttemplates.FirstOrDefault(x => x.Id == id);

            return Mapper.Map<reporttemplate, ReportTemplateViewModel>(reportTemplate);
        }
        public IEnumerable<ReportTemplateViewModel> GetReportTemplates()
        {
            var reportTemplates = _context.reporttemplates.ToList();
            return Mapper.Map<IEnumerable<reporttemplate>, IEnumerable<ReportTemplateViewModel>>(reportTemplates);
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
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
