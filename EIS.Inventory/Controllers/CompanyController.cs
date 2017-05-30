using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{

    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IVendorService _vendorService;
        private readonly ICredentialService _credentialService;

        public CompanyController(ICompanyService companyService,
            IVendorService vendorService,
            ICredentialService credentialService)
        {
            _companyService = companyService;
            _vendorService = vendorService;
            _credentialService = credentialService;
        }

        [AuthorizeRoles("Administrator", "CanViewCompanies")]
        public ActionResult Index(string searchString,
            int page = 1,
            int pageSize = 10)
        {
            var companies = _companyService.GetPagedCompanies(page, pageSize, searchString);

            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedCompanies", companies);

            return View(companies);
        }

        [AuthorizeRoles("Administrator", "CanEditCompanies")]
        public ActionResult Create()
        {
            return View(new CompanyDto());
        }

        [AuthorizeRoles("Administrator", "CanEditCompanies")]
        public ActionResult Edit(int id)
        {
            var company = _companyService.GetCompany(id);
            ViewBag.PreviousUrl = Request.UrlReferrer;
            ViewBag.Message = TempData["Message"];

            return View(company);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditCompanies")]
        public ActionResult Save(int id, CompanyDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));

                // return to the original page which the request came from
                var originpage = id == 0 ? "create" : "edit";
                return View(originpage, model);
            }

            // set who modified this record
            model.ModifiedBy = User.Identity.Name;
            
            if (id == 0)
                _companyService.CreateCompany(model);
            else
                _companyService.UpdateCompany(model);

            if (model.IsDefault == 1)
                _companyService.ResetDefaultCompanies(id);

            // if we got this far, everything is OK
            TempData["Message"] = "Changes have been successfully saved!";

            return RedirectToAction("edit", new { id = model.Id });
        }

        [AuthorizeRoles("Administrator", "CanViewCompanies")]
        public JsonResult _GetAllCompanies()
        {
            var companies = _companyService.GetAllCompanies();

            return Json(companies, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanEditCompanies")]
        public JsonResult _GetCompany(int id)
        {
            var company = _companyService.GetCompany(id);

            return Json(company, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanEditCompanies")]
        public JsonResult _GetCompanyVendors(int id)
        {
            var vendors = _vendorService.GetVendorsByCompany(id);

            return Json(vendors, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetCompanyCredentials(int id)
        {
            var credentials = _credentialService.GetCredentialsByCompany(id);

            return Json(credentials, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCompanies")]
        public JsonResult _DeleteCompany(int id)
        {
            try
            {
                _companyService.DeleteCompany(id);

                return Json(new { Success = "Company has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting company! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _companyService.Dispose();
            _vendorService.Dispose();
            _credentialService.Dispose();
            base.Dispose(disposing);
        }
    }
}