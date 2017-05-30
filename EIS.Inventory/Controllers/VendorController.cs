using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class VendorController : Controller
    {
        private readonly IVendorService _vendorService;
        private readonly ICompanyService _companyService;

        public VendorController(IVendorService vendorService, ICompanyService companyService)
        {
            _vendorService = vendorService;
            _companyService = companyService;
        }

        [AuthorizeRoles("Administrator", "CanViewVendors")]
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeRoles("Administrator", "CanViewVendors")]
        public JsonResult _GetVendors()
        {
            var vendors = _vendorService.GetAllVendors();

            return Json(vendors, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewVendors")]
        public JsonResult _GetVendor(int id)
        {
            var vendor = _vendorService.GetVendor(id);
            return Json(vendor, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetDepartments()
        {
            var departments = _vendorService.GetDepartments();

            return Json(departments, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditVendors")]
        public JsonResult _SaveVendor(VendorDto model, int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }

            // trim the EIS start SKU code
            model.SKUCodeStart = model.SKUCodeStart.Trim();
            model.ModifiedBy = User.Identity.Name;

            var result = new VendorDto();
            if (id == -1)
                result = _vendorService.CreateVendor(model);
            else
                result = _vendorService.UpdateVendor(model);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteVendors")]
        public JsonResult _DeleteVendor(int id)
        {
            try
            {
                _vendorService.DeleteVendor(id);

                return Json(new { Success = "Vendor has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting vendor! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _GetVendorsByCompany(int companyId)
        {
            var results = _vendorService.GetVendorsByCompany(companyId);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetVendorEmailAddress(int vendorId)
        {
            var email = _vendorService.GetVendorEmail(vendorId);

            return Json(email, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            _companyService.Dispose();
            _vendorService.Dispose();
            base.Dispose(disposing);
        }
    }
}