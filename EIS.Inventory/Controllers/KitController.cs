using System.Collections.Generic;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class KitController : Controller
    {
        private readonly IKitService _service;

        public KitController(IKitService service)
        {
            _service = service;
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewKits")]
        public JsonResult _SearchProducts(string searchStr, int page = 1, int pageSize = 10)
        {
            var products = _service.GetProducts(page, pageSize, searchStr);

            return Json(new
            {
                FirstItemOnPage = products.FirstItemOnPage,
                HasNextPage = products.HasNextPage,
                HasPreviousPage = products.HasPreviousPage,
                IsFirstPage = products.IsFirstPage,
                IsLastPage = products.IsLastPage,
                LastItemOnPage = products.LastItemOnPage,
                PageCount = products.PageCount,
                PageNumber = products.PageNumber,
                PageSize = products.PageSize,
                TotalItemCount = products.TotalItemCount,
                Items = products
            }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewKits")]
        public ActionResult _GetKit(string id)
        {
            var kit = _service.GetKitByParentKitSku(id);
            ViewBag.EisSKU = id;

            if (!Request.IsAjaxRequest())
                return RedirectToAction("edit", "product", new { id = id });

            ViewBag.EisSKU = id;
            return PartialView("_Kit", kit ?? new KitDto { ParentKitSKU = id });
        }

        [AuthorizeRoles("Administrator", "CanViewKits")]
        public JsonResult _GetKitDetail(string parentKitSku, string childKitSku)
        {
            var kitDetail = _service.GetKitDetailByIds(parentKitSku, childKitSku);

            return Json(kitDetail, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewKits")]
        public JsonResult _GetKitDetailsByParentKitSku(string parentKitSku)
        {
            var kitDetails = _service.GetKitDetailsByParentKitSku(parentKitSku);

            return Json(kitDetails, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditKits")]
        public JsonResult _UpdateKit(string parentKitSku, KitDto model)
        {
            model = _service.UpdateKit(parentKitSku, model);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditKits")]
        public JsonResult _AddKitDetails(string parentKitSKU, List<KitDetailDto> models)
        {
            var kit = _service.SaveKitDetails(parentKitSKU, models);

            return Json(kit, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditKits")]
        public JsonResult _UpdateKitDetail(KitDetailDto model)
        {
            var kitDetail = _service.UpdateKitDetail(model);

            return Json(kitDetail, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteKits")]
        public JsonResult _DeleteKitDetail(string parentKitSku, string childKitSku)
        {
            var isSuccess = _service.DeleteKitDetail(parentKitSku, childKitSku);

            return Json(new { IsSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }
    }
}