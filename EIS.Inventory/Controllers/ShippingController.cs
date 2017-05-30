using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.Managers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ShippingController : Controller
    {
        private readonly IShippingService _shippingService;
        private readonly IOrderService _orderService;
        private readonly IShippingRateManager _shippingRateManager;

        public ShippingController(IShippingService shippingService, IOrderService orderService, IShippingRateManager shippingManager)
        {
            _shippingService = shippingService;
            _orderService = orderService;
            _shippingRateManager = shippingManager;
        }

        // GET: Shipping
        [AuthorizeRoles("Administrator", "CanViewAwaitingShipments")]
        public ActionResult AwaitingShipment(string searchString, int page = 1, int pageSize = 10)
        {
            // get the awaiting shipment orders
            var results = _shippingService.GetAwaitingShipments(page, pageSize);

            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedAwaitingShipments", results);

            return View(results);
        }

        [AuthorizeRoles("Administrator", "CanViewAwaitingShipments")]
        public JsonResult _GetOrderProductDetail(string orderId)
        {
            var result = _shippingService.GetOrderProductDetailByOrderId(orderId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Shipping Location CRUD

        [AuthorizeRoles("Administrator", "CanViewShippingLocations")]
        public ActionResult ShippingLocation(int page = 1, int pageSize = 10)
        {
            var results = _shippingService.GetShippingLocations(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_PagedShippingLocations", results);

            return View(results);
        }

        [AuthorizeRoles("Administrator", "CanEditShippingLocations")]
        public JsonResult _GetShippingLocation(int id)
        {
            var result = _shippingService.GetShippingLocationById(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditShippingLocations")]
        public ActionResult _SaveShippingLocation(ShippingLocationDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }
            
            model.ModifiedBy = User.Identity.Name;
            
            if (model.HasId)
                _shippingService.UpdateShippingLocation(model.Id, model);
            else
                _shippingService.CreateShippingLocation(model);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteShippingLocations")]
        public JsonResult _DeleteShippingLocation(int id)
        {
            try
            {
                _shippingService.DeleteShippingLocation(id);

                return Json(new { Success = "Shipping Location has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting shipping location! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        [AuthorizeRoles("Administrator", "CanViewAwaitingShipments")]
        public JsonResult _GetAllShippingLocations()
        {
            var results = _shippingService.GetAllShippingLocations();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanRequestShipmentRates")]
        [HttpPost]
        public JsonResult _CalculateShipmentRates(string provider, Shipment shipment)
        {
            var results = _shippingRateManager.GetShipmentRates(provider, shipment);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            _shippingService.Dispose();
            base.Dispose(disposing);
        }
    }
}