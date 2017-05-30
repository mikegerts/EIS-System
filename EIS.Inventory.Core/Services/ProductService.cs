using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using X.PagedList;
using AutoMapper;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using MySql.Data.MySqlClient;

namespace EIS.Inventory.Core.Services
{
    public class ProductService : IProductService, IDisposable
    {
        private readonly string _connectionString;
        private readonly EisInventoryContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly ILogService _logger;

        public ProductService(IImageHelper imageHelper, ILogService logger)
        {
            _context = new EisInventoryContext();
            _imageHelper = imageHelper;
            _logger = logger;
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        #region EIS eisProduct
        public IPagedList<ProductListDto> GetPagedProducts(int page,
            int pageSize,
            string searchString,
            int companyId,
            int vendorId,
            int inventoryQtyFrom,
            int inventoryQtyTo,
            int productGroupId,
            int withImages,
            bool? isKit,
            SkuType? skuType,
            bool? isSKULinked,
            bool? isAmazonEnabled,
            bool? hasASIN)
        {
            var templateQuery = createProductListQuery(searchString,
                productGroupId,
                companyId,
                vendorId,
                inventoryQtyFrom,
                inventoryQtyTo,
                withImages,
                isKit,
                skuType,
                isSKULinked,
                isAmazonEnabled,
                hasASIN);

            var totalItemCount = 0;
            var results = new List<ProductListDto>();
            using(var connection = new MySqlConnection(_connectionString))
            {
                // create query to get the total records found
                var recordQuery = string.Format(templateQuery, "COUNT(1)");
                var countReader = MySqlHelper.ExecuteReader(connection, CommandType.Text, recordQuery, null);
                while (countReader.Read())
                    totalItemCount = Convert.ToInt32(countReader[0]);

                // close the reader
                countReader.Close();

                // compute the pagination
                var currentPage = (page - 1) * pageSize;

                // now for the product fields
                var productQuery = string.Format(templateQuery, @"
                    p.EisSKU
                    , (SELECT vwa.EisSupplierSKU FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS EisSupplierSKU
                    , p.Name
                    , (SELECT vwa.VendorName FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS VendorName
                    , (SELECT CASE WHEN p.SkuType = 1 THEN FLOOR((vwa.Quantity * vwa.MinPack) / s.FactorQuantity) ELSE vwa.Quantity END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS Quantity
                    , (SELECT CASE WHEN p.SkuType = 1 THEN ROUND((vwa.SupplierPrice * (s.FactorQuantity / vwa.MinPack)), 2) ELSE vwa.SupplierPrice END FROM vw_availablevendorproducts vwa WHERE vwa.EisSKU = p.EisSKU LIMIT 1) AS SupplierPrice
                    , p.SellerPrice");
                var productReader = MySqlHelper.ExecuteReader(connection, CommandType.Text, string.Format("{0} ORDER BY p.EisSKU LIMIT {1}, {2}", productQuery, currentPage, pageSize), null);
                while (productReader.Read())
                {
                    var result = new ProductListDto();
                    result.EisSKU = productReader["EisSKU"].ToString();
                    result.EisSupplierSKU = productReader["EisSupplierSKU"].ToString();
                    result.Name = productReader["Name"].ToString();
                    result.VendorName = productReader["VendorName"].ToString();
                    result.Quantity = productReader["Quantity"] == DBNull.Value ? null : (int?)Convert.ToInt32(productReader["Quantity"]);
                    result.SupplierPrice = productReader["SupplierPrice"] as decimal?;
                    result.SellerPrice = productReader["SellerPrice"] as decimal?;
                    results.Add(result);
                }

            }

            return new StaticPagedList<ProductListDto>(results, page, pageSize, totalItemCount);
        }

        public IEnumerable<MarketplaceProductFeedDto> GetProductPostFeeds(MarketplaceFeed param)
        {
            var products = getProductFeed(param);
            var productList = products.Where(o => o.IsBlacklisted == false).ToList();

            // convert it first into product feed
            var productsFeed = Mapper.Map<List<MarketplaceProductFeedDto>>(productList);

            // iterate and get its images
            foreach (var product in productsFeed)
            {
                var images = GetProductImages(product.EisSKU);
                product.ImageUrls = images.Select(x => x.Url).ToList();

                var shadow = _context.shadows.FirstOrDefault(x => x.ShadowSKU == product.EisSKU);

                if (shadow != null)
                    product.FactorQuantity = shadow.FactorQuantity;
            }

            return productsFeed;
        }

        public IEnumerable<AmazonInfoFeed> GetProductInfoFeeds(List<string> eisSkus)
        {
            var products = _context.products.Where(x => eisSkus.Contains(x.EisSKU));
            if (!products.Any())
                return null;

            // just return products only with ASIN
            return products
                .Where(x => x.productamazon != null)
                .Select(x => new AmazonInfoFeed
                {
                    EisSKU = x.EisSKU,
                    ASIN = x.productamazon.ASIN,
                    UPC = x.UPC,
                    EAN = x.EAN
                });
        }

        public ProductDto GetProductByEisSKU(string eisSKU)
        {
            var product = _context.products
                .Include("shadows")
                .FirstOrDefault(x => x.EisSKU == eisSKU);
            var shadow = _context.shadows.FirstOrDefault(x => x.ShadowSKU == eisSKU);

            var productModel = Mapper.Map<product, ProductDto>(product);

            // if product is a shadow product then let' get its parent product
            if (product.SkuType == SkuType.Shadow && shadow != null)
            {
                productModel.FactorQuantity = shadow.FactorQuantity;
                productModel.ParentProductEisSKU = shadow.ParentSKU;
            }

            return productModel;
        }

        public ProductDto SaveProduct(ProductDto model)
        {
            try
            {
                // ensure the EIS SKU is upper cased
                model.EisSKU.ToUpper();
                var product = Mapper.Map<ProductDto, product>(model);
                product.CreatedBy = model.ModifiedBy;
                product.Created = DateTime.UtcNow;

                _context.products.Add(product);
                _context.SaveChanges();

                return Mapper.Map<product, ProductDto>(product);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return model;
            }
        }

        public ProductDto UpdateProduct(string eisSku, ProductDto model)
        {
            try
            {
                var existingProduct = _context.products.Find(eisSku);
                if (existingProduct == null)
                    return model;

                // reflect the changes from model
                var oldUpcCode = existingProduct.UPC;
                Mapper.Map(model, existingProduct);

                // if this product is Shadow product, let's not update its UPC
                if (existingProduct.SkuType == SkuType.Shadow)
                    existingProduct.UPC = oldUpcCode;

                existingProduct.ModifiedBy = model.ModifiedBy;
                existingProduct.Modified = DateTime.UtcNow;

                // save the changes first for the product
                _context.SaveChanges();

                if (existingProduct.SkuType == SkuType.Normal)
                {
                    // let's check if the product has shadow products
                    var shadows = _context.shadows.Where(x => x.ParentSKU == model.EisSKU).ToList();

                    // update its shadow products' supplier price, seller price and the quantity as well
                    // and also update its UPC code
                    if (shadows.Any())
                    {
                        var parentSellerPrice = (decimal)model.SellerPrice;
                        var parentWeight = Convert.ToInt32(model.AccurateWeight);

                        foreach (var shadow in shadows)
                        {
                            // get the shadow product details
                            var shadowProduct = _context.products.FirstOrDefault(x => x.EisSKU == shadow.ShadowSKU);
                            shadowProduct.UPC = model.UPC;
                            shadowProduct.SellerPrice = parentSellerPrice * shadow.FactorQuantity;

                            var accurateWeight = parentWeight * shadow.FactorQuantity;
                            shadowProduct.AccurateWeight = accurateWeight;
                            shadowProduct.AccurateWeightUnit = model.AccurateWeightUnit;
                            shadowProduct.GuessedWeight = accurateWeight;
                            shadowProduct.GuessedWeightUnit = model.GuessedWeightUnit;

                            var accurateRate = getShippingRate(accurateWeight);
                            shadowProduct.AccurateShipping = accurateRate.ToString();
                            shadowProduct.GuessedShipping = accurateRate.ToString();
                            shadowProduct.ModifiedBy = model.ModifiedBy;
                            shadowProduct.Modified = DateTime.UtcNow;
                        }

                        // let's save the changes
                        _context.SaveChanges();
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }

            return model;
        }

        private decimal getShippingRate(int accurateWeight)
        {
            decimal output = 0;

            var shippingRate = _context.shippingrates
                .FirstOrDefault(o => o.WeightFrom == accurateWeight ||
                        (o.WeightTo.HasValue && o.WeightTo != 0
                        && o.WeightFrom <= accurateWeight
                        && accurateWeight <= o.WeightTo));

            if (shippingRate != null)
            {
                output = shippingRate.Rate.Value;
            }

            return output;
        }

        public bool DeleteProduct(string eisSKU)
        {
            var isSuccessful = false;
            using (var context = new EisInventoryContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var imagesFileName = new List<string>();
                    try
                    {
                        // get the product first
                        var product = context.products.FirstOrDefault(x => x.EisSKU == eisSKU);
                        if (product == null)
                            return true;

                        // delete first the product amaon
                        var productAmazon = context.productamazons.FirstOrDefault(x => x.EisSKU == eisSKU);
                        if (productAmazon != null)
                            context.productamazons.Remove(productAmazon);

                        // delete the eBay product details  
                        var producteBay = context.productebays.FirstOrDefault(x => x.EisSKU == eisSKU);
                        if (producteBay != null)
                            context.productebays.Remove(producteBay);

                        // delete the bigcommerce product details
                        var productBigCommerce = context.productbigcommerces.FirstOrDefault(x => x.EisSKU == eisSKU);
                        if (productBigCommerce != null)
                            context.productbigcommerces.Remove(productBigCommerce);

                        // delete the product from product group details
                        var productGroupDetails = product.productgroupdetails.ToList();
                        foreach (var group in productGroupDetails)
                            product.productgroupdetails.Remove(group);

                        // delete first the child kit details
                        var childKitDetails = context.kitdetails.Where(x => x.ChildKitSKU == eisSKU);
                        context.kitdetails.RemoveRange(childKitDetails);

                        // delete the parent kit details
                        var parentKitDetails = context.kitdetails.Where(x => x.ParentKitSKU == eisSKU);
                        context.kitdetails.RemoveRange(parentKitDetails);

                        // then, the kits
                        var kits = context.kits.Where(x => x.ParentKitSKU == eisSKU);
                        context.kits.RemoveRange(kits);

                        // delete the child shadow
                        var childShadows = context.shadows.Where(x => x.ShadowSKU == eisSKU);
                        context.shadows.RemoveRange(childShadows);

                        // delete product from parent shadows
                        var parentShadows = context.shadows.Where(x => x.ParentSKU == eisSKU);
                        context.shadows.RemoveRange(parentShadows);

                        // delete its vendor product links
                        var vendorProductLinks = context.vendorproductlinks.Where(x => x.EisSKU == eisSKU);
                        context.vendorproductlinks.RemoveRange(vendorProductLinks);

                        // get the images first for this product
                        var images = context.productimages.Where(x => x.EisSKU == eisSKU);
                        imagesFileName = images.Select(x => x.FileName).ToList();
                        context.productimages.RemoveRange(images);

                        // lastly, the product
                        context.products.Remove(product);

                        // save the changes
                        context.SaveChanges();

                        // commit the changes
                        transaction.Commit();
                        isSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        isSuccessful = false;
                        transaction.Rollback();
                    }
                    finally
                    {
                        // delete the product images if it is successfull
                        if (isSuccessful)
                        {
                            imagesFileName.ForEach(fileName => _imageHelper.RemoveProductImage(eisSKU, fileName));
                        }
                    }
                }
            }

            return isSuccessful;
        }

        public MarketplaceProductFeedDto GetProductPostFeedByEisSku(string eisSku)
        {
            var product = _context.products.FirstOrDefault(x => x.EisSKU == eisSku);
            var productFeed = Mapper.Map<product, MarketplaceProductFeedDto>(product);

            // get the product images
            var images = GetProductImages(productFeed.EisSKU);
            productFeed.ImageUrls = images.Select(x => x.Url).ToList();

            // check if the product is a shadow
            var shadow = _context.shadows.FirstOrDefault(x => x.ShadowSKU == product.EisSKU);
            if (shadow != null)
                productFeed.FactorQuantity = shadow.FactorQuantity;

            return productFeed;
        }

        public List<string> GetProductsEisSku(MarketplaceFeed param)
        {
            // get the product items from the list of EIS SKUs
            var products = getProductFeed(param);

            return products.Select(x => x.EisSKU).ToList();
        }

        public bool IsEisSKUExists(string eisSku)
        {
            return _context.products.Count(x => x.EisSKU == eisSku) > 0;
        }

        public string GetMaxEisSKUByCompany(int companyId)
        {
            return _context.products
                .Where(x => x.CompanyId == companyId && x.SkuType == SkuType.Normal)
                .Max(x => x.EisSKU);
        }

        public IEnumerable<MediaContent> GetProductImages(string eisSKU)
        {
            var productImages = _context.productimages
                .Where(x => x.EisSKU == eisSKU && (x.ImageType == "CUSTOM" || x.ImageType == "LARGE"));

            var images = new List<MediaContent>();
            foreach (var item in productImages)
            {
                images.Add(new MediaContent
                {
                    Id = item.Id,
                    ParentId = item.EisSKU,
                    Url = _imageHelper.GetProductImageUri(eisSKU, item.FileName),
                    Caption = item.Caption,
                    Type = item.ImageType
                });
            }

            return images;
        }

        public MediaContent GetProductImage(long id)
        {
            var image = _context.productimages.FirstOrDefault(x => x.Id == id);
            if (image == null) return null;

            return new MediaContent
            {
                Id = image.Id,
                ParentId = image.EisSKU,
                Url = _imageHelper.GetProductImageUri(image.EisSKU, image.FileName),
                Caption = image.Caption,
                Type = image.ImageType,
            };
        }

        public void UpdateProductImages(List<MediaContent> imageUrls, string eisSku)
        {
            try
            {
                // get the Amazon images
                var amazonImages = _context.productimages
                    .Where(x => x.EisSKU == eisSku && x.ImageType != "CUSTOM")
                    .ToList();
                foreach(var image in amazonImages)
                {
                    // delete first the image file from the directory
                    _imageHelper.RemoveProductImage(eisSku, image.FileName);

                    // then the image record from the database
                    _context.productimages.Remove(image);
                }

                // let's save the changes first for the deleted images
                _context.SaveChanges();


                foreach (var media in imageUrls)
                {
                    // download the image from net and save it to the file system
                    var fileName = _imageHelper.SaveProductImage(eisSku, media.Url);
                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    // add the database to the database
                    addProductImage(fileName, eisSku, 99, "Amazon Large Image", media.Type);
                }

                // save the images
                _context.SaveChanges();

            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }


        public void AddProductImage(MediaContent media)
        {
            addProductImage(media.Url, media.ParentId, 99, media.Caption, media.Type);
        }

        public void UpdateProductImage(long id, string fileName, string caption)
        {
            var image = _context.productimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return;

            image.FileName = fileName;
            image.Caption = caption;

            _context.SaveChanges();
        }

        public bool DeleteProductImage(long id)
        {
            var image = _context.productimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return true;

            // delete the image from database first
            var eisSKU = image.EisSKU;
            _context.productimages.Remove(image);
            _context.SaveChanges();

            // then the file
            _imageHelper.RemoveProductImage(eisSKU, image.FileName);

            return true;
        }

        public Models.Item GetProductItemByEisSku(string skuPart)
        {
            var product = _context.products.FirstOrDefault(x => x.EisSKU == skuPart);
            if (product == null)
                return null;

            return new Models.Item
            {
                SKU = product.EisSKU,
                Name = product.Name,
                Description = product.Description,
                QtyAvailable = product.Quantity,
                SupplierPrice = product.SupplierPrice,
                SellingPrice = product.SellerPrice
            };
        }

        #endregion

        #region Marketplace Methods
        public IEnumerable<MarketplaceInventoryFeed> GetProductInventoryFeed(MarketplaceFeed param)
        {
            var products = getProductFeed(param);            
            var productList = products.Where(o => !o.IsBlacklisted).ToList();

            return Mapper.Map<List<MarketplaceInventoryFeed>>(productList);
        }

        public MarketplaceInventoryFeed GetProductInventoryBySku(string eisSku)
        {
            var product = _context.products.FirstOrDefault(x => x.EisSKU == eisSku);

            return Mapper.Map<product, MarketplaceInventoryFeed>(product);
        }

        public IEnumerable<MarketplacePriceFeedDto> GetProductPriceFeed(MarketplaceFeed param)
        {
            // get the product list
            var products = getProductFeed(param);
            var productList = products.Where(o => !o.IsBlacklisted).ToList();

            return Mapper.Map<List<MarketplacePriceFeedDto>>(productList);
        }

        public IEnumerable<ItemFeed> GeteBayItemFeeds(MarketplaceFeed feed)
        { 
            // get the product list
            var products = getProductFeed(feed);
            var productList = products.Where(o => !o.IsBlacklisted).ToList();

            return Mapper.Map<List<ItemFeed>>(productList);
        }
        
        public MarketplacePriceFeedDto GetProductPriceFeedBySku(string eisSku)
        {
            var product = _context.products.FirstOrDefault(x => x.EisSKU == eisSku);

            return Mapper.Map<product, MarketplacePriceFeedDto>(product);
        }

        public List<AmazonInfoFeed> GetProductInfoFeeds(MarketplaceFeed feed)
        {
            var products = getProductFeed(feed);
            if (!products.Any())
                return null;

            // just return products only with ASIN
            return products.Select(x => x.productamazon)
                .Select(x => new AmazonInfoFeed
                {
                    EisSKU = x.EisSKU,
                    ASIN = x.ASIN,
                    UPC = x.product.UPC
                })
            .ToList();
        }
        public IEnumerable<eBayCategoryFeed> GeteBaySuggestedCategoryFeed(MarketplaceFeed feed)
        {
            var products = getProductFeed(feed);
            var productList = products.ToList();

            return Mapper.Map<List<eBayCategoryFeed>>(productList);
        }
        #endregion

        #region Amazon eisProduct
        public ProductAmazonDto GetProductAmazon(string eisSku)
        {
            var product = _context.productamazons.Find(eisSku);

            return Mapper.Map<productamazon, ProductAmazonDto>(product);
        }

        public ProductAmazonDto SaveProductAmazon(ProductAmazonDto model)
        {
            var product = Mapper.Map<ProductAmazonDto, productamazon>(model);
            try
            {
                product.Modified = DateTime.UtcNow;
                product.ModifiedBy = model.ModifiedBy;

                _context.productamazons.Add(product);
                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }

            return model;
        }

        public ProductAmazonDto UpdateProductAmazon(string eisSku, ProductAmazonDto model)
        {
            var oldProduct = _context.productamazons.Find(eisSku);
            
            try
            { 
                // ensure the EIS SKU is upper cased
                model.EisSKU.ToUpper();
                _context.Entry(oldProduct).CurrentValues.SetValues(model);
                oldProduct.Modified = DateTime.UtcNow;
                oldProduct.ModifiedBy = model.ModifiedBy;

                _context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }

            return model;
        }

        public MarketplaceProduct GetMarketplaceProductInfo(string marketplace, string eisSku)
        {
            if (marketplace == "Amazon")
            {
                var product = _context.productamazons.FirstOrDefault(x => x.EisSKU == eisSku);
                if (product == null)
                    return null;

                _context.Entry<productamazon>(product).Reload();
                return Mapper.Map<productamazon, ProductAmazon>(product);
            }

            return null;
        }
        #endregion

        #region eBay product
        public ProducteBayDto GetProducteBay(string eisSku)
        {
            var ebay = _context.productebays.FirstOrDefault(x => x.EisSKU == eisSku);

            return Mapper.Map<productebay, ProducteBayDto>(ebay);
        }

        public bool SaveProducteBay(ProducteBayDto model)
        {
            var product = Mapper.Map<ProducteBayDto, productebay>(model);
            product.Created = DateTime.UtcNow;

            _context.productebays.Add(product);
            _context.SaveChanges();

            return true;
        }

        public bool UpdateProducteBay(string eisSku, ProducteBayDto model)
        {
            var oldProduct = _context.productebays.Find(eisSku);

            // ensure the EIS SKU is upper cased
            model.EisSKU.ToUpper();
            _context.Entry(oldProduct).CurrentValues.SetValues(model);
            oldProduct.Modified = DateTime.UtcNow;

            _context.SaveChanges();

            return true;
        }
        #endregion

        #region BigCommerce eisProduct

        public ProductBigCommerceDto GetProductBigCommerce ( string eisSku ) {
            var bigcommerce = _context.productbigcommerces.FirstOrDefault(x => x.EisSKU == eisSku);

            return Mapper.Map<productbigcommerce, ProductBigCommerceDto>(bigcommerce);
        }

        public bool SaveProductBigCommerce ( ProductBigCommerceDto model ) {
            var product = Mapper.Map<productbigcommerce>(model);
            try {
                product.Created = DateTime.UtcNow;
                product.CreatedBy = model.ModifiedBy;

                _context.productbigcommerces.Add(product);
                _context.SaveChanges();

                return true;
            } catch (DbEntityValidationException ex) {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                throw ex;
            } catch (Exception ex) {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public bool UpdateProductBigCommerce ( string eisSku, ProductBigCommerceDto model ) {
            try {
                var oldProduct = _context.productbigcommerces.Find(eisSku);

                // ensure the EIS SKU is upper cased
                model.EisSKU.ToUpper();
                _context.Entry(oldProduct).CurrentValues.SetValues(model);
                oldProduct.Modified = DateTime.UtcNow;
                oldProduct.ModifiedBy = model.ModifiedBy;

                _context.SaveChanges();

                return true;
            } catch (DbEntityValidationException ex) {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.ProductService, errorMsg, ex.StackTrace);
                throw ex;
            } catch (Exception ex) {
                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public bool CreateOrUpdateBigCommerceCustomFields(List<BigCommerceCustomFieldDto> customFields)
        {
            var success = false;    

            try
            {
                var forUpdateData = customFields.Where(o => o.Id != -1 && o.ProductId != -1).ToList();
                var forCreateData = customFields.Where(o => o.Id == -1 && o.ProductId != -1).ToList();
                var forDeleteData = customFields.Where(o => o.ProductId == -1).ToList();

                // Update Custom Fields
                foreach (var data in forUpdateData)
                {
                    var eisCustomField = _context.bigcommercecustomfields.FirstOrDefault(o => o.Id == data.Id);

                    if (eisCustomField != null)
                    {
                        eisCustomField.Name = data.Name;
                        eisCustomField.Text = data.Text;
                    }
                }


                // Create Custom Fields
                foreach (var data in forCreateData)
                {
                    var eisCustomField = Mapper.Map<BigCommerceCustomFieldDto, bigcommercecustomfield>(data);

                    _context.bigcommercecustomfields.Add(eisCustomField);
                }


                //  Delete Custom Fields
                foreach (var data in forDeleteData)
                {
                    var eisCustomField = _context.bigcommercecustomfields.FirstOrDefault(o => o.Id == data.Id);

                    if(eisCustomField != null)
                    {
                        _context.bigcommercecustomfields.Remove(eisCustomField);
                    }
                }

                success = _context.SaveChanges() > 0;

            } catch(Exception ex)
            {
                success = false;

                _logger.LogError(LogEntryType.ProductService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }

            return success;
        }

        #endregion

        #region helper methods

        private IQueryable<product> getProductFeed(MarketplaceFeed param)
        {
            IQueryable<product> products = null;
            if (param.IsAllProductItems)
            {
                if (!string.IsNullOrEmpty(param.SearchString))
                    products = _context.products
                        .Where(x => x.EisSKU.Contains(param.SearchString)
                            || x.UPC.Contains(param.SearchString)
                            || x.Category.Contains(param.SearchString)
                            || x.Name.Contains(param.SearchString));
                else if (param.ProductGroupId != -1)
                    products = getGroupedProducts(param.ProductGroupId,
                        param.SearchString,
                        param.CompanyId ?? -1,
                        //param.VendorId ?? -1,
                        //param.QuantityFrom ?? -1,
                        //param.QuantityTo ?? -1,
                        param.ExcludedEisSKUs,
                        param.WithImages,
                        param.IsKit, 
                        param.SkuType,
                        param.IsAmazonEnabled,
                        param.HasASIN);
                else
                    products = getFilteredProducts(param.SearchString, 
                        param.CompanyId ?? -1,
                        //param.VendorId ?? -1,
                        //param.QuantityFrom ?? -1,
                        //param.QuantityTo ?? -1,
                        param.ExcludedEisSKUs,
                        param.WithImages,
                        param.IsKit,
                        param.SkuType, 
                        param.IsSKULinked,
                        param.IsAmazonEnabled,
                        param.HasASIN);
            }
            else
            {
                products = _context.products.Where(x => param.SelectedEisSKUs.Contains(x.EisSKU)
                    && (!param.ExcludedEisSKUs.Any() || param.ExcludedEisSKUs.Contains(x.EisSKU)));
            }

            return products.AsQueryable();
        }

        private void addProductImage(string fileName, string eisSku, int order, string caption, string imageType, bool isPush = false)
        {
            var productImage = new productimage
            {
                EisSKU = eisSku,
                Caption = caption,
                FileName = fileName,
                ImageType = imageType,
                Order_ = order
            };

            _context.productimages.Add(productImage);

            if(!isPush)
            _context.SaveChanges();
        }

        private IQueryable<product> getFilteredProducts(string searchString,
            int companyId,
            //int vendorId,
            //int inventoryQtyFrom,
            //int inventoryQtyTo,
            List<string> excludedEisSKUs,
            int withImages,
            bool? isKit,
            SkuType? skuType,
            bool? isSKULinked,
            bool? isAmazonEnabled,
            bool? hasASIN)
        {
            var products = _context.products.Where(x => (string.IsNullOrEmpty(searchString)
                                       || (x.EisSKU.Contains(searchString)
                                       || x.UPC.Contains(searchString)
                                       || x.Category.Contains(searchString)
                                       || x.Name.Contains(searchString)))
                                   && (companyId == -1 || x.CompanyId == companyId)
                                   //&& (vendorId == -1 || x.VendorId == vendorId)
                                   //&& ((inventoryQtyFrom == -1 || inventoryQtyTo == -1) || (x.Quantity >= inventoryQtyFrom && x.Quantity <= inventoryQtyTo))
                                   && (!excludedEisSKUs.Any() || excludedEisSKUs.Contains(x.EisSKU))
                                   && (isKit == null || x.IsKit == isKit)
                                   && (skuType == null || x.SkuType == skuType)
                                   && (isSKULinked == null || x.vendorproductlinks.Any() == isSKULinked)
                                   && (isAmazonEnabled == null || (x.productamazon != null && x.productamazon.IsEnabled == isAmazonEnabled))
                                   && (hasASIN == null || (hasASIN.Value && x.productamazon.ASIN != null) || (!hasASIN.Value && x.productamazon.ASIN == null)))
                            .OrderBy(x => x.EisSKU);

            if (withImages == -1)
                return products.AsQueryable();
            else
            {
                var isWithImages = withImages == 1;
                return products
                    .Where(x => x.productimages.Any() == isWithImages)
                    .AsQueryable();
            }
        }

        private string createProductListQuery(string searchString,
           int productGroupId,
           int companyId,
           int vendorId,
           int inventoryQtyFrom,
           int inventoryQtyTo,
           int withImages,
           bool? isKit,
           SkuType? skuType,
           bool? isSKULinked,
           bool? isAmazonEnabled,
           bool? hasASIN)
        {
            var sb = new StringBuilder(@"
                SELECT 
                    {0}
                FROM products p 
                LEFT JOIN shadows s ON s.ShadowSKU = p.EisSKU AND s.IsConnected = 1
                LEFT JOIN productamazons pa ON pa.EisSKU = p.EisSKU
                LEFT JOIN productebays pe ON pe.EisSKU = p.EisSKU
                WHERE 1 = 1 ");

            if (!string.IsNullOrEmpty(searchString))
                sb.AppendFormat(@" AND (p.EisSKU LIKE '%{0}%' OR p.UPC LIKE '%{0}%' OR p.Name LIKE '%{0}%' OR p.Category LIKE '%{0}%')", searchString.Replace("_", "\\_"));

            if (productGroupId != -1)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT EisSKU FROM productgroups WHERE Id = {0})", productGroupId);

            if (vendorId != -1)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT DISTINCT EisSKU FROM vw_availablevendorproducts WHERE VendorId = {0})", vendorId);

            if (inventoryQtyFrom != -1 || inventoryQtyTo != -1)
                sb.AppendFormat(" AND p.EisSKU IN (SELECT DISTINCT EisSKU FROM vw_availablevendorproducts WHERE Quantity >= {0} AND Quantity <= {1})", inventoryQtyFrom, inventoryQtyTo);
            
            if (withImages != -1)
                sb.AppendFormat(" AND p.EisSKU {0} IN (SELECT DISTINCT EisSKU FROM productimages)", withImages == 1 ? "" : "NOT");
            
            if (isSKULinked.HasValue)
                sb.AppendFormat(" AND p.EisSKU {0} IN (SELECT DISTINCT EisSKU FROM vendorproductlinks)", isSKULinked.Value ? "" : "NOT");

            if (companyId != -1)
                sb.AppendFormat(" AND p.CompanyId = {0}", companyId);

            if (skuType.HasValue)
                sb.AppendFormat(" AND p.SkuType = {0}", (int)skuType);

            if (isKit.HasValue)
                sb.AppendFormat(" AND p.IsKit = {0}", isKit.Value ? 1 : 0);

            if (isAmazonEnabled != null)
                sb.AppendFormat(" AND pa.IsEnabled = {0}", isAmazonEnabled.Value ? 1 : 0);

            if (hasASIN != null)
                sb.AppendFormat(" AND pa.ASIN IS {0} NULL", hasASIN.Value ? "NOT" : "");

            return sb.ToString();
        }

        private IQueryable<product> getGroupedProducts(long productGroupId,
            string searchString,
            int companyId,
            //int vendorId,
            //int inventoryQtyFrom,
            //int inventoryQtyTo,
            List<string> excludedEisSKUs,
            int withImages,
            bool? isKit,
            SkuType? skuType,
            bool? isAmazonEnabled,
            bool? hasASIN)
        {
            var productGroup = _context.productgroupdetails.FirstOrDefault(x => x.Id == productGroupId);
            if (productGroup == null)
                return null;

            var products = productGroup.products
                    .Where(x => (string.IsNullOrEmpty(searchString)
                            || x.EisSKU.Contains(searchString)
                            || x.EisSupplierSKU.Contains(searchString)
                            || (!string.IsNullOrEmpty(x.UPC) && x.UPC.Contains(searchString))
                            || (!string.IsNullOrEmpty(x.Category) && x.Category.Contains(searchString))
                            || x.Name.Contains(searchString))
                        && (companyId == -1 || x.CompanyId == companyId)
                        //&& (vendorId == -1 || x.VendorId == vendorId)
                        //&& ((inventoryQtyFrom == -1 || inventoryQtyTo == -1) || (x.Quantity >= inventoryQtyFrom && x.Quantity <= inventoryQtyTo))
                        && (!excludedEisSKUs.Any() || excludedEisSKUs.Contains(x.EisSKU))
                        && (isKit == null || x.IsKit == isKit)
                        && (skuType == null || x.SkuType == skuType)
                        && (isAmazonEnabled == null || (x.productamazon != null && x.productamazon.IsEnabled == isAmazonEnabled))
                        && (hasASIN == null || x.productamazon != null || (hasASIN.Value && x.productamazon.ASIN != null) || (!hasASIN.Value && x.productamazon.ASIN == null)))
                    .OrderBy(x => x.EisSKU);

            if (withImages == -1)
            {
                return products.AsQueryable();  
            }
            else
            {

                // get the EisSKU with images
                var eisSkusWithImage = _context.productimages.GroupBy(x => x.EisSKU).Select(x => x.Key).ToArray();

                if (withImages == 1)
                {
                   var productsWithImage =  products.Where(x => eisSkusWithImage.Contains(x.EisSKU));
                   return productsWithImage.AsQueryable();
                }
                else
                {
                   var productsWithOutImage = products.Where(x => !eisSkusWithImage.Contains(x.EisSKU));
                   return productsWithOutImage.AsQueryable();
                }
            }      
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                    // Dispose other managed resources.
                }
                //release unmanaged resources.
            }
            _disposed = true;
        }
        #endregion
    }
}
