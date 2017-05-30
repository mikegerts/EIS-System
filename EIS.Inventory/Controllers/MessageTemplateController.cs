using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class MessageTemplateController : Controller
    {
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ISystemEmailsService _systemEmailsService;

        public MessageTemplateController(IMessageTemplateService messageTemplateService, ISystemEmailsService systemEmailsService)
        {
            _messageTemplateService = messageTemplateService;
            _systemEmailsService = systemEmailsService;
        }

        [AuthorizeRoles("Administrator", "CanViewMessageTemplates")]
        public ActionResult Index(string searchString, int page = 1, int pageSize = 10)
        {
            var messageTemplates = _messageTemplateService.GetMessageTemplates(page, pageSize, searchString);
            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedMessageTemplates", messageTemplates);

            return View(messageTemplates);
        }

        // GET: create
        [AuthorizeRoles("Administrator", "CanEditMessageTemplates")]
        public ActionResult Create()
        {
            // create view bag first for the message template types
            populateMessageTypesViewBag();

            return View(new MessageTemplateDto());
        }

        [AuthorizeRoles("Administrator", "CanEditMessageTemplates")]
        public ActionResult Edit(int id)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplate(id);

            // create view bag first for the vendors
            populateMessageTypesViewBag();
            ViewBag.Message = TempData["Message"];

            return View(messageTemplate);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditMessageTemplates")]
        public ActionResult Save(int id, MessageTemplateDto model)
        {
            // create view bag first for the vendors
            populateMessageTypesViewBag();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                return View(model);
            }

            // save the message template
            model.UserName = User.Identity.Name;
            if (id == 0)
                model = _messageTemplateService.CreateMessageTemplate(model);
            else
                model = _messageTemplateService.UpdateMessageTemplate(model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = model.Id });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteMessageTemplates")]
        public JsonResult _DeleteMessageTemplate(int id)
        {
            try
            {
                _messageTemplateService.DeleteMessageTemplate(id);

                return Json(new { Success = "Message template has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting message template! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanEditMessageTemplates")]
        public JsonResult _GetMessageTemplates(MessageType messageType)
        {
            var results = _messageTemplateService.GetMessageTemplatesByType(messageType);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        private void populateMessageTypesViewBag(object selected = null)
        {
            // convert the message type enum to list
            var messageTypes = EnumHelper.GetEnumKeyValuePairList<MessageType>();
            ViewBag.MessageTypeList = new SelectList(messageTypes, "Id", "Name", selected);

            var SystemEmailList = _systemEmailsService.GetAllSystemEmails().Where(x => x.IsActive);
            ViewBag.SystemEmailList = new SelectList(SystemEmailList, "Id", "EmailAddress", selected);
        }
    }
}