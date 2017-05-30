using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    public class VendorProductController : Controller
    {
        private readonly IVendorProductService _vendorProductService;
        private readonly IVendorService _vendorService;
        private readonly ICompanyService _companyService;
        private readonly ISystemJobService _systemJobService;
        private readonly IVendorProductLinkService _productLinkService;
        private readonly IExportDataService _exportDataService;
        private readonly IImageHelper _imageHelper;
        private readonly string _systemJobsRoot;
        private readonly ISavedSearchFilterService _savedSearchFilterService;

        public VendorProductController(IVendorProductService vendorProductService,
            IVendorService vendorService,
            ICompanyService companyService,
            ISystemJobService systemJobService,
            IVendorProductLinkService productLinkService,
            IExportDataService exportDataService, 
            IImageHelper imageHelper,
            ISavedSearchFilterService savedSearchFilterService)
        {
            _vendorProductService = vendorProductService;
            _vendorService = vendorService;
            _companyService = companyService;
            _systemJobService = systemJobService;
            _productLinkService = productLinkService;
            _exportDataService = exportDataService;
            _systemJobsRoot = ConfigurationManager.AppSettings["SystemJobsRoot"].ToString();
            _imageHelper = imageHelper;
            _savedSearchFilterService = savedSearchFilterService;
        }

        // GET: VendorProduct
        [AuthorizeRoles("Administrator", "CanViewVendorProducts")]
        public ActionResult Index(string searchString,
            int page = 1,
            int pageSize = 10,
            int vendorId = -1,
            int companyId = -1,
            int withEisSKULink = -1,
            int inventoryQtyFrom = -1,
            int inventoryQtyTo = -1,
            int withImages = -1)
        {
            var products = _vendorProductService.GetPagedVendorProducts(page,
                pageSize,
                searchString,
                vendorId,
                companyId,
                withEisSKULink,
                inventoryQtyFrom,
                inventoryQtyTo,
                withImages);

            ViewBag.SearchString = searchString;
            ViewBag.VendorId = vendorId;
            ViewBag.CompanyId = companyId;
            ViewBag.WithEisSKULink = withEisSKULink;
            ViewBag.InventoryQtyFrom = inventoryQtyFrom;
            ViewBag.InventoryQtyTo = inventoryQtyTo;
            ViewBag.WithImages = withImages;

            ViewBag.SavedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.VendorProduct, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_PagedVendorProducts", products);

            return View(products);
        }

        [AuthorizeRoles("Administrator", "CanEditVendorProducts")]
        public ActionResult Create()
        {
            // init the viewbags for dropdown
            populateViewBags();

            return View(new VendorProductDto());
        }

        [AuthorizeRoles("Administrator", "CanEditVendorProducts")]
        public ActionResult Edit(string id)
        {
            // create view bag first for the vendors
            populateViewBags(); 

            var product = _vendorProductService.GetVendorProduct(id);
            if (product == null)
                throw new Exception(string.Format("Vendor product with ID {0} was not found!", id));

            ViewBag.PreviousUrl = Request.UrlReferrer;
            ViewBag.Message = TempData["Message"];

            return View(product);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditVendorProducts")]
        public ActionResult Save(string id, VendorProductDto model)
        {
            // init the viewbags for dropdown
            populateViewBags();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));

                // return to the original page which the request came from
                var originpage = string.IsNullOrEmpty(id) ? "create" : "edit";      
                return View(originpage, model);
            }

            // set the IsAutoLinkToEisSKU if the IsCreateEisSKUAndLink set to TRUE
            if (model.IsCreateEisSKUAndLink)
                model.IsAutoLinkToEisSKU = true;

            // remove the trailing the trailing spaces
            model.SupplierSKU = model.SupplierSKU.Trim();
            model.Name = model.Name.Trim();
            model.ModifiedBy = User.Identity.Name;

            if (string.IsNullOrEmpty(id))
            {
                // get the vendor start SKU code
                var startSKUCode = _vendorService.GetVendorStartSku(model.VendorId);
                var eisSupplierSKU = string.Format("{0}{1}", startSKUCode.Trim(), model.SupplierSKU.Trim()).ToUpper();

                // let's determine first if this vendor product already exists
                if (_vendorProductService.IsEisSupplierSKUExists(eisSupplierSKU))
                {
                    ModelState.AddModelError("", string.Format("This vendor product with EisSupplierSKU: \'{0}\' is already exist!", eisSupplierSKU));
                    return View("create", model);
                }

                // set the EisSupplierSKU and save the product
                model.EisSupplierSKU = eisSupplierSKU;
                _vendorProductService.CreateVendorProduct(model);
            }
            else
            {
                // update the vendor product
                _vendorProductService.UpdateVendorProduct(id, model);
            }


            // check first if we want to auto-link and create new EIS product if it doesn't exist
            if (model.IsCreateEisSKUAndLink && !string.IsNullOrEmpty(model.UPC))
            {
                var vendorProduct = new VendorProduct();
                CopyObject.CopyFields(model, vendorProduct);
                _vendorProductService.AddLinkAndCreateEisProductIfNoMatchWithUPC(vendorProduct);
            }
            else
            {
                // check if there's a need to auto-link this vendor product with EIS product
                if (model.IsAutoLinkToEisSKU && !model.IsCreateEisSKUAndLink && !string.IsNullOrEmpty(model.UPC))
                    _vendorProductService.UpdateEisProductLinks(model.EisSupplierSKU, model.UPC, model.MinPack);
                else if (!model.IsAutoLinkToEisSKU)
                {
                    // delete the existing EIS product links if there's any
                    _vendorProductService.DeleteOldVendorProductLinks(model.EisSupplierSKU, new List<string>());
                }
            }

            // if we got this far, everything is OK
            TempData["Message"] = "Changes have been successfully saved!";

            return RedirectToAction("edit", new { id = model.EisSupplierSKU });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteVendorProducts")]
        public JsonResult _DeleteVendorProduct(string id)
        {
            try
            {
                _vendorProductService.DeleteVendorProduct(id);

                return Json(new { Success = "Vendor product has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting vendor product! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteVendorProducts")]
        public JsonResult _DeleteBulkVendorProducts(VendorProductFilterDto model)
        {
            try
            {
                // get product's EIS SKU
                var eisSupplierSKUs = _vendorProductService.GetVendorProductsEisSupplierSKUs(model);

                var fileFullPath = string.Format("{0}\\BulkDeleteVendorProducts_{1:yyyyMMdd_HHmmss}.txt", _systemJobsRoot, DateTime.Now);

                using (var streamWriter = new StreamWriter(fileFullPath))
                {
                    var writer = new CsvHelper.CsvWriter(streamWriter);
                    foreach (var sku in eisSupplierSKUs)
                    {
                        // just write the vendor product's Id which we need to delete
                        writer.WriteField(sku);
                        writer.NextRecord();
                    }
                }

                // create new job for the Amazon Get Info for the asins
                var systemJob = new SystemJobDto
                {
                    JobType = JobType.BulkDeleteVendorProduct,
                    Parameters = fileFullPath,
                    SubmittedBy = User.Identity.Name
                };
                _systemJobService.CreateSystemJob(systemJob);

                return Json("Bulk deletion of vendor products has been passed to EIS System Jobs for execution.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting vendor products! - Message: " + EisHelper.GetExceptionMessage(ex) },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult _SearchEisProducts(string keyword)
        {
            var results = _vendorProductService.SearchEisProducts(keyword);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewVendorProducts")]
        public JsonResult _GetEisProducLinks(string eisSupplierSKU)
        {
            var eisProudcts = _productLinkService.GetEisProductLinks(eisSupplierSKU);

            return Json(eisProudcts, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanAddEisProductLinks")]
        public JsonResult _AddEisProductLinks(string eisSupplierSKU, List<string> selectedEisSKUs)
        {
            var isSuccess = _productLinkService.AddEisProductLinks(eisSupplierSKU, selectedEisSKUs);

            return Json(new { IsSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteEisProductLinks")]
        public JsonResult _DeleteProductLink(string eisSKU, string eisSupplierSKU)
        {
            var isSuccess = _productLinkService.DeleteProductLink(eisSKU, eisSupplierSKU);

            return Json(new { IsSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }

        #region Vendor Product Links

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanAddEisProductLinks")]
        public JsonResult _UpdateVendorProductLinks(string eisSupplierSKU)
        {
            var product = _vendorProductService.GetVendorProduct(eisSupplierSKU);
            if(product == null)
                return Json(new { Error = "Unable to find vendor product." }, JsonRequestBehavior.AllowGet);

            _vendorProductService.UpdateEisProductLinks(product.EisSupplierSKU, product.UPC, product.MinPack);
            return Json(new { Success = "Vendor product links have been successfully updated." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanAddEisProductLinks")]
        public JsonResult _CreateEisSKUAndUpdateLinks(string eisSupplierSKU)
        {
            var product = _vendorProductService.GetVendorProduct(eisSupplierSKU);
            if (product == null)
                return Json(new { Error = "Unable to find vendor product." }, JsonRequestBehavior.AllowGet);

            var vendorProduct = new VendorProduct();
            CopyObject.CopyFields(product, vendorProduct);
            _vendorProductService.AddLinkAndCreateEisProductIfNoMatchWithUPC(vendorProduct);

            return Json(new { Success = "Vendor product links have been successfully updated." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteEisProductLinks")]
        public JsonResult _DeleteVendorProductLinks(string eisSupplierSKU)
        {
            var product = _vendorProductService.GetVendorProduct(eisSupplierSKU);
            if (product == null)
                return Json(new { Error = "Unable to find vendor product." }, JsonRequestBehavior.AllowGet);

            _vendorProductService.DeleteOldVendorProductLinks(product.EisSupplierSKU, new List<string>());
            return Json(new { Success = "Vendor product links have been successfully deleted." }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetVendorProdutLinks(string eisSKU)
        {
            var results = _productLinkService.GetVendorProductLinks(eisSKU);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _SearchVendorProducts(string keyword)
        {
            var results = _vendorProductService.SearchVendorProducts(keyword);

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanAddEisProductLinks")]
        public JsonResult _AddVendorProductLinks(string eisSKU, List<string> selectedEisSupplierSKUs)
        {
            var isSuccess = _productLinkService.AddVendorProductLinks(eisSKU, selectedEisSupplierSKUs);

            return Json(new { IsSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        [HttpPost]
        [AuthorizeRoles("Administrator", "CanExportVendorProducts")]
        public ActionResult CustomExportVendorProducts(ExportVendorProduct model)
        {
            var fileName = _exportDataService.CustomExportVendorProducts(model);
            var fileInfo = new FileInfo(fileName);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportVendorProducts.csv\"", model.RequestedDate));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }


        [HttpGet]
        [AuthorizeRoles("Administrator", "CanManageSearchFilter")]
        public JsonResult _ManageSearchFilter(int selectedSearchFilter, string filterName, string searchString)
        {
            var isNameExists = _savedSearchFilterService.IsFilterExist(selectedSearchFilter,
                EnumSavedSearchFilters.VendorProduct,
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
                SavedSearchFilterId = Convert.ToInt32(EnumSavedSearchFilters.VendorProduct),
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
            var savedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.VendorProduct, User.Identity.Name).Select(x => new SelectListItem
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


        protected override void Dispose(bool disposing)
        {
            _vendorProductService.Dispose();
            _vendorService.Dispose();
            _companyService.Dispose();
            _systemJobService.Dispose();
            _productLinkService.Dispose();
            _exportDataService.Dispose();
            base.Dispose(disposing);
        }

        private void populateViewBags(object selected = null)
        {
            // get the list of vendors
            var vendors = _vendorService.GetAllVendors();
            ViewBag.VendorList = new SelectList(vendors, "Id", "Name", selected);

            // get the list of companies
            var companies = _companyService.GetAllCompanies();
            ViewBag.CompanyList = new SelectList(companies, "Id", "Name", null);
        }

        #region Product's Images        
        [AuthorizeRoles("Administrator", "CanViewProducts")]
        public JsonResult _GetVendorProductImage(long id)
        {
            var productImage = _vendorProductService.GetVendorProductImage(id);

            return Json(productImage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetVendorProductImages(string eisSku)
        {
            var images = _vendorProductService.GetVendorProductImages(eisSku);

            return Json(images, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditProducts")]
        public ActionResult _SaveVendorProductImage(string eisSKU, MediaContent model)
        {
            if (Request.Files.Count == 0)
                throw new ArgumentException("No image file attachment detected.");

            // parsed and save the image to a file
            var file = Request.Files[0];
            using (var ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                ms.Position = 0;
                model.Url = _imageHelper.SaveVendorProductImage(eisSKU, ms.ToArray());
            }

            model.Type = "CUSTOM";
            if (model.Id == -1)
                _vendorProductService.AddVendorProductImage(model);
            else
            {
                // delete the old image file
                var oldImage = _vendorProductService.GetVendorProductImage(model.Id);

                var fileName = Path.GetFileName(oldImage.Url);

                _imageHelper.RemoveVendorProductImage(oldImage.ParentId, fileName);

                _vendorProductService.UpdateVendorProductImage(model.Id, model.Url, model.Caption);
            }

            return Json(new { isUploaded = true, message = "Image file has been successfully uploaded" }, "text/html");
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProducts")]
        public JsonResult _DeleteVendorProductImage(long id)
        {
            try
            {
                var productImage = _vendorProductService.DeleteVendorProductImage(id);

                return Json(productImage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = string.Format("Unable to delete product image! Err Message: {0}", EisHelper.GetExceptionMessage(ex)) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}