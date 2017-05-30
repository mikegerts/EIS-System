using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public interface IMessageTemplateService
    {
        /// <summary>
        /// Get the paged message templates
        /// </summary>
        /// <param name="page">The number of page</param>
        /// <param name="pageSize">The size of the records to display</param>
        /// <param name="searchString">The string to be searched</param>
        /// <returns></returns>
        IPagedList<MessageTemplateListDto> GetMessageTemplates(int page, int pageSize, string searchString);

        /// <summary>
        /// Get the list of message templates with the specified message type
        /// </summary>
        /// <param name="messageType">The type of message template</param>
        /// <returns></returns>
        IEnumerable<MessageTemplateListDto> GetMessageTemplatesByType(MessageType messageType);

        /// <summary>
        /// Get the list of message templates with the specified message type
        /// </summary>
        /// <param name="messageType">The type of message template</param>
        /// <returns></returns>
        IEnumerable<MessageTemplateDto> GetMessageTemplatesDataByType(MessageType messageType);

        /// <summary>
        /// Get the message template with the specified message id
        /// </summary>
        /// <param name="id">The id of the message template</param>
        /// <returns></returns>
        MessageTemplateDto GetMessageTemplate(int id);

        /// <summary>
        /// Create the message template
        /// </summary>
        /// <param name="model">The message template data</param>
        /// <returns></returns>
        MessageTemplateDto CreateMessageTemplate(MessageTemplateDto model);

        /// <summary>
        /// Update the message template
        /// </summary>
        /// <param name="model">The message template data</param>
        /// <returns></returns>
        MessageTemplateDto UpdateMessageTemplate(MessageTemplateDto model);

        /// <summary>
        /// Delete the message template with specified id
        /// </summary>
        /// <param name="id">The id of the message template</param>
        /// <returns></returns>
        bool DeleteMessageTemplate(int id);
    }
}
