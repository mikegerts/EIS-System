using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class FileSettingController : Controller
    {
        private readonly IFileSettingService _fileSettingService;
        private readonly IVendorService _vendorService;

        public FileSettingController(IFileSettingService fileSettingService, IVendorService vendorService)
        {
            _fileSettingService = fileSettingService;
            _vendorService = vendorService;
        }

        #region Product File Settings
        // GET: FileSetting
        public ActionResult ProductFiles()
        {
            return View();
        }

        public JsonResult _GetProductSettings()
        {
            var fileSettings = _fileSettingService.GetProductFileSettings();

            return Json(fileSettings, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetProductSetting(int vendorId)
        {
            var fileSetting = _fileSettingService.GetProductFileSettingByVendor(vendorId);

            return Json(fileSetting, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetUnConfiguredProductVendors()
        {
            var vendors = _vendorService.GetAllVendors();
            var configuredInventoryVendors = _fileSettingService.GetProductFileSettings().ToList();

            var unconfiguredVendors = new List<dynamic>();
            foreach (var vendor in vendors)
            {
                if (configuredInventoryVendors.Exists(x => x.VendorId == vendor.Id))
                    continue;

                unconfiguredVendors.Add(new { Id = vendor.Id, Name = vendor.Name });
            }

            return Json(unconfiguredVendors, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _SaveProductSetting(FileSettingViewModel model, int modelId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }

            if (modelId == -1)
            {
                // set the file path and transfer path from the web.config
                model.FilePath = ConfigurationManager.AppSettings["FilePath"];
                model.TransferPath = ConfigurationManager.AppSettings["TransferPath"];

                _fileSettingService.CreateProductFileSetting(model);
            }
            else
            {
                // do the update for the file setting
                _fileSettingService.UpdateProductFileSetting(modelId, model);
            }

            return Json(new { Success = "Product file setting has been successfully saved!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _DeleteProductSetting(int vendorId)
        {
            try
            {
                _fileSettingService.DeleteProductFileSetting(vendorId);

                return Json(new { Success = "Product file setting has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting product file setting! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) });
            }
        }
        #endregion

        #region Inventory File Settings
        public ActionResult InventoryFiles()
        {
            return View();
        }
        public JsonResult _GetInventorySettings()
        {
            var fileSettings = _fileSettingService.GetInventoryFileSettings();

            return Json(fileSettings, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetInventorySetting(int vendorId)
        {
            var fileSetting = _fileSettingService.GetInventoryFileSettingByVendor(vendorId);

            return Json(fileSetting, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetUnConfiguredInventoryVendors()
        {
            var vendors = _vendorService.GetAllVendors();
            var configuredInventoryVendors = _fileSettingService.GetInventoryFileSettings().ToList();

            var unconfiguredVendors = new List<dynamic>();
            foreach (var vendor in vendors)
            {
                if (configuredInventoryVendors.Exists(x => x.VendorId == vendor.Id))
                    continue;

                unconfiguredVendors.Add(new { Id = vendor.Id, Name = vendor.Name });
            }

            return Json(unconfiguredVendors, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _SaveInventorySetting(FileSettingViewModel model, int modelId)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }

            if (modelId == -1)
            {
                // set the file path and transfer path from the web.config
                model.FilePath = ConfigurationManager.AppSettings["FilePath"];
                model.TransferPath = ConfigurationManager.AppSettings["TransferPath"];

                _fileSettingService.CreateInventoryFileSetting(model);
            }
            else
            {
                // do the update for the file setting
                _fileSettingService.UpdateInventoryFileSetting(modelId, model);
            }

            return Json(new { Success = "Inventory file setting has been successfully saved!"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _DeleteInventorySetting(int vendorId)
        {
            try
            {
                _fileSettingService.DeleteInventoryFileSetting(vendorId);

                return Json(new { Success = "Inventory file setting has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting inventory file setting! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) });
            }
        }
        #endregion
    }
}