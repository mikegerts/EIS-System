using EIS.Inventory.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class CategoryMappingController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductTypeService _productTypeService;

        public CategoryMappingController(IProductService productService, IProductTypeService productTypeService)
        {
            _productService = productService;
            _productTypeService = productTypeService;
        }

        // GET: CategoryMapping
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeRoles("Administrator", "CanViewCategoryMappings")]
        public JsonResult _GetProductTypes()
        {
            var productTpyes = _productTypeService.GetAllProductTypes();

            return Json(productTpyes, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanEditCategoryMappings")]
        public JsonResult _GetProductMappedCategories(int id)
        {
            var mappedCategories = _productTypeService.GetProductCategoryMappings(id);

            return Json(mappedCategories, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanEditCategoryMappings")]
        public JsonResult _GetProductUnMappedCategories()
        {
            var unMappedCategories = _productTypeService.GetUnMappedProductCategories();

            return Json(unMappedCategories, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditCategoryMappings")]
        public JsonResult _AddProductCategories(int id, List<string> categories)
        {
            _productTypeService.AddProductCategories(id, categories);

            return Json(new { Success = "Categories have been successfully added!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCategoryMappings")]
        public JsonResult _DeleteMappedCategory(int id, string category)
        {
            _productTypeService.DeleteProductCategoryMapping(id, category);

            return Json(new { Success = "Category has been successfully deleted!" }, JsonRequestBehavior.AllowGet);
        }
    }
}