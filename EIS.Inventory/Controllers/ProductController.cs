using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using EIS.Inventory.Core.Managers;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductTypeService _productTypeService;
        private readonly IVendorService _vendorService;
        private readonly ISystemJobService _systemJobService;
        private readonly ICompanyService _companyService;
        private readonly IImageHelper _imageHelper;
        private readonly IMarketplaceInventoryManager _inventoryManager;
        private readonly IMarketplaceProductManager _productManager;
        private readonly IReportTemplateService _reportTemplateManager;
        private readonly IVendorProductLinkService _productLinkService;
        private readonly IExportDataService _exportDataService;
        private readonly string _systemJobsRoot;
        private readonly ISavedSearchFilterService _savedSearchFilterService;

        public ProductController(IProductService productService,
            IProductTypeService productTypeService,
            IVendorService vendorService,
            ISystemJobService jobService,
            ICompanyService companyService,
            IImageHelper imageHelper,
            IMarketplaceInventoryManager inventoryManager,
            IMarketplaceProductManager productManager,
            IReportTemplateService reportTemplateManager,
            IVendorProductLinkService productLinkService,
            IExportDataService exportDataService,
            ISavedSearchFilterService savedSearchFilterService)
        {
            _productService = productService;
            _productTypeService = productTypeService;
            _vendorService = vendorService;
            _systemJobService = jobService;
            _companyService = companyService;
            _imageHelper = imageHelper;
            _inventoryManager = inventoryManager;
            _productManager = productManager;
            _reportTemplateManager = reportTemplateManager;
            _productLinkService = productLinkService;
            _exportDataService = exportDataService;
            _systemJobsRoot = ConfigurationManager.AppSettings["SystemJobsRoot"].ToString();
            _savedSearchFilterService = savedSearchFilterService;
        }

        #region Product's CRUD

        // GET: Product
        [AuthorizeRoles("Administrator", "CanViewProducts")]
        public ActionResult Index(string searchString, 
            int page = 1, 
            int pageSize = 10,
            int vendorId = -1,
            int companyId = -1,
            int inventoryQtyFrom = -1, 
            int inventoryQtyTo = -1,
            int productGroupId = -1,
            int withImages = -1,
            bool? isKit = null,
            SkuType? skuType = null,
            bool? isSKULinked = null,
            bool? isAmazonEnabled = null,
            bool? hasASIN = null )
        {
            var products = _productService.GetPagedProducts(page,
                pageSize,
                searchString,
                companyId,
                vendorId,
                inventoryQtyFrom,
                inventoryQtyTo,
                productGroupId,
                withImages,
                isKit,
                skuType,
                isSKULinked,
                isAmazonEnabled,
                hasASIN);

            ViewBag.SearchString = searchString;
            ViewBag.VendorId = vendorId;
            ViewBag.CompanyId = companyId;
            ViewBag.InventoryQtyFrom = inventoryQtyFrom;
            ViewBag.InventoryQtyTo = inventoryQtyTo;
            ViewBag.ProductGroupId = productGroupId;
            ViewBag.WithImages = withImages;
            ViewBag.IsKit = isKit;
            ViewBag.SkuType = skuType;
            ViewBag.IsSKULinked = isSKULinked;
            ViewBag.IsAmazonEnabled = isAmazonEnabled;
            ViewBag.HasASIN = hasASIN;

            ViewBag.SavedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Product, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList(); ;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedProducts", products);

            return View(products);
        }

        // GET: create
        [AuthorizeRoles("Administrator", "CanEditProducts")]
        public ActionResult Create()
        {
            // create view bag first for the vendors
            populateViewBags();

            return View(new ProductDto());
        }

        // GET: edit/xxx
        [AuthorizeRoles("Administrator", "CanEditProducts")]
        public ActionResult Edit(string id)
        {
            // create view bag first for the vendors
            populateViewBags();

            var product = _productService.GetProductByEisSKU(id);
            if (product == null)
                throw new Exception(string.Format("{0} product SKU was not found!", id));

            if (Request.IsAjaxRequest())
                return PartialView("_Eshopo", product);
            
            ViewBag.Message = TempData["Message"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(product);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditProducts")]
        public ActionResult Save(string id, ProductDto model)
        {
            // create view bag first for the vendors
            populateViewBags();
            var originpage = string.IsNullOrEmpty(id) ? "create" : "edit";
            model.ModifiedBy = User.Identity.Name;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                
                // return to the original page which the request came from
                return View(originpage, model);
            }

            if (string.IsNullOrEmpty(id))
            {
                // generate the EisSKU for this product
                // 1st get the StartSKUCode for this product via its Company
                var company = _companyService.GetCompany(model.CompanyId);
                var skuCodeStart = company.SKUCodeStart.Trim();
                var serialSkuCode = company.SearialSKUCode.Trim();

                // get the max EisSKU for this company
                var maxEisSKU = _productService.GetMaxEisSKUByCompany(model.CompanyId);
                if (!string.IsNullOrEmpty(maxEisSKU))
                    serialSkuCode = maxEisSKU.RightStartAt(skuCodeStart.Length);

                // get the next SKU code and assign it to the model
                serialSkuCode = EisHelper.GetNextCode(serialSkuCode);
                model.EisSKU = string.Format("{0}{1}", skuCodeStart, serialSkuCode);

                // check if the EisSKU exists
                if (_productService.IsEisSKUExists(model.EisSKU))
                {
                    ModelState.AddModelError("", string.Format("The \'{0}\' EIS SKU is already exist!", model.EisSKU));
                    return View(originpage, model);
                }

                // if we got this far, everything is OK
                _productService.SaveProduct(model);
            }
            else
            {
                // if we got this far, everything is OK
                _productService.UpdateProduct(id, model);
            }

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("Edit", new { id = model.EisSKU });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProducts")]
        public JsonResult _DeleteProduct(string id)
        {
            try
            {
                _productService.DeleteProduct(id);

                return Json(new { Success = "Product has been successfully deleted!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting product! - Message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProducts")]
        public JsonResult _DeleteProducts(MarketplaceFeed model)
        {
            try
            {
                // get product's EIS SKU
                var productEisSkus = _productService.GetProductsEisSku(model);

                var fileFullPath = string.Format("{0}\\BulkDeleteProducts_{1:yyyyMMdd_HHmmss}.txt", _systemJobsRoot, DateTime.Now);

                using (var streamWriter = new StreamWriter(fileFullPath))
                {
                    var writer = new CsvHelper.CsvWriter(streamWriter);
                    foreach (var eisSku in productEisSkus)
                    {
                        writer.WriteField(eisSku);
                        writer.NextRecord();
                    }
                }

                // create new job for the Amazon Get Info for the asins
                var systemJob = new SystemJobDto
                {
                    JobType = JobType.BulkDeleteProduct,
                    Parameters = fileFullPath,
                    SubmittedBy = User.Identity.Name
                };
                _systemJobService.CreateSystemJob(systemJob);

                return Json("Bulk deletion of products has been passed to EIS System Jobs for execution.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting products! - Message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult _GetProductItem(string eisSku)
        {
            var product = _productService.GetProductItemByEisSku(eisSku);
            if (product == null)
                return Json(new { Error = string.Format("Product with EIS SKU \'{0}\' was not found!", eisSku) }, JsonRequestBehavior.AllowGet);

            return Json(product, JsonRequestBehavior.AllowGet);
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
                    model.Discriminator = "Products";
                    var template = _reportTemplateManager.SaveTemplate(model);
                    status = "success";
                    description = "Template " + template.Name +  " successfully added";
                }
            }
            catch (Exception ex)
            {

                status = "Error";
                description = EisHelper.GetExceptionMessage(ex).ToString();
            }

            return Json(new { status = status,description = description}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _DeleteReportTemplate(int id)
        {
            string status = String.Empty;

            try
            {
                _reportTemplateManager.DeleteTemplate(id);
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
            var reportTemplates = _reportTemplateManager.GetReportTemplates().Where(x => x.Discriminator=="Products");
            return Json(new { templates = reportTemplates }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetReportTemplateById(int id)
        {
            var reportTemplate = _reportTemplateManager.GetReportTemplateById(id);
            return Json(new { tempate = reportTemplate }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Product's Images        
        [AuthorizeRoles("Administrator", "CanViewProducts")]
        public JsonResult _GetProductImage(long id)
        {
            var productImage = _productService.GetProductImage(id);

            return Json(productImage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetProductImages(string eisSku)
        {
            var images = _productService.GetProductImages(eisSku);

            return Json(images, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditProducts")]
        public ActionResult _SaveProductImage(string eisSKU, MediaContent model)
        {
            if (Request.Files.Count == 0)
                throw new ArgumentException("No image file attachment detected.");

            // parsed and save the image to a file
            var file = Request.Files[0];
            using (var ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                ms.Position = 0;
                model.Url = _imageHelper.SaveProductImage(eisSKU, ms.ToArray());
            }

            model.Type = "CUSTOM";
            if (model.Id == -1)
                _productService.AddProductImage(model);
            else
            {
                // delete the old image file
                var oldImage = _productService.GetProductImage(model.Id);
                _imageHelper.RemoveProductImage(oldImage.ParentId, oldImage.Url);

                _productService.UpdateProductImage(model.Id, model.Url, model.Caption);
            }

            return Json(new { isUploaded = true, message = "Image file has been successfully uploaded" }, "text/html");
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteProducts")]
        public JsonResult _DeleteProductImage(long id)
        {
            try
            {
                var productImage = _productService.DeleteProductImage(id);

                return Json(productImage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = string.Format("Unable to delete product image! Err Message: {0}", EisHelper.GetExceptionMessage(ex)) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Product Amazon
        [AuthorizeRoles("Administrator", "CanViewProductAmazons")]
        public ActionResult _GetProductAmazon(string id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction("edit", new { id = id });

            ViewBag.EisSKU = id;
            var product = _productService.GetProductAmazon(id) ?? new ProductAmazonDto();

            // get the EIS product info, to get its ASIN
            var eisProduct = _productService.GetProductByEisSKU(id);

            return PartialView("_Amazon", product);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditProductAmazons")]
        public ActionResult UpdateAmazonProduct(string id, ProductAmazonDto model)
        {
            // get the original eisProduct 
            var origAmazonProduct = _productService.GetProductAmazon(id);
            model.ModifiedBy = User.Identity.Name;
            if (origAmazonProduct == null)
                _productService.SaveProductAmazon(model);
            else
                _productService.UpdateProductAmazon(id, model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = id });
        }
        #endregion

        #region Product eBay
        [AuthorizeRoles("Administrator", "CanViewProducteBays")]
        public ActionResult _GetProducteBay(string id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction("edit", new { id = id });

            ViewBag.EisSKU = id;
            var eBay = _productService.GetProducteBay(id) ?? new ProducteBayDto();

            // populate the eBay categories for dropdown list
            //populateeBayCategories(eBay.CategoryId);

            return PartialView("_eBay", eBay);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditProducteBays")]
        public ActionResult UpdateProducteBay(string id, ProducteBayDto model)
        {
            try
            {
                // get the original product 
                var eBay = _productService.GetProducteBay(id);

                // try to determine the category ID for the eBay
                int categoryId = 0;
                Int32.TryParse(model.CategoryName, out categoryId);

                // try to look the value for category name if the category is null
                if (model.CategoryId == null)
                    model.CategoryId = categoryId != 0 ? (int?)categoryId : null;

                if (eBay == null)
                    _productService.SaveProducteBay(model);
                else
                    _productService.UpdateProducteBay(id, model);

                TempData["Message"] = "Changes have been successfully saved!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format("Error in saving eBay product. Exception Msg: {0}",
                    EisHelper.GetExceptionMessage(ex));
            }
            return RedirectToAction("edit", new { id = id });
        }

        [AuthorizeRoles("Administrator", "CanGeteBaySuggestedCategories")]
        public JsonResult _GeteBaySuggestedCategories(string keyword)
        {
            var categories = _productManager.GetSuggestedCategories("eBay", keyword);

            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Product Big Commerce

        [AuthorizeRoles("Administrator", "CanViewProductBigCommerces")]
        public ActionResult _GetProductBigCommerce(string id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction("edit", new { id = id });

            ViewBag.EisSKU = id;
            
            var bigCommerce = _productService.GetProductBigCommerce(id) ?? new ProductBigCommerceDto();

            var productSeoUrl = bigCommerce.ProductURL;
            ViewBag.BigCommerceSEOURL = "http://www.bigsavingz.com" + productSeoUrl;
            
            //populateBigCommerceCategories();
            populateBigCommerceMultipleCategories(bigCommerce.Categories);
            populateBigCommerceBrands();

            return PartialView("_BigCommerce", bigCommerce);
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditProductBigCommerces")]
        public ActionResult UpdateProductBigCommerce(string id, ProductBigCommerceDto model)
        {
            // get the original product 
            var bigCommerce = _productService.GetProductBigCommerce(id);
            model.ModifiedBy = User.Identity.Name;
            if (bigCommerce == null)
                _productService.SaveProductBigCommerce(model);
            else
                _productService.UpdateProductBigCommerce(id, model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = id });
        }

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanEditProductBigCommerces")]
        public JsonResult UpdateBigCommerceCustomFields(string id, List<BigCommerceCustomFieldDto> model)
        {
            var success = _productService.CreateOrUpdateBigCommerceCustomFields(model);
            
            return Json(new { id = id, success = success });
        }

        [AuthorizeRoles("Administrator", "CanEditProductBigCommerces")]
        public JsonResult _GetBigCommerceSuggestedCategories(string keyword)
        {
            var categories = _productManager.GetSuggestedCategories("BigCommerce", keyword);

            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Product's Feeds

        [AuthorizeRoles("Administrator", "CanViewProductBuycoms")]
        public ActionResult _GetProductBuycom(string id)
        {
            if (!Request.IsAjaxRequest())
                return RedirectToAction("edit", new { id = id });

            ViewBag.EisSKU = id;
            return PartialView("_Buycom");
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanGetAmazonProductInfo")]
        public JsonResult _GetProductsInfo(MarketplaceFeed model)
        {
            // get the products's ASINs
            var infoFeeds = _productService.GetProductInfoFeeds(model);

            // create the file to save the list of product's ASIN
            var fileFullPath = string.Format("{0}\\Products_ASINS_{1:yyyyMMdd_HHmmss}.txt", _systemJobsRoot, DateTime.Now);

            // write the asins to the file
            using (var streamWriter = new StreamWriter(fileFullPath))
            {
                var writer = new CsvHelper.CsvWriter(streamWriter);
                foreach (var info in infoFeeds)
                {
                    writer.WriteField(info.EisSKU);
                    writer.WriteField(info.ASIN);
                    writer.WriteField(info.UPC);
                    writer.NextRecord();
                }
            }

            // create new job for the Amazon Get Info for the asins
            var systemJob = new SystemJobDto
            {
                JobType = JobType.AmazonGetInfo,
                Parameters = fileFullPath,
                SubmittedBy = User.Identity.Name
            };
            _systemJobService.CreateSystemJob(systemJob);

            return Json(new { Success = "Amazon Get Info has been passed to EIS System Jobs for execution." }, 
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanGeteBaySuggestedCategories")]
        public JsonResult _GeteBayBulkSuggestedCategories(MarketplaceFeed model)
        {
            try
            {
                // get the product feed for bul eBay suggested categories
                var productKeywordFeeds = _productService.GeteBaySuggestedCategoryFeed(model);

                var fileFullPath = string.Format("{0}\\BulkeBaySuggestedCategories_{1:yyyyMMdd_HHmmss}.csv",
                    _systemJobsRoot,
                    DateTime.Now);

                using (var streamWriter = new StreamWriter(fileFullPath))
                {
                    var writer = new CsvHelper.CsvWriter(streamWriter);
                    foreach (var item in productKeywordFeeds)
                    {
                        writer.WriteField(item.EisSKU);
                        writer.WriteField(item.Keyword);
                        writer.NextRecord();
                    }
                }

                // create new job for eBay suggested categories
                var systemJob = new SystemJobDto
                {
                    JobType = JobType.BulkeBaySuggestedCategories,
                    Parameters = fileFullPath,
                    SubmittedBy = User.Identity.Name
                };
                var jobId = _systemJobService.CreateSystemJob(systemJob);

                return Json(new { Success = "Bulk eBay get suggested categories has been passed to EIS System Jobs for execution.",
                    JobId = jobId },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in getting bulk eBay suggested categories! - Message: " + EisHelper.GetExceptionMessage(ex) },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult DownloadBulkeBaySuggestedCategoriesResult(int jobId)
        {
            // get the system job details
            var systemJob = new SystemJobDto();
            do
            {
                // let's sleep a while to give time the system job for eBay bull categories
                try
                {
                    Thread.Sleep(1000);
                }
                catch { }

                // get the job info
                systemJob = _systemJobService.GetSystemJob(jobId);

                // send out the file result if the system job is done
                if (systemJob.Status == JobStatus.Completed)
                {
                    var fileInfo = new FileInfo(systemJob.ParametersOut);
                    Response.Clear();
                    Response.ContentType = "text/csv";
                    Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                    Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0}\"", fileInfo.Name));
                    Response.TransmitFile(fileInfo.FullName);
                    Response.Flush();
                    Response.End();
                }

            } while (systemJob.Status != JobStatus.Completed || systemJob.Status != JobStatus.Failed);

            return null;
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanGetAmazonProductInfo")]
        public JsonResult _GetProductInfo(string marketplace, string eisSku)
        {
            var productInfo = _productManager.GetMarketplaceProductInfo(marketplace, eisSku);
            if (productInfo == null)
                return Json(new { Error = string.Format("Unable to find the product \'{0}\' in \'{1}\'", eisSku, marketplace) }, JsonRequestBehavior.AllowGet);

            return Json(productInfo, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPostProductListingFeeds")]
        public async Task<JsonResult> _SubmitProductFeed(MarketplaceFeed model)
        {
            await _inventoryManager.SubmitProductsListingFeedAsync(model, User.Identity.Name);

            return Json(new { Success = "Product feeds have been successfully posted to marketplaces" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPostProductListingFeeds")]
        public async Task<JsonResult> _SubmitSingleProductFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmitSingleProductListingFeed(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "Product update feed has been successfully posted to marketplace!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPostProductReviseFeeds")]
        public async Task<JsonResult> _SubmitProductReviseFeed(MarketplaceFeed model)
        {
            await _inventoryManager.SubmitProductsReviseFeedAsync(model, User.Identity.Name);

            return Json(new { Success  = "Product revise feed have been successfully posted to marketplace." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPostProductReviseFeeds")]
        public async Task<JsonResult> _SubmitSingleProductReviseFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmitSingleProductReviseFeed(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "Product revise feed has been successfully posted to marketplace!" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPostProductInventoryFeeds")]
        public async Task<JsonResult> _SubmitInventoryFeed(MarketplaceFeed model)
        {
            await _inventoryManager.SubmitInventoryFeedAysnc(model, User.Identity.Name);

            return Json(new { Success = "Inventory feeds have been successfully posted to marketplaces" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPostProductInventoryFeeds")]
        public async Task<JsonResult> _SubmitSingleInventoryFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmitInventoryFeedBySkuAsync(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "Inventory feed has been successfully posted to marketplace" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPostProductPriceFeeds")]
        public async Task<JsonResult> _SubmitPriceFeed(MarketplaceFeed model)
        {
            await _inventoryManager.SubmitPriceFeedAsync(model, User.Identity.Name);

            return Json(new { Success = "Price feeds have been successfully posted to marketplaces" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPostProductPriceFeeds")]
        public async Task<JsonResult> _SubmitSinglePriceFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmitPriceFeedBySkuAsync(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "Price feed has been successfully posted to marketplace" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPosteBayProductListingFeeds")]
        public JsonResult _SubmiteBayEndProductListing(MarketplaceFeed model)
        {
            try
            {
                var productItems = _productService.GeteBayItemFeeds(model).ToList();

                var fileFullPath = string.Format("{0}\\eBayProductEndListing_{1:yyyyMMdd_HHmmss}.csv",
                    _systemJobsRoot,
                    DateTime.Now);

                using (var streamWriter = new StreamWriter(fileFullPath))
                {
                    var writer = new CsvHelper.CsvWriter(streamWriter);
                    foreach (var item in productItems)
                    {
                        writer.WriteField(item.EisSKU);
                        writer.WriteField(item.ItemId);
                        writer.NextRecord();
                    }
                }

                // create new job for eBay suggested categories
                var systemJob = new SystemJobDto
                {
                    JobType = JobType.eBayProductsEndItem,
                    Parameters = fileFullPath,
                    SupportiveParameters = model.Mode,
                    SubmittedBy = User.Identity.Name
                };
                var jobId = _systemJobService.CreateSystemJob(systemJob);

                return Json(new
                {
                    Success = "eBay EndItem has been passed to EIS System Jobs for execution.",
                    JobId = jobId
                },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in submitting eBay EndItem! - Message: " + EisHelper.GetExceptionMessage(ex) },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPosteBayProductListingFeeds")]
        public async Task<JsonResult> _SubmitSingleEndItemFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmiteBayEndItemFeedBySkuAsync(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "EndItem feed has been successfully posted to marketplace" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanPosteBayProductReListingFeeds")]
        public JsonResult _SubmiteBayProductReListing(MarketplaceFeed model)
        {
            try
            {
                // get the eBay products for the relisting
                var productItems = _productService.GeteBayItemFeeds(model).ToList();

                var fileFullPath = string.Format("{0}\\eBayProductReListing_{1:yyyyMMdd_HHmmss}.csv",
                    _systemJobsRoot,
                    DateTime.Now);

                using (var streamWriter = new StreamWriter(fileFullPath))
                {
                    var writer = new CsvHelper.CsvWriter(streamWriter);
                    foreach (var item in productItems)
                    {
                        writer.WriteField(item.EisSKU);
                        writer.WriteField(item.ItemId);
                        writer.NextRecord();
                    }
                }

                // create new job for eBay suggested categories
                var systemJob = new SystemJobDto
                {
                    JobType = JobType.eBayProductsReListing,
                    Parameters = fileFullPath,
                    SupportiveParameters = model.Mode,
                    SubmittedBy = User.Identity.Name
                };
                var jobId = _systemJobService.CreateSystemJob(systemJob);

                return Json(new
                {
                    Success = "eBay Product ReListing has been passed to EIS System Jobs for execution.",
                    JobId = jobId
                },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in submitting eBay product relisting! - Message: " + EisHelper.GetExceptionMessage(ex) },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanPostBigCommerceProductListingFeeds")]
        public async Task<JsonResult> _SubmitBigCommerceSingleEndItemFeed(string marketplace, string eisSku)
        {
            await _inventoryManager.SubmitEndItemFeedBySkuAsync(marketplace, eisSku, User.Identity.Name);

            return Json(new { Success = "EndItem feed has been successfully posted to marketplace" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetMappedProductTypes()
        {
            var mappedProductTypes = _productTypeService.GetMappedProductTypes();

            return Json(mappedProductTypes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetMarketplaceChannels()
        {
            var channelNames = _inventoryManager.GetMarketplaceInventoryChannels();

            return Json(channelNames.Select(name => new { Id = name, Name = name }),
                JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult _UpdateBigCommerceCategories()
        {
            var bcOrderService = GetBigCommerceService();

            var success = bcOrderService.UpdateEISBigCommerceCategory();

            if (success)
            {
                return Json(new { Success = "Update of the categories successful."}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = "Update of the categories failed." }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult _UpdateBigCommerceBrands()
        {
            var bcOrderService = GetBigCommerceService();

            var success = bcOrderService.UpdateEISBigCommerceBrand();

            if (success)
            {
                return Json(new { Success = "Update of the brands successful." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = "Update of the brands failed." }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult _GetBigCommerceProductCustomFields(string EisSKU)
        {
            var customFieldList = new List<BigCommerceCustomFieldDto>();

            var bcOrderService = GetBigCommerceService();
            
            var product = _productService.GetProductBigCommerce(EisSKU);
            
            if(product != null && product.ProductId.HasValue)
            {
                var list = bcOrderService.GetProductCustomFields(product.ProductId.Value);
                if (list != null)
                    customFieldList = list.ToList();
            }

            return Json(new { customFieldList = customFieldList }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region eisProduct file upload and download

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanExportProducts")]
        public ActionResult CustomExportProducts(ExportProduct model)
        {
            var fileName =  _exportDataService.CustomExportProducts(model);
            var fileInfo = new FileInfo(fileName);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportProducts.csv\"", model.RequestedDate));
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
                EnumSavedSearchFilters.Product,
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
                SavedSearchFilterId = Convert.ToInt32(EnumSavedSearchFilters.Product),
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
            var savedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Product, User.Identity.Name).Select(x => new SelectListItem
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
        [AuthorizeRoles("Administrator", "CanExportProducts")]
        public ActionResult ExportBigCommerceCategoriesReport()
        {
            var fileName = _exportDataService.ExportBigCommerceCategories();
            var fileInfo = new FileInfo(fileName);
            var currentDate = DateTime.Now;

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_ExportBigCommerceCategories.csv\"", currentDate));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }

        #endregion

        #region helper methods
        private void populateViewBags(object selected = null)
        {
            // get the list of companies
            var companies = _companyService.GetAllCompanies();
            ViewBag.CompanyList = new SelectList(companies, "Id", "Name", null);
        }

        private void populateeBayCategories(object selected = null)
        {
            var categories = _productTypeService.GeteBayCategories();
            ViewBag.eBayCategoryList = new SelectList(categories, "Id", "OptionName", selected);
        }

        private void populateBigCommerceCategories(object selected = null)
        {
            var categories = _productTypeService.GetBigCommerceCategories().OrderBy(o => o.ParentName).ThenBy(o => o.Name).ToList();

            ViewBag.BigCommerceCategoryList = new SelectList(categories, "Id", "OptionName", "ParentName", selected);
        }

        private void populateBigCommerceMultipleCategories(string categories)
        {
            var categoryNames = new StringBuilder();

            if (!string.IsNullOrEmpty(categories))
            {
                var categoryList = categories.Split(',').ToList();
                var bigcommerceService = GetBigCommerceService();

                foreach (var category in categoryList)
                {
                    var name = bigcommerceService.GetCategoryNameById(Convert.ToInt32(category));
                    var parentList = bigcommerceService.GetEISParentHierarchy(Convert.ToInt32(category));
                    categoryNames.AppendFormat("{0} - {1} > {2} | ", category, string.Join(" > ", parentList.ToArray()), name);
                }
            }

            ViewBag.BigCommerceCategoryMultipleList = categoryNames.ToString();
        }

        private void populateBigCommerceBrands(object selected = null)
        {
            var bcOrderService = GetBigCommerceService();
            var brandList = bcOrderService.GetEISBrandList().OrderBy(o => o.Name).ToList();

            ViewBag.BigCommerceBrandList = new SelectList(brandList, "Id", "OptionName", selected);
        }

        private BigCommerceService GetBigCommerceService()
        {
            var bcOrderService = new BigCommerceService();

            return bcOrderService;
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            _productService.Dispose();
            _companyService.Dispose();
            _vendorService.Dispose();
            _productTypeService.Dispose();
            _systemJobService.Dispose();
            _productLinkService.Dispose();
            _exportDataService.Dispose();
            base.Dispose(disposing);
        }
    }
}