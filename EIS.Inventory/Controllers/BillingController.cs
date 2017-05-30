using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly IBillingService _service;

        public BillingController(IBillingService service)
        {
            _service = service;
        }

        // GET: Billing
        [AuthorizeRoles("Administrator", "CanViewBillings")]
        public ActionResult Index(int page = 1,
            int pageSize = 10,
            int vendorId = -1,
            PaymentStatus paymentStatus = PaymentStatus.All,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var orders = _service.GetPurchaseOrders(page, pageSize, vendorId, paymentStatus, fromDate, toDate)
                .OrderByDescending(x => x.Created)
                .ToPagedList(page, pageSize);

            // set the ViewBag values
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PaymentStatus = paymentStatus;

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPagedPurchaseOrders", orders);
            
            return View(orders);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewBillings")]
        public ActionResult Search(string searchString, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(searchString))
                return RedirectToAction("Index", new { pageSize = pageSize });

            var orders = _service.GetPurchaseOrdersContainsBy(searchString)
                .OrderByDescending(x => x.Created)
                .ToPagedList(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPagedPurchaseOrders", orders);

            ViewBag.SearchString = searchString;

            return View("Index", orders);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanEditBillings")]
        public JsonResult _GetPurchaseOrder(string id)
        {
            var order = _service.GetPurchaseOrder(id);

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanEditBillings")]
        public JsonResult _GetPurchaseOrderItems(string poId, int page = 1)
        {
            var orderItems = _service.GetPurchaseOrderItems(poId)
                .OrderBy(x => x.Id)
                .ToPagedList(page, 10);

            return Json(new
            {
                Items = orderItems,
                CurrentPageIndex = orderItems.CurrentPageIndex,
                EndItemIndex = orderItems.EndItemIndex,
                StartItemIndex = orderItems.StartItemIndex,
                TotalItemCount = orderItems.TotalItemCount,
                TotalPageCount = orderItems.TotalPageCount,
                ModelId = poId
            }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditBillings")]
        public JsonResult _UpdatePoItemsPaid(string poId, List<long> paidPoItems,List<long> unpaidPoItems)
        {
            var updatedPo = _service.UpdatePurchaseOrderItems(poId, paidPoItems, unpaidPoItems);

            return Json(updatedPo, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditBillings")]
        public JsonResult _SavePurchaseOrder(PurchaseOrderViewModel model)
        {
            try
            {
                _service.SavePurchaseOrder(model);

                return Json(new { Success = "Purchase Order has been successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in saving purchase order. Error Msg: " + EisHelper.GetExceptionMessage(ex) });
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditBillings")]
        public JsonResult _UpdatePurchaseOrder(string poId, PurchaseOrderViewModel model)
        {
            try
            {
                _service.UpdatePurchaseOrderItems(poId, model);

                return Json(new { Success = "Purchase Order has been successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in updating purchase order. Error Msg: " + EisHelper.GetExceptionMessage(ex) });
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteBillings")]
        public JsonResult _DeleteBillings(bool isSelectAllPages, List<string> billingIds)
        {
            _service.DeleteBillings(isSelectAllPages, billingIds);

            return Json(new { Success = "Bulk delete of the billings have been successfully executed!" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GeneratePurchaseOrderId()
        {
            var poId = string.Format("{0:MM}{0:dd}{0:yy}-{0:hh}{0:mm}{0:ss}", DateTime.UtcNow);

            return Json(poId, JsonRequestBehavior.AllowGet);
        }
    }
}