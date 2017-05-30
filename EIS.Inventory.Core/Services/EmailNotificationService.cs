using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using System.Text;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Services
{
    public class EmailNotificationService
    {
        IMessageTemplateService _messageTemplateService;
        ISystemEmailsService _systemEmailService;
        ILogService _logService;

        public EmailNotificationService()
        {
            _logService = new LogService();
            _messageTemplateService = new MessageTemplateService(_logService);
            _systemEmailService = new SystemEmailsService(_logService);
        }

        public bool SendEmail(string subject, string body, string mailTo)
        {
            // get the message template
            var msgTemplate = getMessageTemplate(MessageType.EmailNotification);
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[Body]", body);

            var client = getSmtpClient();
            var mailMsg = createMailMessage(string.Format("{0} - {1}", msgTemplate.Subject, subject), body, mailTo, GetSenderEmail(msgTemplate.SystemEmailId));

            return send(mailMsg);
        }

        public bool SendShipstationWarningEmailAdmin(string subject, string body)
        {
            var mailTo = getEmailTo();
            var message = createMailMessage(subject, body, mailTo, "");

            return send(message);
        }

        public void SendEmailForInsufficientProducts(OrderProductResult result)
        {
            var msgTemplate = getMessageTemplate(MessageType.InsufficientVendorProduct);
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[OrderItem.OrderId]", result.OrderItem.OrderId);
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[OrderItem.SKU]", result.OrderItem.SKU);
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[OrderItem.Title]", result.OrderItem.Title);
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[OrderItem.QtyOrdered]", result.OrderItem.QtyOrdered.ToString());

            // create table row for the order products

            var tableBodySb = new StringBuilder();
            foreach(var item in result.OrderProducts)
            {
                tableBodySb.AppendFormat(
                    @"<tr>
                        <td style='font-size:12px; font-family: Arial;'>{0}</td>
                        <td style='font-size:12px; font-family: Arial;'>{1}</td>
                        <td style='font-size:12px; font-family: Arial;'>{2}</td>
                    </tr>", item.EisSupplierSKU, item.Pack, item.Quantity);
            }
            var tabledOrderProducts = string.Format(
            @"<table style='width:100%;>
	            <thead>
		            <tr>
			            <td style='font-size:12px; font-family: Arial;'>EIS Supplier SKU</td>
			            <td style='font-size:12px; font-family: Arial;'>Pack</td>
			            <td style='font-size:12px; font-family: Arial;'>Quantity</td>
		            </tr>
	            </thead>
	            <tbody>
		            {0}
	            </tbody>
            </table>", tableBodySb.ToString());

            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[Item.TabledOrderProducts]", tableBodySb.ToString());
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[Item.ItemPack]", result.ItemPack.ToString());
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[Item.TotalOrderedItems]", result.TotalOrderedItems.ToString());
            msgTemplate.ContentHtml = msgTemplate.ContentHtml.Replace("[Item.TotalAvailableItems]", result.TotalAvailableItems.ToString());

            var mailMessage = createMailMessage(string.Format(msgTemplate.Subject, result.OrderItem.OrderId), msgTemplate.ContentHtml, getEmailTo(), GetSenderEmail(msgTemplate.SystemEmailId));

            send(mailMessage);
        }

        public bool SendEmailAdminException(string subject, Exception exParam, bool? useDefaultTemplate, string url, string userName)
        {
            var mailTo = getEmailTo();
            var message = getMailMessageException(subject, exParam, mailTo, useDefaultTemplate, url, userName);

            return send(message);
        }

        private bool send(MailMessage message)
        {
#if DEBUG
            var isReturn = true;
            if (isReturn)
                return isReturn;
#endif
            try
            {
                var client = getSmtpClient();
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError(LogEntryType.General,
                    string.Format("Email Notification Error Sending - {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return false;
            }
        }

        private MailMessage createMailMessage(string subject, string body, string mailTo,string senderEmailFrom)
        {
            var mailFrom = string.IsNullOrEmpty(senderEmailFrom) ? ConfigurationManager.AppSettings["EmailServiceUser"] : senderEmailFrom;
            var message = new MailMessage(mailFrom, mailTo);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            // include the CC emails if there's any
            var mailCcs = getEmailCCs();
            if (mailCcs != null)
                mailCcs.ForEach(mail => message.CC.Add(mail));

            return message;
        }

        private messagetemplate getMessageTemplate(MessageType messageType)
        {
            var messageTemplate = new messagetemplate();
            using(var context = new EisInventoryContext())
            {
                messageTemplate = context.messagetemplates
                    .FirstOrDefault(x => x.MessageType == messageType && x.IsEnabled);
            }

            return messageTemplate;
        }

        private List<string> getEmailCCs()
        {
            var mailCC = ConfigurationManager.AppSettings["EmailServiceCC"];
            if (string.IsNullOrEmpty(mailCC))
                return null;

            return mailCC.Split(',').ToList();
        }

        private string getEmailTo()
        {
            return ConfigurationManager.AppSettings["EmailServiceTo"];
        }

        private SmtpClient getSmtpClient()
        {
            var smtpServer = ConfigurationManager.AppSettings["SMTPServer"];
            var smtpPort = ConfigurationManager.AppSettings["SMTPPort"];
            var emailServiceUser = ConfigurationManager.AppSettings["EmailServiceUser"];
            var emailSerivcePass = ConfigurationManager.AppSettings["EmailServicePass"];

            return new SmtpClient(smtpServer, int.Parse(smtpPort))
            {
                Credentials = new NetworkCredential(emailServiceUser, emailSerivcePass),
                EnableSsl = true
            };
        }

        private MailMessage getMailMessageException(string subject, Exception ex, string mailTo, bool? defaultTemplate, string url, string userName)
        {
            var body = EisHelper.GetExceptionMessage(ex);

            if (defaultTemplate.HasValue && defaultTemplate.Value)
            {
                var messageTemplate = getMessageTemplateException(ex, subject, url, userName);

                if (messageTemplate != null)
                {
                    subject = messageTemplate.Subject;
                    body = messageTemplate.ContentHtml;
                }
            }
            var msgTemplate = getMessageTemplate(MessageType.ErrorNotification);

            return createMailMessage(subject, body, mailTo, GetSenderEmail(msgTemplate.SystemEmailId));
        }

        private messagetemplate getMessageTemplateException(Exception ex, string subject, string url, string userName)
        {
            var messageTemplate = getMessageTemplate(MessageType.ErrorNotification);

            var replaceText = messageTemplate.ContentHtml;
            replaceText = replaceText.Replace("[url]", url);
            replaceText = replaceText.Replace("[user]", userName);
            replaceText = replaceText.Replace("[exception_type]", ex.GetType().ToString());
            replaceText = replaceText.Replace("[message]", EisHelper.GetExceptionMessage(ex));
            replaceText = replaceText.Replace("[stack_trace]", ex.StackTrace);

            if(ex.InnerException != null)
            {
                replaceText = replaceText.Replace("[inner_exception]", EisHelper.GetExceptionMessage(ex));
            } 

            messageTemplate.ContentHtml = replaceText;
            messageTemplate.Subject = string.Format("{0} - {1}", messageTemplate.Subject, subject);

            return messageTemplate;
        }

        private string GetSenderEmail(int? systemEmailId)
        {
            var fromEmail = "";
            if (systemEmailId != null)
            {
                fromEmail = _systemEmailService.GetSystemEmail(systemEmailId.Value).EmailAddress;
            }

            return fromEmail;
        }
    }
}
