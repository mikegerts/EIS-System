using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ProductTypeController : Controller
    {
        private readonly IProductTypeService _service;

        public ProductTypeController(IProductTypeService service)
        {
            _service = service;
        }

        // GET: ProductType
        [AuthorizeRoles("Administrator", "CanViewProductTypes")]
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeRoles("Administrator", "CanViewProductTypes")]
        public JsonResult _GetProductTypes()
        {
            var productTypes = _service.GetAllProductTypes();

            return Json(productTypes, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewProductTypes")]
        public JsonResult _GetProductType(int id)
        {
            var productType = _service.GetProductType(id);

            return Json(productType, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetAmazonMainCategories()
        {
            var categories = _service.GetAmazonMainCategories();

            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetAmazonSubCategories(string parentCode)
        {
            var subCategories = _service.GetAmazonSubCategories(parentCode);

            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditProductTypes")]
        public JsonResult _SaveProductType(int modelId, ProductTypeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                return Json(new { Error = string.Join("<br/>", errors) }, JsonRequestBehavior.AllowGet);
            }

            if (modelId == -1)
            {
                // create new eisProduct type
                viewModel = _service.CreateProductType(viewModel);
            }
            else
            {
                // do the update for the file setting
                viewModel = _service.UpdateProductType(modelId, viewModel);
            }

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProductTypes")]
        public JsonResult _DeleteProductType(int id)
        {
            try
            {
                _service.DeleteProductType(id);

                return Json(new { Success = "Product type has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting product type! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }
    }
}