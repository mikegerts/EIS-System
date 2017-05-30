using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using MySql.Data.MySqlClient;
using AutoMapper;

namespace EIS.SystemJobApp.Repositories
{
    public class ProductRepository
    {
        private readonly MySqlConnection _connection;
        private readonly IImageHelper _imageHelper;
        protected readonly LoggerRepository _logger;
        
        public ProductRepository() { }

        public ProductRepository(LoggerRepository logger)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            _connection = new MySqlConnection(connectionString);
            _logger = logger;
            _imageHelper = new ImageHelper(new PersistenceHelper());
        }

        public int DoUpadateOrInsertProduct(Product model, bool isUpdate, string submittedBy)
        {
            using(var context = new EisInventoryContext())
            {
                if (isUpdate)
                {
                    // get the exising product from db
                    var product = context.products.FirstOrDefault(x => x.EisSKU == model.EisSKU);
                    if (product == null)
                        return 0;

                    // let's update its data except for Products.Name
                    if (model.Description != null) product.Description = model.Description;
                    if (model.ShortDescription != null) product.ShortDescription = model.ShortDescription;
                    if (model.Category != null) product.Category = model.Category;
                    if (model.isSellerPriceSet) product.SellerPrice = model.SellerPrice;
                    if (model.PkgLength != null) product.PkgLength = model.PkgLength;
                    if (model.PkgWidth != null) product.PkgWidth = model.PkgWidth;
                    if (model.PkgHeight != null) product.PkgHeight = model.PkgHeight;
                    if (model.PkgLenghtUnit != null) product.PkgLenghtUnit = model.PkgLenghtUnit;
                    if (model.PkgWeight != null) product.PkgWeight = model.PkgWeight;
                    if (model.PkgWeightUnit != null) product.PkgWeightUnit = model.PkgWeightUnit;
                    if (model.ItemLength != null) product.ItemLength = model.ItemLength;
                    if (model.ItemWidth != null) product.ItemWidth = model.ItemWidth;
                    if (model.ItemHeight != null) product.ItemHeight = model.ItemHeight;
                    if (model.ItemLenghtUnit != null) product.ItemLenghtUnit = model.ItemLenghtUnit;
                    if (model.ItemWeight != null) product.ItemWeight = model.ItemWeight;
                    if (model.ItemWeightUnit != null) product.ItemWeightUnit = model.ItemWeightUnit;
                    if (model.GuessedWeight != null) product.GuessedWeight = model.GuessedWeight;
                    if (model.AccurateWeight != null) product.AccurateWeight = model.AccurateWeight;
                    if (model.GuessedWeightUnit != null) product.GuessedWeightUnit = model.GuessedWeightUnit;
                    if (model.AccurateWeightUnit != null) product.AccurateWeightUnit = model.AccurateWeightUnit;
                    if (model.GuessedShipping != null) product.GuessedShipping = model.GuessedShipping;
                    if (model.AccurateShipping != null) product.AccurateShipping = model.AccurateShipping;
                    if (model.isCompanySet) product.CompanyId = model.CompanyId;
                    if (model.isKitSet) product.IsKit = model.IsKit;
                    if (model.isBlacklistedSet) product.IsBlacklisted = model.IsBlacklisted;
                    if (model.isSkuTypeSet) product.SkuType = model.SkuType;

                    // Only upate UPC if this is a Normal product
                    if (model.UPC != null && product.SkuType == SkuType.Normal)
                    {
                        product.UPC = model.UPC;

                        // let's check if the product has shadow products
                        var shadows = context.shadows.Where(x => x.ParentSKU == model.EisSKU).ToList();
                        foreach(var shadow in shadows)
                        {
                            // get the shadow product details
                            var shadowProduct = context.products.FirstOrDefault(x => x.EisSKU == shadow.ShadowSKU);
                            shadowProduct.UPC = model.UPC;
                            shadowProduct.ModifiedBy = submittedBy;
                            shadowProduct.Modified = DateTime.UtcNow;
                        }
                    }

                    product.ModifiedBy = submittedBy;
                    product.Modified = DateTime.UtcNow;
                }
                else
                {
                    // add first the product item
                    context.products.Add(new product {
                        EisSKU = model.EisSKU,
                        CompanyId = model.CompanyId,
                        Name = model.Name,
                        Description = model.Description,
                        ShortDescription = model.ShortDescription,
                        Category = model.Category,
                        ProductTypeId = model.ProductTypeId,
                        UPC = model.UPC,
                        SellerPrice = model.SellerPrice,
                        PkgLength = model.PkgLength,
                        PkgWidth = model.PkgWidth,
                        PkgHeight = model.PkgHeight,
                        PkgLenghtUnit = model.PkgLenghtUnit,
                        PkgWeight = model.PkgWeight,
                        PkgWeightUnit = model.PkgWeightUnit,
                        ItemLength = model.ItemLength,
                        ItemWidth = model.ItemWidth,
                        ItemHeight = model.ItemHeight,
                        ItemLenghtUnit = model.ItemLenghtUnit,
                        ItemWeight = model.ItemWeight,
                        ItemWeightUnit = model.ItemWeightUnit,
                        EAN = model.EAN,
                        Brand = model.Brand,
                        Color = model.Color,
                        Model = model.Model_,
                        GuessedWeight = model.GuessedWeight,
                        GuessedWeightUnit = model.GuessedWeightUnit,
                        AccurateWeight = model.AccurateWeight,
                        AccurateWeightUnit = model.AccurateWeightUnit,
                        GuessedShipping = model.GuessedShipping,
                        AccurateShipping = model.AccurateShipping,
                        IsKit = model.IsKit,
                        IsBlacklisted = model.IsBlacklisted,
                        SkuType = model.SkuType,
                        CreatedBy = submittedBy,
                        Created = DateTime.UtcNow
                    });
                }
                
                // save the change first to the product
                context.SaveChanges();

                // download and update product images
                UpdateProductImages(model.Images, model.EisSKU);

                return 1;
            }
        }

