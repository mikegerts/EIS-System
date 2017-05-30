using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using CsvHelper;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ProductGroupController : Controller
    {
        private readonly IProductGroupService _service;

        public ProductGroupController(IProductGroupService service)
        {
            _service = service;
        }

        // GET: ProductGroup
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var groups = _service.GetPagedProductGroups(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_PagedProductGroups", groups);

            return View(groups);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetAllProductGroups()
        {
            var groups = _service.GetAllProductGroups();

            return Json(groups, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetProductGroupDetails(long id, int page = 1, int pageSize = 10)
        {
            var groupDetail = _service.GetProductGroupDetails(id, page, pageSize);
            if (groupDetail == null)
                return Json(new { Error = string.Format("Product Group with ID \'{0}\' was not found!", id) }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                Id = groupDetail.Id,
                Name = groupDetail.Name,
                Description = groupDetail.Description,
                FirstItemOnPage = groupDetail.Products.FirstItemOnPage,
                HasNextPage = groupDetail.Products.HasNextPage,
                HasPreviousPage = groupDetail.Products.HasPreviousPage,
                IsFirstPage = groupDetail.Products.IsFirstPage,
                IsLastPage = groupDetail.Products.IsLastPage,
                LastItemOnPage = groupDetail.Products.LastItemOnPage,
                PageCount = groupDetail.Products.PageCount,
                PageNumber = groupDetail.Products.PageNumber,
                PageSize = groupDetail.Products.PageSize,
                TotalItemCount = groupDetail.Products.TotalItemCount,
                Items = groupDetail.Products
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewProductGroups")]
        public JsonResult _GetProductItems(long groupId, int page = 1, int pageSize = 10)
        {
            var productItems = _service.GetProductsByGroup(groupId)
                .OrderBy(x => x.EisSKU)
                .ToPagedList(page, pageSize);

            return Json(new
            {
                Items = productItems,
                CurrentPageIndex = productItems.CurrentPageIndex,
                EndItemIndex = productItems.EndItemIndex,
                StartItemIndex = productItems.StartItemIndex,
                TotalItemCount = productItems.TotalItemCount,
                TotalPageCount = productItems.TotalPageCount,
                ModelId = groupId
            }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditProductGroups")]
        public JsonResult _SaveProductGroup(long id, ProductGroupDetailDto model)
        {
            var result = new ProductGroupDetailDto();
            if (id > 0)
                result = _service.UpdateProductGroup(id, model);
            else
                result = _service.CreateProductGroup(model);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProductGroups")]
        public JsonResult _DeleteProductGroup(long id)
        {
            try
            {
                _service.DeleteProductGroup(id);

                return Json(new { Success = "Product Group has been successfully deleted!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting product group! - Message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpPost]
        [AuthorizeRoles("Administrator", "CanUploadProducts")]
        public ActionResult UploadProductGroup(int groupId)
        {
            var eisSkusList = new List<string>();
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
                        eisSkusList.Add(csvReader.GetField<string>(0));

                    totalSkus = eisSkusList.Count();
                }

                // load the products to the database
                var affectedProducts = _service.UpdateProductGroupEisSKUs(groupId, eisSkusList);
                message = string.Format("Upload completed: {0}/{1} EIS SKUs have been added to the product group!", affectedProducts, totalSkus);
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