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
    public class SystemEmailController : Controller
    {
        private readonly ISystemEmailsService _systemEmailsService;

        public SystemEmailController(ISystemEmailsService systemEmailsService)
        {
            _systemEmailsService = systemEmailsService;
        }

        [AuthorizeRoles("Administrator", "CanViewSystemEmails")]
        public ActionResult Index(string searchString, int page = 1, int pageSize = 10)
        {
            var systemEmails = _systemEmailsService.GetPagedSystemEmails(page, pageSize, searchString);
            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedSystemEmails", systemEmails);

            return View(systemEmails);
        }


        // GET: create
        [AuthorizeRoles("Administrator", "CanEditSystemEmail")]
        public ActionResult Create()
        {
            // create view bag first for the message template types
            populateSystemEmailViewBag();

            return View(new SystemEmailDto());
        }

        [AuthorizeRoles("Administrator", "CanEditSystemEmail")]
        public ActionResult Edit(int id)
        {
            var systemEmail = _systemEmailsService.GetSystemEmail(id);

            // create view bag first for the vendors
            populateSystemEmailViewBag();
            ViewBag.Message = TempData["Message"];

            return View(systemEmail);
        }



        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditSystemEmails")]
        public ActionResult Save(int id, SystemEmailDto model)
        {
            // create view bag first for the vendors
            populateSystemEmailViewBag();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                return View("create", model);
            }

            if (_systemEmailsService.IsEmailExist(id, model.EmailAddress))
            {
                if (id <= 0)
                {
                    ModelState.AddModelError("", "Email address already exists");
                    return View("create", model);
                }
                else
                {
                    ModelState.AddModelError("", "Email address already exists");
                    return View("edit", model);
                }
            }

            // save the message template
            model.ModifiedBy = User.Identity.Name;
            if (id == 0)
                model = _systemEmailsService.CreateSystemEmail(model);
            else
                model = _systemEmailsService.UpdateSystemEmail(model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = model.Id });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteSystemEmails")]
        public JsonResult _DeleteSystemEmail(int id)
        {
            try
            {
                if (_systemEmailsService.IsMessageTemplateFound(id))
                {
                    return Json(new { Error = "This system email attached with message template(s)." },
                JsonRequestBehavior.AllowGet);
                }
                _systemEmailsService.DeleteSystemEmail(id);

                return Json(new { Success = "System email has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting System email! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }


        private void populateSystemEmailViewBag(object selected = null)
        {
            // convert the message type enum to list
            var messageTypes = EnumHelper.GetEnumKeyValuePairList<MessageType>();
            ViewBag.MessageTypeList = new SelectList(messageTypes, "Id", "Name", selected);
        }
    }
}