        public int DoUpdateOrInsertAmazon(ProductAmazon model, string submittedBy)
        {
            using (var context = new EisInventoryContext())
            {
                // check if the bigcommerce product already exists
                var product = context.productamazons.FirstOrDefault(x => x.EisSKU == model.EisSKU);

                // create new item if it doesn't exists
                if (product == null)
                {
                    var domain = Mapper.Map<productamazon>(model);
                    domain.CreatedBy = submittedBy;
                    domain.Created = DateTime.UtcNow;
                    context.productamazons.Add(domain);
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.ASIN)) product.ASIN = model.ASIN;
                    if (model.Price != null) product.Price = model.Price;
                    if (model.LeadtimeShip != null) product.LeadtimeShip = model.LeadtimeShip;
                    if (model.PackageQty != null) product.PackageQty = model.PackageQty;
                    if (model.NumOfItems != null) product.NumOfItems = model.NumOfItems;
                    if (model.MaxOrderQty != null) product.MaxOrderQty = model.MaxOrderQty;
                    if (model.ProductTitle != null) product.ProductTitle = model.ProductTitle;
                    if (model.MapPrice != 0) product.MapPrice = model.MapPrice;
                    if (model.isAllowGiftWrapSet) product.IsAllowGiftWrap = model.IsAllowGiftWrap;
                    if (model.isAllowGiftMsgSet) product.IsAllowGiftMsg = model.IsAllowGiftMsg;
                    if (model.Condition != null) product.Condition = model.Condition;
                    if (model.ConditionNote != null) product.ConditionNote = model.ConditionNote;
                    if (model.FulfilledBy != null) product.FulfilledBy = model.FulfilledBy;
                    if (model.FbaSKU != null) product.FbaSKU = model.FbaSKU;
                    if (model.isEnabledSet) product.IsEnabled = model.IsEnabled;
                    if (!string.IsNullOrEmpty(model.ProductGroup)) product.ProductGroup = model.ProductGroup;
                    if (model.ProductTypeName != null) product.ProductTypeName = model.ProductTypeName;
                    if (model.TaxCode != null) product.TaxCode = model.TaxCode;
                    //if (model.WeightBox1 != null) product.WeightBox1 = model.WeightBox1;
                    //if (!string.IsNullOrEmpty(model.WeightBox1Unit)) product.WeightBox1Unit = model.WeightBox1Unit;
                    //if (model.WeightBox2 != null) product.WeightBox2 = model.WeightBox2;
                    //if (!string.IsNullOrEmpty(model.WeightBox2Unit)) product.WeightBox2Unit = model.WeightBox2Unit;

                    product.ModifiedBy = submittedBy;
                    product.Modified = DateTime.UtcNow;
                }

