using CsvHelper;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace EIS.Inventory.Controllers
{
    public class OrderGroupController : Controller
    {
        private readonly IOrderGroupService _service;
        public OrderGroupController(IOrderGroupService service)
        {
            _service = service;
        }
        // GET: OrderGroup
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public ActionResult Index(int page =1 , int pageSize =10)
        {
            var orderGroups = _service.GetAllOrderGroups()
                .OrderBy(x => x.Id)
                       .ToPagedList(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPageOrderGroups", orderGroups);

            return View(orderGroups);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetAllOrderGroups()
        {
            var orderGroups = _service.GetAllOrderGroups();

            return Json(orderGroups.Select(x => new { x.Id, x.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetOrderGroup(long id)
        {
            var orderGroup = _service.GetOrderGroup(id);

            if(orderGroup == null)
                return Json(new { Error = string.Format("Order Group with ID \'{0}\' was not found!", id) }, JsonRequestBehavior.AllowGet);

            return Json(orderGroup, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditProductGroups")]
        public JsonResult _SaveOrderGroup(long id, OrderGroupViewModel model)
        {
            var result = new OrderGroupViewModel();
            if (id == -1)
                result = _service.CreateOrderGroup(model);
            else
                result = _service.UpdateOrderGroup(id, model);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProductGroups")]
        public JsonResult _DeleteOrderGroup(long id)
        {
            try
            {
                _service.DeleteOrderGroup(id);

                return Json(new { Success = "Order Group has been successfully deleted!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting order group! - Message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetOrderItems(long id,int page =1)
        {
            var orderitems = _service.GetOrderByGroup(id)
                .OrderBy(x => x.EisOrderId)
                .ToPagedList(page,10);

            return Json(new
            {
                Items = orderitems,
                CurrentPageIndex = orderitems.CurrentPageIndex,
                EndItemIndex = orderitems.EndItemIndex,
                StartItemIndex = orderitems.StartItemIndex,
                TotalItemCount = orderitems.TotalItemCount,
                TotalPageCount = orderitems.TotalPageCount,
                ModelId = id
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanUploadProducts")]
        public ActionResult UploadOrderGroup(int groupId)
        {
            var eisOrderIds = new List<string>();
            var totalSkus = 0;
            var csvFile = Request.Files[0];
            var message = string.Empty;
            var isUploaded = true;

            try
            {
                using (var reader = new StreamReader(csvFile.InputStream))
                using (var csvReader = new CsvReader(reader))
                {
                    // tell CSV reader that file has no header
                    csvReader.Configuration.HasHeaderRecord = false;

                    while (csvReader.Read())
                        eisOrderIds.Add(csvReader.GetField<string>(0));

                    totalSkus = eisOrderIds.Count();
                }

                // load the products to the database
                var affectedOrderGroups = _service.UpdateOrderGroupEisOrderIds(groupId, eisOrderIds);
                message = string.Format("Upload completed: {0}/{1} EIS OrderIds have been added to the order group!", affectedOrderGroups, totalSkus);
            }
            catch (Exception ex)
            {
                isUploaded = false;
                message = string.Format("File upload failed: {0}", EisHelper.GetExceptionMessage(ex));
            }

            return Json(new { isUploaded = isUploaded, message = message }, "text/html");
        }

    }
}