using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using EIS.SchedulerTaskApp.Models;
using EIS.SchedulerTaskApp.Repositories;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class EmailSender
    {
        private static string _emailFrom;
        private static string[] _adminBccs;

        static EmailSender()
        {
            var emailBcc = ConfigurationManager.AppSettings["EmailBcc"].ToString();
            _emailFrom = ConfigurationManager.AppSettings["EmailFrom"].ToString();
            _adminBccs = emailBcc.Split(',');
        }

        public static bool SendPoMessage(string[] mailTos, string[] mailCcs, string subject, PurchaseOrder exportedOrders)
        {
            var mailBody = _getMessageBody(exportedOrders);

            return SendMessage(mailTos, mailCcs, subject, mailBody);
        }

        public static bool SendConfirmationMessage(string[] mailTos, bool hasOrders, string orderFilePath)
        {
            try
            {
                // get the message template
                var messageTemplate = File.ReadAllText(".\\Templates\\EisMailTemplate.html");
                var msgBody = hasOrders
                    ? string.Format(messageTemplate, DateTime.Now, string.Format(@"<h2 style='color: green'>Order File Dropped Successfully on {0:MMM dd yyyy h:mm tt}</h2>", DateTime.Now))
                    : "No Order File Dropped Today. Reason: \"No orders today\"";

                return SendMessage(mailTos,
                    null,
                    string.Format("EIS Scheduled Export Order - {0:dd-MMM-yyy}", DateTime.Now),
                    msgBody,
                    orderFilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool SendConfirmationMessage(string[] mailTos, string[] mailCc, string orderFilePath)
        {
            try
            {
                // get the message template
                var messageTemplate = File.ReadAllText(".\\Templates\\EisMailTemplate.html");
                messageTemplate = string.Format(messageTemplate, DateTime.Now, string.Format(@"<p>Requested Sku list of file is attached with price. Please review it.</p>", DateTime.Now));
                return SendMessage(mailTos,
                    mailCc,
                    string.Format("EIS Scheduled Requested Sku Price  - {0:dd-MMM-yyy}", DateTime.Now),
                    messageTemplate,
                    orderFilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendMessage(string[] mailTos, string[] mailCCs, string subject, string body, string attachmentFilename = "")
        {
#if DEBUG
            var isReturn = true;
            if (isReturn)
                return isReturn;
#endif
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress(_emailFrom);

                // add the mail TO address
                foreach (var email in mailTos)
                    mail.To.Add(email);

                // add the mail CC address
                if (mailCCs != null)
                {
                    foreach (var email in mailCCs)
                        mail.CC.Add(email);
                }

                // add the Admin emails for BCC
                foreach (var email in _adminBccs)
                    mail.Bcc.Add(email);

                // add the attachement if there's any
                if (!string.IsNullOrEmpty(attachmentFilename))
                    mail.Attachments.Add(new Attachment(attachmentFilename));

                mail.Subject = string.Format(subject, DateTime.UtcNow);
                mail.IsBodyHtml = true;
                mail.Body = body;

                var smtp = new SmtpClient();
                smtp.Send(mail);

                Logger.LogInfo(LogEntryType.EmailSender, string.Format("\'{1}\' has been successfully sent to -> {0}", string.Join(",", mailTos), subject));
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(LogEntryType.EmailSender, string.Format("Error in sending email TO:{0}, CC:{1} <br/>Error message: {2}",
                    string.Join(",", mailTos), string.Join(",", mailCCs), ex.Message), ex.StackTrace);
                return false;
            }
        }

        private static string _getMessageBody(PurchaseOrder exportedOrders)
        {
            var messageTemplate = File.ReadAllText(".\\Templates\\OrderPlaced.html");
            var config = new TemplateServiceConfiguration();
            config.DisableTempFileLocking = true;

            Engine.Razor = RazorEngineService.Create(config);
            return Engine.Razor.RunCompile(messageTemplate, "templateKey", null, exportedOrders);
        }
    }
}
