using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using X.PagedList;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class VendorProductService : IVendorProductService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;
        private readonly IImageHelper _imageHelper;

        public VendorProductService(IImageHelper imageHelper, ILogService logger)
        {
            _logger = logger;
            _imageHelper = imageHelper;
            _context = new EisInventoryContext();
        }

        public IPagedList<VendorProductListDto> GetPagedVendorProducts(int page,
            int pageSize,
            string searchString,
            int vendorId,
            int companyId,
            int withEisSKULink,
            int inventoryQtyFrom,
            int inventoryQtyTo,
            int withImages)
        {
            var products = searchVendorProducts(searchString,
                vendorId,
                companyId,
                withEisSKULink,
                inventoryQtyFrom,
                inventoryQtyTo,
                withImages);

            return products
                .OrderBy(x => x.EisSupplierSKU)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<vendorproduct, VendorProductListDto>();
        }

        public List<string> GetVendorProductsEisSupplierSKUs(VendorProductFilterDto param)
        {
            IEnumerable<vendorproduct> vendorProducts = null;
            if (param.IsAllSelectedItems)
            {
                if (!string.IsNullOrEmpty(param.SearchString))
                    vendorProducts = _context.vendorproducts
                        .Where(x => x.SupplierSKU.Contains(param.SearchString)
                            || x.Name.Contains(param.SearchString));
                else
                    vendorProducts = searchVendorProducts(param.SearchString,
                        param.VendorId ?? -1,
                        param.CompanyId ?? -1,
                        param.WithEisSKULink ?? -1,
                        param.QuantityFrom ?? -1,
                        param.QuantityTo ?? -1,
                      param.withImages,
                        param.ExcludedModelIds);
            }
            else
            {
                vendorProducts = _context.vendorproducts.Where(x => param.SelectedModelIds.Contains(x.EisSupplierSKU)
                    && (!param.ExcludedModelIds.Any() || param.ExcludedModelIds.Contains(x.EisSupplierSKU)));
            }

            return vendorProducts
                .Select(x => x.EisSupplierSKU)
                .ToList();
        }

        public VendorProductDto GetVendorProduct(string eisSupplierSKU)
        {
            var result = _context.vendorproducts
                .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU);
            if (result == null)
                return null;

            return Mapper.Map<VendorProductDto>(result);
        }

        public bool CreateVendorProduct(VendorProductDto model)
        {
            try
            {
                // get its vendor's info and if it's always on stock, use it alwaysQuantity
                var vendor = _context.vendors.FirstOrDefault(x => x.Id == model.VendorId);
                if (vendor.IsAlwaysInStock)
                    model.Quantity = vendor.AlwaysQuantity ?? 0;

                var product = Mapper.Map<vendorproduct>(model);
                product.Created = DateTime.UtcNow;
                product.CreatedBy = model.ModifiedBy;

                _context.vendorproducts.Add(product);
                _context.SaveChanges();

                // generated id
                model.EisSupplierSKU = product.EisSupplierSKU;

                return true;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorProductService, errorMsg, ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return false;
            }
        }

        public bool UpdateVendorProduct(string eisSupplierSKU, VendorProductDto model)
        {
            try
            {
                var existingProduct = _context.vendorproducts
                    .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU);
                if (existingProduct == null)
                    return false;

                // get its vendor's info and if it's always on stock, use it alwaysQuantity
                var vendor = existingProduct.vendor;
                if (vendor.IsAlwaysInStock)
                    model.Quantity = vendor.AlwaysQuantity ?? 0;

                // reflect the changes from model
                Mapper.Map(model, existingProduct);
                existingProduct.ModifiedBy = model.ModifiedBy;
                existingProduct.Modified = DateTime.UtcNow;
                _context.SaveChanges();

                return true;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorProductService, errorMsg, ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return false;
            }
        }

        public bool DeleteVendorProduct(string eisSupplierSKU)
        {
            var product = _context.vendorproducts
                .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU);
            if (product == null)
                return true;

            // remove first its product links
            _context.vendorproductlinks.RemoveRange(product.vendorproductlinks.ToList());

            _context.vendorproducts.Remove(product);
            _context.SaveChanges();

            return true;
        }

        public bool IsEisSupplierSKUExists(string eisSupplierSKU)
        {
            var result = _context.vendorproducts
               .FirstOrDefault(x => x.EisSupplierSKU == eisSupplierSKU);

            return result != null;
        }

        public IEnumerable<ProductResultDto> SearchEisProducts(string keyword)
        {
            return _context.products
                .Where(x => x.EisSKU.Contains(keyword)
                    || x.UPC.Contains(keyword)
                    || x.Category.Contains(keyword)
                    || x.Name.Contains(keyword))
                .Take(50)
                .ProjectTo<ProductResultDto>();
        }

        public IEnumerable<VendorProductResultDto> SearchVendorProducts(string keyword)
        {
            return _context.vendorproducts
                .Where(x => x.EisSupplierSKU.Contains(keyword)
                        || x.SupplierSKU.Contains(keyword)
                        || x.UPC.Contains(keyword)
                        || x.Category.Contains(keyword)
                        || x.Name.Contains(keyword))
                .Take(50)
                .ProjectTo<VendorProductResultDto>();
        }

        private IQueryable<vendorproduct> searchVendorProducts(string searchString,
            int vendorId,
            int companyId,
            int withEisSKULink,
            int inventoryQtyFrom,
            int inventoryQtyTo,
            int withImages,
            List<string> excludedModelIds = null)
        {
            excludedModelIds = excludedModelIds ?? new List<string>();
            var products = _context.vendorproducts
                .Where(x => (string.IsNullOrEmpty(searchString)
                            || x.EisSupplierSKU.Contains(searchString)
                            || x.SupplierSKU.Contains(searchString)
                            || x.UPC.Contains(searchString)
                            || x.Category.Contains(searchString)
                            || x.Name.Contains(searchString))
                    && (vendorId == -1 || x.VendorId == vendorId)
                    && ((inventoryQtyFrom == -1 || inventoryQtyTo == -1) || (x.Quantity >= inventoryQtyFrom && x.Quantity <= inventoryQtyTo))
                    && (!excludedModelIds.Any() || excludedModelIds.Contains(x.EisSupplierSKU))
                    && (withImages == -1 || (withImages == 0 && !_context.vendorproductimages.Any(y => y.EisSupplierSKU == x.EisSupplierSKU))
                    || (withImages == 1 && _context.vendorproductimages.Any(y => y.EisSupplierSKU == x.EisSupplierSKU))))
                .OrderBy(x => x.EisSupplierSKU);

            // check if do we need to return with EIS SKU links
            if (withEisSKULink == -1)
                return products.AsQueryable();
            else
            {
                var hasEisLink = withEisSKULink == 1;
                return products.Where(p => p.vendorproductlinks.Any(x => x.IsActive) == hasEisLink)
                    .AsQueryable();
            }
        }

        #region methods for Console apps
        public UploadResultType UpdateEisProductLinks(string eisSupplierSKU, string upc, int minPack)
        {
            if (string.IsNullOrEmpty(upc))
                return UploadResultType.NoChanges;

            // get the EIS products with have the same UPC code
            var compatibleEisProductSKUs = getCompatibleEisProductSKUs(upc, minPack);
            var uploadResult = new VendorProductUploadResult();

            // update the vendor product links
            updateVendorProductLinks(eisSupplierSKU, compatibleEisProductSKUs);
            uploadResult.UpdatedEisSupplierSKUs.Add(eisSupplierSKU);

            return UploadResultType.UpdatedItem;
        }

        public string AddLinkAndCreateEisProductIfNoMatchWithUPC(VendorProduct vendorProduct)
        {
            // get all EIS products which have the same UPC with the vendor product
            var compatibleEisProducts = getEisProductsByUpc(vendorProduct.UPC);
            var newEisSKULinks = new List<string>();

            // if there are no compatible EIS products then create a new one
            if (!compatibleEisProducts.Any())
            {
                // create new EIS product and link
                var linkEisSKU = createNewEisProductAndLink(vendorProduct, null /**No ParentSKU/EisSKU yet**/);
                newEisSKULinks.Add(linkEisSKU);
            }
            else
            {
                if (vendorProduct.MinPack > 1)
                {
                    // get all shadows for the compatible products
                    var shadows = compatibleEisProducts.SelectMany(x => x.shadows).ToList();
                    var hasSimilarEisSku = false;

                    // determine if the vendor product MinPack can be divisble by Shadow Factor Quantity
                    foreach (var shadow in shadows)
                    {
                        // create a product link to this shadow product(ShadowSKU) if it is divisible
                        if (shadow.FactorQuantity % vendorProduct.MinPack == 0)
                        {
                            createVendorProductLinkIfNotExist(shadow.ShadowSKU, vendorProduct.EisSupplierSKU);
                            newEisSKULinks.Add(shadow.ShadowSKU);
                        }

                        // check if has already EIS product with the same factor quantity as with vendorproduct's min pack
                        if (shadow.FactorQuantity == vendorProduct.MinPack)
                            hasSimilarEisSku = true;
                    }

                    // create shadow product with factor quanity equal to vendorproduct min pack
                    if (!hasSimilarEisSku)
                    {
                        var parentSKU = compatibleEisProducts.FirstOrDefault().EisSKU;
                        var linkEisSKU = createNewEisProductAndLink(vendorProduct, parentSKU);
                        newEisSKULinks.Add(linkEisSKU);
                    }
                }
                else
                {
                    // the vendor prouct min pack is 1, so let's add it to all EIS Normal and Shadow product since 1 is compatible to its factor quantity
                    foreach (var product in compatibleEisProducts)
                    {
                        createVendorProductLinkIfNotExist(product.EisSKU, vendorProduct.EisSupplierSKU);
                        newEisSKULinks.Add(product.EisSKU);
                    }
                }
            }

            // delete the old exiting vendor product links
            DeleteOldVendorProductLinks(vendorProduct.EisSupplierSKU, newEisSKULinks);

            return string.Join(",", newEisSKULinks);
        }

        public void DeleteOldVendorProductLinks(string eisSupplierSKU, List<string> compatibleEisProductSKUs)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                // get the existing product links which are not in compatible EIS SKUs
                var oldProductLinks = context.vendorproductlinks
                    .Where(x => x.EisSupplierSKU == eisSupplierSKU && !compatibleEisProductSKUs.Contains(x.EisSKU))
                    .ToList();

                // we have to mark IsActive to FALSE since for some reason we are unable to delete the links in here
                oldProductLinks.ForEach(x => x.IsActive = false);

                // save the changes
                context.SaveChanges();
            }
        }

        public string GetVendorProductSKU(VendorProduct product)
        {
            if (!string.IsNullOrEmpty(product.EisSupplierSKU))
                return product.EisSupplierSKU;

            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            // get the EisSupplierSKU by its supplier SKU and vendorId
            using (var context = new EisInventoryContext())
            {
                var result = context.vendorproducts
                    .FirstOrDefault(x => x.SupplierSKU == product.SupplierSKU && x.VendorId == product.VendorId);

                return result == null ? null : result.EisSupplierSKU;
            }
        }

        public int DoUpadateOrInsertVendorProduct(VendorProduct model, bool isToUpdate, string submittedBy)
        {
            try
            {
                // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
                using (var context = new EisInventoryContext())
                {
                    if (isToUpdate)
                    {
                        // get the existing vendor product
                        var product = context.vendorproducts.FirstOrDefault(x => x.EisSupplierSKU == model.EisSupplierSKU);
                        if (product == null)
                            return 0;

                        // set the new quantity for this vendor product
                        if (model.IsQuantitySet)
                        {
                            // get the number of order items which are Unshipped/Pending for this item
                            var pendingQtyOrders = context.orderproducts.Where(x => x.EisSupplierSKU == model.EisSupplierSKU)
                                .Join(context.orderitems,
                                        op => op.OrderItemId,
                                        oi => oi.OrderItemId,
                                        (op, oi) => new { OrderProduct = op, OrderItem = oi })
                               .Join(context.orders.Where(x => x.OrderStatus == OrderStatus.Unshipped || x.OrderStatus == OrderStatus.Pending),
                                        ooi => ooi.OrderItem.OrderId,
                                        o => o.OrderId,
                                        (ooi, o) => new { ooi.OrderProduct })
                               .Select(x => x.OrderProduct)
                               .DefaultIfEmpty<orderproduct>()
                               .Sum(x => (x == null ? 0 : x.Quantity));

                            // deduct the availability for this item with its pending orders
                            product.Quantity = model.Quantity - pendingQtyOrders;
                        }

                        if (model.Name != null) product.Name = model.Name;
                        if (model.Description != null) product.Description = model.Description;
                        if (model.IsSupplierPriceSet) product.SupplierPrice = model.SupplierPrice;
                        if (model.IsMinPackSet) product.MinPack = model.MinPack;
                        if (model.UPC != null) product.UPC = model.UPC;
                        if (model.Category != null) product.Category = model.Category;
                        if (model.Weight != null) product.Weight = model.Weight;
                        if (model.WeightUnit != null) product.WeightUnit = model.WeightUnit;
                        if (model.Shipping != null) product.Shipping = model.Shipping;
                        if (model.VendorMOQ != null) product.VendorMOQ = model.VendorMOQ;
                        if (model.VendorMOQType != null) product.VendorMOQType = model.VendorMOQType;
                        if (model.IsAutoLinkToEisSKUSet) product.IsAutoLinkToEisSKU = model.IsAutoLinkToEisSKU;

                        product.Modified = DateTime.UtcNow;
                        product.ModifiedBy = submittedBy;
                        context.SaveChanges();

                        // let's set some properties; assuming that only EisSupplierSKU is supplied
                        // these data will be used when creating new EisSKU
                        model.Name = product.Name;
                        model.Description = product.Description;
                        model.ShortDescription = product.ShortDescription;
                        model.Category = product.Category;
                        model.UPC = product.UPC;
                        model.SubmittedBy = submittedBy;
                    }
                    else
                    {
                        var product = new vendorproduct();
                        CopyObject.CopyFields(model, product);
                        product.Created = DateTime.UtcNow;
                        product.CreatedBy = submittedBy;

                        // add it to the context and save
                        context.vendorproducts.Add(product);
                        context.SaveChanges();
                    }
                    UpdateVendorProductImages(model.Images, model.EisSupplierSKU);
                }
                return 1;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker, errorMsg, ex.StackTrace);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker,
                    string.Format("Error in updating vendor product details -> SupplierSKU: {0} VendorId: {1}. <br/>Error message: {2}",
                    model.SupplierSKU, model.VendorId, EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return 0;
            }
        }


        public int DoUpdateVendorInventoryProduct(VendorProduct model,int vendorId, string submittedBy)
        {
            try
            {
                // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
                using (var context = new EisInventoryContext())
                {
                    // get the existing vendor product
                    var product = context.vendorproducts.FirstOrDefault(x => x.EisSupplierSKU == model.EisSupplierSKU && x.VendorId == vendorId);
                    if (product == null)
                        return 0;

                    product.Quantity = model.Quantity;

                    product.Modified = DateTime.UtcNow;
                    product.ModifiedBy = submittedBy;
                    context.SaveChanges();
                }
                return 1;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker, errorMsg, ex.StackTrace);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.VendorProductFileUploadWorker,
                    string.Format("Error in updating vendor product details -> SupplierSKU: {0} VendorId: {1}. <br/>Error message: {2}",
                    model.SupplierSKU, model.VendorId, EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                return 0;
            }
        }

        public void ZeroOutVendorProductQuantity(long? vendorId, List<string> eisSupplierSKUs, string modifiedBy)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                try
                {
                    // get the products to zero-out its quantity
                    var vendorProducts = context.vendorproducts
                        .Where(x => x.VendorId == vendorId && !eisSupplierSKUs.Contains(x.EisSupplierSKU))
                        .ToList();

                    vendorProducts.ForEach(p =>
                    {
                        p.Quantity = 0;
                        p.Modified = DateTime.UtcNow;
                        p.ModifiedBy = modifiedBy;
                    });

                    // save the changes
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(LogEntryType.VendorProductService,
                        string.Format("Error in zeroing the vendor products: VendorId: {0} Error message: {1}", vendorId, EisHelper.GetExceptionMessage(ex)),
                        ex.StackTrace);
                }
            }
        }

        public string GetVendorStartSku(int vendorId)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                var vendor = context.vendors.FirstOrDefault(x => x.Id == vendorId);
                if (vendor == null)
                    return string.Empty;

                return vendor.SKUCodeStart.Trim();
            }
        }

        public string GetNextEisSKUForCompany(int companyId)
        {
            string eisSKU = null;
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                // 1st get the StartSKUCode for this product via its Company
                var company = context.companies.FirstOrDefault(x => x.Id == companyId);
                if (company == null)
                    return null;

                // get the SKU start code and also its serial SKU code...
                var skuCodeStart = company.SKUCodeStart.Trim();
                var serialSkuCode = company.SearialSKUCode.Trim();

                // get the max EisSKU for this company
                var maxEisSKU = context.products
                    .Where(x => x.CompanyId == companyId && x.SkuType == SkuType.Normal)
                    .Max(x => x.EisSKU);

                // get the serial SKU code for this EIS SKU
                if (!string.IsNullOrEmpty(maxEisSKU))
                    serialSkuCode = maxEisSKU.RightStartAt(skuCodeStart.Length);

                // get the next SKU code and assign it to the model
                serialSkuCode = EisHelper.GetNextCode(serialSkuCode);
                eisSKU = string.Format("{0}{1}", skuCodeStart, serialSkuCode);
            }

            return eisSKU;
        }

        public vendor GetVendorById(int vendorId)
        {
            return _context.vendors.FirstOrDefault(x => x.Id == vendorId);
        }
        #endregion

        private void updateVendorProductLinks(string eisSupplierSKU, List<string> compatibleEisProductSKUs)
        {
            // iterate to all compatile EIS products and see if there already existing vendor product link
            foreach (var eisSKU in compatibleEisProductSKUs)
                createVendorProductLinkIfNotExist(eisSKU, eisSupplierSKU);

            // delete the old vendor product links
            DeleteOldVendorProductLinks(eisSupplierSKU, compatibleEisProductSKUs);
        }

        private string createNewEisProductAndLink(VendorProduct vendorProduct, string parentSKU)
        {
            // let's determine the company via its vendor information
            var companyId = getCompanyId(vendorProduct.VendorId);

            var newEisSKU = parentSKU;
            if (string.IsNullOrEmpty(parentSKU))
            {
                // get  the next EisSKU code for this company
                newEisSKU = GetNextEisSKUForCompany(companyId);

                // create first new EIS product
                createNewEisProduct(newEisSKU, companyId, SkuType.Normal, vendorProduct);
            }

            // create shadow product if min pack is greater than 1
            if (vendorProduct.MinPack > 1)
            {
                var shadowSKU = string.Format("{0}_{1}", newEisSKU, vendorProduct.MinPack);
                createNewEisProduct(shadowSKU, companyId, SkuType.Shadow, vendorProduct);

                // create shadow record for this product
                createShadow(newEisSKU, shadowSKU, vendorProduct.MinPack);

                // link the vendor product to the shadow product created
                createVendorProductLinkIfNotExist(shadowSKU, vendorProduct.EisSupplierSKU);
                newEisSKU = shadowSKU; // for reporting
            }
            else
            {
                // otherwise, just create a link for the normal EIS product
                createVendorProductLinkIfNotExist(newEisSKU, vendorProduct.EisSupplierSKU);
            }

            return newEisSKU;
        }

        private void createVendorProductLinkIfNotExist(string eisSKU, string eisSupplierSKU)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                // let's check first if this combination already exist
                var vendorProductLink = context.vendorproductlinks
                    .FirstOrDefault(x => x.EisSKU == eisSKU && x.EisSupplierSKU == eisSupplierSKU);
                if (vendorProductLink != null)
                {
                    vendorProductLink.IsActive = true;
                }
                else
                {
                    // otherwise, create a new one
                    context.vendorproductlinks.Add(new vendorproductlink
                    {
                        EisSKU = eisSKU,
                        EisSupplierSKU = eisSupplierSKU,
                        IsActive = true,
                        Created = DateTime.UtcNow
                    });
                }

                context.SaveChanges();
            }
        }

        private void createShadow(string parentSKU, string shadowSKU, int minPack)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                context.shadows.Add(new shadow
                {
                    ParentSKU = parentSKU,
                    ShadowSKU = shadowSKU,
                    FactorQuantity = minPack,
                    IsConnected = true
                });

                context.SaveChanges();
            }
        }

        private void createNewEisProduct(string eisSKU, int companyId, SkuType skuType, VendorProduct vendorProduct)
        {
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                // let's create EIS product first
                context.products.Add(new product
                {
                    EisSKU = eisSKU,
                    CompanyId = companyId,
                    Name = vendorProduct.Name,
                    Description = vendorProduct.Description,
                    Category = vendorProduct.Category,
                    UPC = vendorProduct.UPC,
                    SkuType = skuType,
                    IsBlacklisted = false,
                    IsKit = false,
                    Created = DateTime.UtcNow,
                    CreatedBy = vendorProduct.SubmittedBy
                });

                // save first the new EIS product
                context.SaveChanges();
            }
        }

        private List<product> getEisProductsByUpc(string upc)
        {
            var results = new List<product>();
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                // removed any leading ZEROs from the UPC
                var trimmedUPC = upc.TrimStart('0');
                results = context.products
                    .Include("Shadows")
                    .Where(x => (x.UPC.StartsWith("0") && x.UPC.Contains(trimmedUPC))
                        || (upc.StartsWith("0") && x.UPC.Contains(trimmedUPC))
                        || (x.UPC == upc))
                    .ToList();
            }

            return results;
        }

        private List<string> getCompatibleEisProductSKUs(string upc, int minPack)
        {
            var results = new List<string>();
            var compatibleProducts = getEisProductsByUpc(upc);

            // return empty list if there are no compatible products
            if (!compatibleProducts.Any())
                return results;

            // if the vendor product's min pack is greater than 1, get products' shadows
            if (minPack > 1)
            {
                var shadows = compatibleProducts.SelectMany(x => x.shadows).ToList();

                // iterate to each shadow items and determine if its FactoryQty is divisible by minpack
                foreach (var shadow in shadows)
                    if (shadow.FactorQuantity % minPack == 0)
                        results.Add(shadow.ShadowSKU);
            }
            else
            {
                // min pack is 1 so let return all compatible EIS Products which its Factor Qty is divisible by 1
                results = compatibleProducts.Select(x => x.EisSKU).ToList();
            }

            return results;
        }

        private int getCompanyId(int vendorId)
        {
            var companyId = 0;
            // NEED TO CREATE NEW DB CONTEXT SINCE THIS METHOD IS SHARED TO CONSOLE SERVICES
            using (var context = new EisInventoryContext())
            {
                var vendor = context.vendors.FirstOrDefault(x => x.Id == vendorId);
                if (vendor != null)
                    companyId = vendor.CompanyId;
                else
                {
                    var company = context.companies.FirstOrDefault(x => x.IsDefault);
                    companyId = company.Id;
                }
            }
            return companyId;
        }

        public List<vendorproduct> GetProductsByVendorId(int vendorId)
        {
            return _context.vendorproducts.Where(x => x.VendorId == vendorId).ToList();
        }

        #region Product Images
        public IEnumerable<MediaContent> GetVendorProductImages(string eisSupplierSku)
        {
            var productImages = _context.vendorproductimages
               .Where(x => x.EisSupplierSKU == eisSupplierSku);

            var images = new List<MediaContent>();
            foreach (var item in productImages)
            {
                images.Add(new MediaContent
                {
                    Id = item.Id,
                    ParentId = item.EisSupplierSKU,
                    Url = _imageHelper.GetVendorProductImageUri(eisSupplierSku, item.FileName),
                    Caption = item.Caption,
                });
            }

            return images;
        }

        public MediaContent GetVendorProductImage(long id)
        {
            var image = _context.vendorproductimages.FirstOrDefault(x => x.Id == id);
            if (image == null) return null;

            return new MediaContent
            {
                Id = image.Id,
                ParentId = image.EisSupplierSKU,
                Url = _imageHelper.GetVendorProductImageUri(image.EisSupplierSKU, image.FileName),
                Caption = image.Caption,
            };
        }

        public bool DeleteVendorProductImage(long id)
        {
            var image = _context.vendorproductimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return true;

            // delete the image from database first
            var eisSKU = image.EisSupplierSKU;
            _context.vendorproductimages.Remove(image);
            _context.SaveChanges();

            // then the file
            _imageHelper.RemoveProductImage(eisSKU, image.FileName);

            return true;
        }

        public void UpdateVendorProductImages(List<MediaContent> imageUrls, string eisSku)
        {
            try
            {
                // get the Amazon images
                var amazonImages = _context.vendorproductimages
                    .Where(x => x.EisSupplierSKU == eisSku)
                    .ToList();
                foreach (var image in amazonImages)
                {
                    // delete first the image file from the directory
                    _imageHelper.RemoveVendorProductImage(eisSku, image.FileName);

                    // then the image record from the database
                    _context.vendorproductimages.Remove(image);
                }

                // let's save the changes first for the deleted images
                _context.SaveChanges();


                foreach (var media in imageUrls)
                {
                    // download the image from net and save it to the file system
                    var fileName = _imageHelper.SaveVendorProductImage(eisSku, media.Url);
                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    // add the database to the database
                    addVendorProductImage(fileName, eisSku, 99, "Amazon Large Image", media.Type);
                }

                // save the images
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }
        public void UpdateVendorProductImages(List<string> imageUrls, string eisSku)
        {
            // check if there's any image URLs
            if (imageUrls == null || !imageUrls.Any())
                return;

            try
            {
                using (var context = new EisInventoryContext())
                {
                    // get the Amazon images
                    var amazonImages = context.vendorproductimages
                        .Where(x => x.EisSupplierSKU == eisSku)
                        .ToList();
                    foreach (var image in amazonImages)
                    {
                        // delete first the image file from the directory
                        _imageHelper.RemoveVendorProductImage(eisSku, image.FileName);

                        // then the image record from the database
                        context.vendorproductimages.Remove(image);
                    }

                    // let's save the changes first for the deleted images
                    context.SaveChanges();

                    // download and add the images
                    foreach (var url in imageUrls)
                    {
                        // download the image from net and save it to the file system
                        var fileName = _imageHelper.SaveVendorProductImage(eisSku, url);
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        // add the database to the database
                        context.vendorproductimages.Add(new vendorproductimage
                        {
                            EisSupplierSKU = eisSku,
                            Caption = "Amazon",
                            FileName = fileName,
                            Order_ = 99
                        });
                    }

                    // save the images
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonGetInfoWorker, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }



        public void AddVendorProductImage(MediaContent image)
        {
            addVendorProductImage(image.Url, image.ParentId, 99, image.Caption, image.Type);
        }

        public void UpdateVendorProductImage(long id, string fileName, string caption)
        {
            var image = _context.vendorproductimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return;

            image.FileName = fileName;
            image.Caption = caption;

            _context.SaveChanges();
        }

        private void addVendorProductImage(string fileName, string eisSku, int order, string caption, string imageType, bool isPush = false)
        {
            var productImage = new vendorproductimage
            {
                EisSupplierSKU = eisSku,
                Caption = caption,
                FileName = fileName,
                Order_ = order
            };

            _context.vendorproductimages.Add(productImage);

            if (!isPush)
                _context.SaveChanges();
        }


        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _logger.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}