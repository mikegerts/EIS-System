using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using System.IO;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ShippingRateController : Controller
    {
        private readonly IShippingRateService _service;
        private readonly IExportDataService _exportDataService;

        public ShippingRateController ( IShippingRateService service,
            IExportDataService exportDataService)
        {
            _service = service;
            _exportDataService = exportDataService;
        }

        [AuthorizeRoles("Administrator", "CanViewShippingRates")]
        public ActionResult Index ()
        {
            return View();
        }

        [AuthorizeRoles("Administrator", "CanViewShippingRates")]
        public JsonResult _GetAllShippingRates ()
        {
            var shippingrates = _service.GetAllShippingRates();

            return Json(shippingrates, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewShippingRates")]
        public JsonResult _GetShippingRate ( int id )
        {
            var shippingrate = _service.GetShippingRate(id);

            return Json(shippingrate, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditShippingRates")]
        public JsonResult _SaveShippingRate ( ShippingRateDto model, int id )
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }

            var result = new ShippingRateDto();
            if (id == -1)
                result = _service.CreateShippingRate(model);
            else
                result = _service.UpdateShippingRatey(model);
            

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteShippingRates")]
        public JsonResult _DeleteShippingRate ( int id )
        {
            try
            {
                _service.DeleteShippingRate(id);

                return Json(new { Success = "Shipping Rate has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting shipping rate! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        [AuthorizeRoles("Administrator", "CanExportShippingRates")]
        public ActionResult ExportShippingRates(string selectedIds)
        {
            var currentDateTime = DateTime.Now;
            var fileName = _exportDataService.CustomExportShippingRates(selectedIds, currentDateTime);
            var fileInfo = new FileInfo(fileName);


            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportShippingRates.csv\"", currentDateTime));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }
    }
}