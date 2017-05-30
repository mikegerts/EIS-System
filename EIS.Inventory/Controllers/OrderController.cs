using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Managers;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IMarketplaceOrdersManager _manager;
        private readonly IReportTemplateService _remportTemplateService;
        private readonly ISavedSearchFilterService _savedSearchFilterService;

        public OrderController(IOrderService service, IMarketplaceOrdersManager manager, IReportTemplateService templateService,
            ISavedSearchFilterService savedSearchFilterService)
        {
            _orderService = service;
            _manager = manager;
            _remportTemplateService = templateService;
            _savedSearchFilterService = savedSearchFilterService;
        }

        // GET: Order
        [AuthorizeRoles("Administrator", "CanViewOrders")]
        public ActionResult Index(
            string searchString,
            string shippingAddress,
            string shippingCity,
            string shippingCountry,
            int page = 1,
            int pageSize = 10,
            DateTime? orderDateFrom = null,
            DateTime? orderDateTo = null,
            DateTime? shipmentDateFrom = null,
            DateTime? shipmentDateTo = null,
            OrderStatus orderStatus = OrderStatus.None,
            //int vendorId = -1,
            int isExported = -1,
            string marketPlace = "",
            int paymentStatus = -1,
            int orderGroupId = -1
            )
        {
            var orders = _orderService.GetPagedOrders(
                page,
                pageSize,
                searchString,
                shippingAddress,
                shippingCity,
                shippingCountry,
                orderDateFrom,
                orderDateTo,
                shipmentDateFrom,
                shipmentDateTo,
                orderStatus,
                isExported,
                marketPlace,
                paymentStatus,
                orderGroupId);

            ViewBag.ShipmentDateFrom = shipmentDateFrom;
            ViewBag.ShipmentDateTo = shipmentDateTo;
            ViewBag.OrderDateFrom = orderDateFrom;
            ViewBag.OrderDateTo = orderDateTo;
            ViewBag.OrderStatus = (int)orderStatus;
            //ViewBag.VendorId = vendorId;
            ViewBag.IsExported = isExported;
            ViewBag.MarketPlace = marketPlace;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.ShippingAddress = shippingAddress;
            ViewBag.ShippingCity = shippingCity;
            ViewBag.ShippingCountry = shippingCountry;

            ViewBag.SavedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Order, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList();
            
            if (Request.IsAjaxRequest())
                return PartialView("_PagedOrders", orders);

            return View(orders);
        }

        [AuthorizeRoles("Administrator", "CanViewOrders")]
        public ActionResult OrderDetails(string id)
        {
            var order = _orderService.GetOrderById(id);

            ViewBag.PreviousUrl = Request.UrlReferrer;
            return View(order);
        }

        [AuthorizeRoles("Administrator", "CanViewOrders")]
        public ActionResult Details(string id)
        {
            var order = _orderService.GetOrderById(id);
            
            return View(order);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditOrders")]
        public ActionResult UpdateOrder(OrderViewModel model)
        {
            _orderService.UpdateOrder(model);

            return RedirectToAction("OrderDetails", new { id = model.OrderId });
        }
        
        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewOrders")]
        public JsonResult _GetOrder(string orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanImportMarketplaceOrders")]
        public JsonResult _ImportMarketplaceOrders(string marketplace, List<string> marketplaceOrderIds)
        {
            var result = _manager.ImportMarketplaceOrdersData(marketplace, marketplaceOrderIds);

            return Json(new { IsSuccess = result > 0, Message = string.Format("{0}/{1}", result, marketplaceOrderIds.Count) },
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanGetMarketplaceOrderData")]
        public JsonResult _GetLatestMarketplaceOrderData(string marketplace, string marketplaceOrderId)
        {
            var order = _manager.UpdateMarketplaceOrderData(marketplace, marketplaceOrderId);

            // this is done like because we want to format the date and also return as string for the orderstatus
            return Json(order == null ? null : new
            {
                BuyerName = order.BuyerName ?? string.Empty,
                ShippingAddressLine1 = order.ShippingAddressLine1 ?? string.Empty,
                OrderTotal = string.Format("${0}", order.OrderTotal),
                PurchaseDate = String.Format("{0:MM/dd/yyyy hh:mm tt}", order.PurchaseDate),
                OrderStatus = order.OrderStatus.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanToggleOrdersAsExported")]
        public JsonResult _ToggleOrdersExported(bool isExported, bool isSelectAllPages, List<int> eisOrderIds)
        {
            _orderService.ToggleOrderExportValue(isExported, isSelectAllPages, eisOrderIds);

            return Json(new { Success = "Orders' export status has been successfully updated" });
        }

        [HttpGet]
        public JsonResult _GetUnshippedOrderItems(string orderId)
        {
            var orderItems = _orderService.GetOrderUnshippedItems(orderId);

            var orderderShipment = new OrderShipmentViewModel();
            if (orderItems.Any())
            {
                orderderShipment.OrderItems = orderItems;
                orderderShipment.OrderId = orderItems.First().OrderId;
                orderderShipment.Marketplace = orderItems.First().Marketplace;
            }

            return Json(orderderShipment, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _GetShippingCarriers(string marketplace)
        {
            var carries = _manager.GetShippingCarriers(marketplace);
           

            return Json(carries, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPostOrdersShipmentFeed")]
        public JsonResult _PostOrderShipmentDetails(OrderShipmentViewModel model)
        {
            try
            {
                _manager.ConfirmOrderShipmentAsync(model, User.Identity.Name);

                return Json(new { Success = "Shipment has been successfully sent! The order status will reflect in a moment." });
            }
            catch (Exception)
            {
                return Json(new { Error = "Error in confirming order shipment for order ID: " + model.OrderId });
            }
        }

        public JsonResult _GetNextEisOrderId()
        {
            var maxEisOrderId = _orderService.GetNextEisOrderId();

            return Json(maxEisOrderId, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult _GetPendingOrders(string eisSupplierSKU)
        {
            var pendingOrders = _orderService.GetPendingOrders(eisSupplierSKU);

            return Json(pendingOrders, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanCreateManualOrders")]
        public JsonResult _SaveManualOrder(OrderViewModel orderModel)
        {
            try
            {
                _orderService.SaveManualOrder(orderModel);

                return Json(new { Success = "Manual Order has been successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in creating Manual Order <br/> Error message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanCreateManualOrders")]
        public JsonResult _UpdateManualOrder(string orderId, OrderViewModel orderModel)
        {
            try
            {
                _orderService.UpdateManualOrder(orderId, orderModel);

                return Json(new { Success = "Manual Order has been successfully saved!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in saving the changes for Manual Order <br/> Error message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult _UpdateOrderProducts(string orderId)
        {
            try
            {
                _orderService.UpdateOrderProducts(orderId);

                return Json(new { Success = "Order item/s have been successfully updated its order products." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in updating order products for this order. <br/> Error message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult _UnshippedOrder(string orderId, string marketplace)
        {
            try
            {
                _manager.UnshippedOrder(orderId, marketplace);

                return Json(new { Success = "Manual Order has been successfully unshipped!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in unshipping the Manual Order <br/> Error message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanCancelOrders")]
        public JsonResult _CancelOrder(string orderId, string marketplace)
        {
            try
            {
                _manager.CancelOrder(orderId, marketplace);

                return Json(new { Success = "Manual Order has been successfully cancelled!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in cancelling the Manual Order <br/> Error message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanExportOrders")]
        public ActionResult CustomExportOrders(ExportOrder model)
        {
            var fileName = _orderService.CustomExportOrderAsync(model);
            var fileInfo = new FileInfo(fileName);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_CustomExportOrders.csv\"", model.RequestedDate));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }


        [HttpPost]
        public JsonResult _SaveReportTemplate(ReportTemplateViewModel model)
        {
            string status = String.Empty;
            string description = String.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    model.Discriminator = "Orders";
                    var template = _remportTemplateService.SaveTemplate(model);
                    status = "success";
                    description = "Template " + template.Name + " successfully added";
                }
            }
            catch (Exception ex)
            {

                status = "Error";
                description = EisHelper.GetExceptionMessage(ex).ToString();
            }

            return Json(new { status = status, description = description }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _DeleteReportTemplate(int id)
        {
            string status = String.Empty;

            try
            {
                _remportTemplateService.DeleteTemplate(id);
                status = "success";
            }
            catch (Exception)
            {
                status = "Error";
            }

            return Json(new { status = status }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _GetReportTemplates()
        {
            var reportTemplates = _remportTemplateService.GetReportTemplates().Where(x => x.Discriminator == "Orders");
            return Json(new { templates = reportTemplates }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanCreateOrdersShipmentLabel")]
        public JsonResult _CreateShipmentLabel(string orderId)
        {
            var shipstationService = new ShipStationService();
            var base64ResultStringTask = shipstationService.CreateShipmentLabel(orderId);
            base64ResultStringTask.Wait();

            return Json(new { base64ResultString = base64ResultStringTask.Result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanManageSearchFilter")]
        public JsonResult _ManageSearchFilter(int selectedSearchFilter, string filterName, string searchString)
        {
            var isNameExists = _savedSearchFilterService.IsFilterExist(selectedSearchFilter,
                EnumSavedSearchFilters.Order,
                filterName, User.Identity.Name);
            if (isNameExists)
            {
                return Json(new { status = "error", message = "filter name already exists." }, JsonRequestBehavior.AllowGet);
            }


            var model = new SavedSearchFilterDto()
            {
                Created = DateTime.Now,
                CreatedBy = User.Identity.Name,
                Id = selectedSearchFilter,
                SavedSearchFilterId = Convert.ToInt32(EnumSavedSearchFilters.Order),
                SavedSearchFilterName = filterName,
                SearchString = searchString
            };
            if (model.Id == 0)
                _savedSearchFilterService.CreateSavedSearchFilter(model);
            else
                _savedSearchFilterService.UpdateSavedSearchFilter(model);

            return Json(new { status = "success", message = "filter search saved successfully." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanLoadSearchFilter")]
        public JsonResult _LoadSearchFilter()
        {
            var savedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Order, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList();

            return Json(new { status = "success", listItem = savedSearchFiltersList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanGetSearchFilter")]
        public JsonResult _GetSearchFilterValues(int Id)
        {
            var savedSearchFiltersList = _savedSearchFilterService.GetSavedSearchFilter(Id);

            return Json(new { status = "success", item = savedSearchFiltersList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanDeleteSearchFilter")]
        public JsonResult _DeleteSearchFilter(int Id)
        {
            try
            {
                _savedSearchFilterService.DeleteSavedSearchFilter(Id);

                return Json(new { status = "success", message = "Search filter has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Error in deleting search filter! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult _PostShipStation(string orderId)
        {
            var shipstationService = new ShipStationService();
            var result = 0;

            if (!shipstationService.IsOrderExistingInShipStation(orderId) && !shipstationService.IsTrackingExistOrderNumber(orderId))
            {
                var resultTask = shipstationService.PostOrderToShipStationByOrderNumber(orderId);
                resultTask.Wait();
                result = resultTask.Result;
            }

            return Json(new { result = result });
        }

        protected override void Dispose(bool disposing)
        {
            _orderService.Dispose();
            _remportTemplateService.Dispose();
            base.Dispose(disposing);
        }
    }
}