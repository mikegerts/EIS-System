using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using X.PagedList;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class MessageTemplateService : IMessageTemplateService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;

        public MessageTemplateService(ILogService logger)
        {
            _context = new EisInventoryContext();
            _logger = logger;
        }

        public IPagedList<MessageTemplateListDto> GetMessageTemplates(int page, int pageSize, string searchString)
        {
            return _context.messagetemplates
                  .Where(x => string.IsNullOrEmpty(searchString) || (x.Subject.Contains(searchString) || x.Description.Contains(searchString)
                  || (x.systememail !=null && x.systememail.EmailAddress.Contains(searchString)))
                  )
                  .Select(x => new MessageTemplateListDto
                  {
                      Description = x.Description,
                      EmailAddress = x.systememail.EmailAddress,
                      Id = x.Id,
                      IsEnabled = x.IsEnabled,
                      MessageType = x.MessageType,
                      Subject = x.Subject
                  })
                  .OrderBy(x => x.Id)
                  .ToPagedList(page, pageSize).ToMappedPagedList<MessageTemplateListDto, MessageTemplateListDto>();
                  
        }

        public IEnumerable<MessageTemplateListDto> GetMessageTemplatesByType(MessageType messageType)
        {
            var results = _context.messagetemplates
                .Where(x => x.MessageType == messageType);

            return Mapper.Map<IEnumerable<MessageTemplateListDto>>(results);
        }

        public IEnumerable<MessageTemplateDto> GetMessageTemplatesDataByType(MessageType messageType)
        {
            var results = _context.messagetemplates
                .Where(x => x.MessageType == messageType);

            return Mapper.Map<IEnumerable<MessageTemplateDto>>(results);
        }

        public MessageTemplateDto GetMessageTemplate(int id)
        {
            var result = _context.messagetemplates.FirstOrDefault(x => x.Id == id);

            return Mapper.Map<MessageTemplateDto>(result);
        }
        public MessageTemplateDto CreateMessageTemplate(MessageTemplateDto model)
        {
            try
            {
                var result = Mapper.Map<messagetemplate>(model);
                result.Created = DateTime.Now;

                _context.messagetemplates.Add(result);
                _context.SaveChanges();

                return Mapper.Map<MessageTemplateDto>(result);
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

        public MessageTemplateDto UpdateMessageTemplate(MessageTemplateDto model)
        {
            try
            {
                var oldMessageTemplate = _context.messagetemplates.FirstOrDefault(x => x.Id == model.Id);
                var updatedMessageTemplate = Mapper.Map<messagetemplate>(model);
                updatedMessageTemplate.Modified = DateTime.Now;

                _context.Entry(oldMessageTemplate).CurrentValues.SetValues(updatedMessageTemplate);
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

        public bool DeleteMessageTemplate(int id)
        {
            var messageTemplate = _context.messagetemplates.SingleOrDefault(x => x.Id == id);
            if (messageTemplate == null)
                return true;

            _context.messagetemplates.Remove(messageTemplate);
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
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