                // save the changes
                context.SaveChanges();
            }

            return 1;
        }

        public int DoUpadateOrInserteBay(ProducteBayDto model, string submittedBy)
        {
            using (var context = new EisInventoryContext())
            {
                // check if the bigcommerce product already exists
                var product = context.productebays.FirstOrDefault(x => x.EisSKU == model.EisSKU);

                // create new item if it doesn't exists
                if (product == null)
                {
                    model.Title = getTruncatedString(model.Title, 80);
                    model.SubTitle = getTruncatedString(model.SubTitle, 55);
                    var domain = Mapper.Map<productebay>(model);
                    domain.CreatedBy = submittedBy;
                    domain.Created = DateTime.UtcNow;
                    context.productebays.Add(domain);
                }
                else
                {
                    if (model.ItemId != null) product.ItemId = model.ItemId;
                    if (model.Title != null) product.Title = getTruncatedString(model.Title, 80);
                    if (model.SubTitle != null) product.SubTitle = getTruncatedString( model.SubTitle, 55);
                    if (model.Description != null) product.Description = model.Description;
                    if (model.isListingQuantitySet) product.ListingQuantity = model.ListingQuantity;
                    if (model.isSafetyQtySet) product.SafetyQty = model.SafetyQty;
                    if (model.isCategoryIdSet) product.CategoryId = model.CategoryId;
                    if (model.isStartPriceSet) product.StartPrice = model.StartPrice;
                    if (model.isReservePriceSet) product.ReservePrice = model.ReservePrice;
                    if (model.isBinPriceSet) product.BinPrice = model.BinPrice;
                    if (model.ListType != null) product.ListType = model.ListType;
                    if (model.Duration != null) product.Duration = model.Duration;
                    if (model.Location != null) product.Location = model.Location;
                    if (model.isConditionSet) product.Condition_ = model.Condition_;
                    if (model.isDispatchTimeMaxSet) product.DispatchTimeMax = model.DispatchTimeMax;
                    if (model.isOutOfStockListingSet) product.IsOutOfStockListing = model.IsOutOfStockListing;
                    if (model.isBoldTitleSet) product.IsBoldTitle = model.IsBoldTitle;
                    if (model.isRequireAutoPaymentSet) product.IsRequireAutoPayment = model.IsRequireAutoPayment;
                    if (model.isEnabledSet) product.IsEnabled = model.IsEnabled;

                    product.ModifiedBy = submittedBy;
                    product.Modified = DateTime.UtcNow;
                }

                // save the changes
                context.SaveChanges();
            }

            return 1;
        }
        
        public int DoUpdateOrInsertBigCommerce(ProductBigCommerceDto model, string submittedBy)
        {
            using (var context = new EisInventoryContext())
            {
                // check if the bigcommerce product already exists
                var product = context.productbigcommerces.FirstOrDefault(x => x.EisSKU == model.EisSKU);
                if(product == null)
                {
                    var domain = Mapper.Map<productbigcommerce>(model);
                    domain.CreatedBy = submittedBy;
                    domain.Created = DateTime.UtcNow;
                    context.productbigcommerces.Add(domain);
                }
                else
                {
                    if (model.ProductId != null) product.ProductId = model.ProductId;
                    if (model.CategoryId != null) product.CategoryId = model.CategoryId;
                    if (model.Price != null) product.Price = model.Price;
                    if (!string.IsNullOrEmpty(model.Condition)) product.Condition = model.Condition;
                    if (!string.IsNullOrEmpty(model.Categories)) product.Categories = model.Categories;
                    if (model.RetailPrice != null) product.RetailPrice = model.RetailPrice;
                    if (!string.IsNullOrEmpty(model.PrimaryImage)) product.PrimaryImage = model.PrimaryImage;
                    if (model.FixedCostShippingPrice != null) product.FixedCostShippingPrice = model.FixedCostShippingPrice;
                    if (model.Brand != null) product.Brand = model.Brand;
                    if (model.ProductsType != null) product.ProductsType = model.ProductsType;
                    if (model.InventoryLevel != null) product.InventoryLevel = model.InventoryLevel;
                    if (model.InventoryWarningLevel != null) product.InventoryWarningLevel = model.InventoryWarningLevel;
                    if (model.InventoryTracking != null) product.InventoryTracking = model.InventoryTracking;
                    if (model.OrderQuantityMinimum != null) product.OrderQuantityMinimum = model.OrderQuantityMinimum;
                    if (model.OrderQuantityMaximum != null) product.OrderQuantityMaximum = model.OrderQuantityMaximum;
                    if (model.IsEnabled) product.IsEnabled = model.IsEnabled;
                    if (model.Description != null) product.Description = model.Description;
                    if (model.Title != null) product.Title = model.Title;

                    product.ModifiedBy = submittedBy;
                    product.Modified = DateTime.UtcNow;
                }

                // save the changes
                context.SaveChanges();
            }

            return 1;
        }

        public void UpdateEisProduct(ProductAmazon model, string submittedBy)
        {
            try
            {
                using (var context = new EisInventoryContext())
                {
                    // get the existing product to update from db
                    var product = context.products.FirstOrDefault(x => x.EisSKU == model.EisSKU);
                    if (product == null)
                        return;

                    // determine the product type id of the marketplace product
                    var productTypeId = getConfiguredProductTypeId(model.ProductTypeName, model.ProductGroup);

                    product.Brand = model.Brand;
                    product.Color = model.Color;
                    product.EAN = model.EAN;
                    product.Model = model.Model;
                    product.ProductTypeId = productTypeId;

                    // set the product' package dimension
                    if (model.PackageDimension != null)
                    {
                        product.PkgLength = model.PackageDimension.Length.Value;
                        product.PkgWidth = model.PackageDimension.Width.Value;
                        product.PkgHeight = model.PackageDimension.Height.Value;
                        product.PkgLenghtUnit = model.PackageDimension.Length.Unit;
                        product.PkgWeight = model.PackageDimension.Weight.Value;
                        product.PkgWeightUnit = model.PackageDimension.Weight.Unit;
                    }

                    // set the EIS product Item's dimension
                    if (model.ItemDimension != null)
                    {
                        product.ItemLength = model.ItemDimension.Length.Value;
                        product.ItemWidth = model.ItemDimension.Width.Value;
                        product.ItemHeight = model.ItemDimension.Height.Value;
                        product.ItemLenghtUnit = model.ItemDimension.Length.Unit;
                        product.ItemWeight = model.ItemDimension.Weight.Value;
                        product.ItemWeightUnit = model.ItemDimension.Weight.Unit;
                    }

                    product.ModifiedBy = submittedBy;
                    product.Modified = DateTime.UtcNow;

                    // save the changes
                    context.SaveChanges();
                }
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.AmazonGetInfoWorker, errorMsg, ex.StackTrace);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonGetInfoWorker,
                    string.Format("Error in updating Amazon product details -> EisSKU: {0} <br/>Error message: {1}",
                    model.EisSKU, EisHelper.GetExceptionMessage(ex)));
            }
        }

        public string GetNextEisSKUForCompany(int companyId)
        {
            string eisSKU = null;
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

        public int? GetDefaultCompany()
        {
            using (var context = new EisInventoryContext())
            {
                var company = context.companies.FirstOrDefault(x => x.IsDefault);
                return company == null ? null : (int?)company.Id;
            }
        }

        public void UpdateProductImages(List<string> imageUrls, string eisSku)
        {
            // check if there's any image URLs
            if (imageUrls == null || !imageUrls.Any())
                return;

            try
            {
                using (var context = new EisInventoryContext())
                {
                    // get the Amazon images
                    var amazonImages = context.productimages
                        .Where(x => x.EisSKU == eisSku && x.ImageType != "CUSTOM")
                        .ToList();
                    foreach (var image in amazonImages)
                    {
                        // delete first the image file from the directory
                        _imageHelper.RemoveProductImage(eisSku, image.FileName);

                        // then the image record from the database
                        context.productimages.Remove(image);
                    }

                    // let's save the changes first for the deleted images
                    context.SaveChanges();

                    // download and add the images
                    foreach (var url in imageUrls)
                    {
                        // download the image from net and save it to the file system
                        var fileName = _imageHelper.SaveProductImage(eisSku, url);
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        // add the database to the database
                        context.productimages.Add(new productimage
                        {
                            EisSKU = eisSku,
                            Caption = "Amazon",
                            FileName = fileName,
                            ImageType = "LARGE",
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

        public int UpdateProductsBlacklistedStatus(List<string> eisSKUs, bool isBlacklisted, string submittedBy)
        {
            if (eisSKUs == null || !eisSKUs.Any())
                return 0;

            using(var context = new EisInventoryContext())
            {
                // get the product items first
                var products = context.products.Where(x => eisSKUs.Contains(x.EisSKU))
                    .ToList();

                // then update its blacklisted status
                products.ForEach(item =>
                {
                    item.IsBlacklisted = isBlacklisted;
                    item.ModifiedBy = submittedBy;
                    item.Modified = DateTime.UtcNow;
                });
                context.SaveChanges();
            }
            return eisSKUs.Count;
        }

        public int DeleteProduct(string eisSKU)
        {
            var retValue = 0;
            using (var context = new EisInventoryContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var isSuccessful = false;
                    var imagesFileName = new List<string>();
                    try
                    {
                        // get the product first
                        var product = context.products.FirstOrDefault(x => x.EisSKU == eisSKU);
                        if (product == null)
                            return 1;

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
                        retValue = 1;
                    }
                    catch(Exception ex)
                    {
                        isSuccessful = false;
                        retValue = 0;
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

            return retValue;
        }
        
        public void UpdateeBayEndedItem(string eisSKU)
        {
            using (var context = new EisInventoryContext())
            {
                // get the product eBay to update
                var product = context.productebays.FirstOrDefault(x => x.EisSKU == eisSKU);
                if (product == null)
                    return;

                // set the null for the relisted date and set the date for the endeditem date
                product.ReListedItemDate = null;
                product.EndedItemDate = DateTime.Now;

                // let's save the changes
                context.SaveChanges();
            }
        }

        public void UpdateReListedeBayProduct(string eisSKU, string newItemId)
        {
            using (var context = new EisInventoryContext())
            {
                // get the product eBay to update
                var product = context.productebays.FirstOrDefault(x => x.EisSKU == eisSKU);
                if (product == null)
                    return;

                // set the null for the relisted date and set the date for the endeditem date
                product.ItemId = newItemId;
                product.ReListedItemDate = DateTime.Now;
                product.EndedItemDate = null;

                // let's save the changes
                context.SaveChanges();
            }
        }

        public void CloseDbConnection()
        {
            if(_connection != null)
            {
                _connection.Close();
            }
        }

        private int? getConfiguredProductTypeId(string productTypeName, string productGroup)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@SubName", productTypeName}
            };
            var reader = MySqlHelper.ExecuteReader(_connection, CommandType.Text,
                     @"SELECT s.ParentCode, s.Code, a.Name, s.Name FROM amazonsubcategories s INNER JOIN amazoncategories a ON a.Code = s.ParentCode WHERE s.Name=@SubName", parameters);

            // get the Amazon Sub Cagetory Code and its ParentCode
            subcategory subcategory = null;
            while (reader.Read())
            {
                subcategory = new subcategory();
                subcategory.ParentCode = reader[0].ToString();
                subcategory.SubCode = reader[1].ToString();
                subcategory.ParentName = reader[2].ToString();
                subcategory.SubName = reader[3].ToString();
            }

            // close the data reader
            reader.Close();

            // if it doesn't exist
            if (subcategory == null)
            {
                _logger.LogWarning(LogEntryType.AmazonGetInfoWorker, string.Format("ProductType: {0} - ProductGroup: {1} not found in Amazon Sub-Category", productTypeName, productGroup));
                return null;
            }

            // get the product type id if it's exist
            var productTypeId = getProductTypeIdByCodes(subcategory.ParentCode, subcategory.SubCode);
            if (productTypeId != null)
                return productTypeId;

            // if NULL, let's create new Product Type
            var insertProductTypeParam = new Dictionary<string, object>
            {
                {"@TypeName", string.Format("{0} - {1}", subcategory.ParentName, subcategory.SubName)},
                {"@AmazonMainCategoryCode", subcategory.ParentCode},
                {"@AmazonSubCategoryCode", subcategory.SubCode},
            };

            MySqlHelper.ExecuteNonQuery(_connection, @"INSERT INTO producttypes(TypeName,AmazonMainCategoryCode,AmazonSubCategoryCode)
                VALUES(@TypeName,@AmazonMainCategoryCode,@AmazonSubCategoryCode)", insertProductTypeParam);

            // then, let's query and return the product type id
            return getProductTypeIdByCodes(subcategory.ParentCode, subcategory.SubCode);
        }

        private int? getProductTypeIdByCodes(string parentCode, string subCode)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@AmazonMainCategoryCode", parentCode},
                {"@AmazonSubCategoryCode", subCode},
            };
            var reader = MySqlHelper.ExecuteReader(_connection, CommandType.Text,
                     @"SELECT Id FROM producttypes WHERE AmazonMainCategoryCode=@AmazonMainCategoryCode AND AmazonSubCategoryCode=@AmazonSubCategoryCode", parameters);

            int? producTypeId = null;
            while (reader.Read())
                producTypeId = reader[0] as int?;

            // close the data reader
            reader.Close();

            return producTypeId;
        }

        private string getTruncatedString(string input, int max)
        {
            if (!string.IsNullOrEmpty(input) && input.Length > max)
                return string.Format("{0}", input.Substring(0, max));

            return input;
        }
    }

    public class subcategory
    {
        public string SubCode { get; set; }
        public string ParentCode { get; set; }
        public string SubName { get; set; }
        public string ParentName { get; set; }
    }
}
