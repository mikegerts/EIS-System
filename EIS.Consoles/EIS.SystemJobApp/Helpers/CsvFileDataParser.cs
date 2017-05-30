using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.SystemJobApp.Helpers
{
    public static class CsvFileDataParser
    {
        public static Message ParseVendorProductFile(string filePath, List<VendorProduct> vendorProducts, bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        var vendorProduct = new VendorProduct();
                        vendorProduct.Images = new List<string>();

                        string eisSupplierSKU = null;
                        csvReader.TryGetField<string>("EisSupplierSKU", out eisSupplierSKU);
                        vendorProduct.EisSupplierSKU = eisSupplierSKU;

                        string supplierSku = null;
                        csvReader.TryGetField<string>("SupplierSKU", out supplierSku);
                        vendorProduct.SupplierSKU = supplierSku;

                        int vendorId;
                        csvReader.TryGetField<int>("VendorId", out vendorId);
                        vendorProduct.VendorId = vendorId;

                        // the vendor product file should contain the EisSupplierSKU
                        if (eisSupplierSKU == null && supplierSku == null && vendorId == 0)
                            throw new ArgumentException("File upload failed: One of the required items not found! \"EIS Supplier SKU\"!");

                        string productName = null;
                        csvReader.TryGetField<string>("Name", out productName);
                        vendorProduct.Name = productName;

                        string description = null;
                        csvReader.TryGetField<string>("Description", out description);
                        vendorProduct.Description = description;

                        decimal? supplierPrice = null;
                        csvReader.TryGetField<decimal?>("SupplierPrice", out supplierPrice);
                        vendorProduct.SupplierPrice = supplierPrice ?? 0;
                        vendorProduct.IsSupplierPriceSet = supplierPrice != null;

                        int? quantity = null;
                        csvReader.TryGetField<int?>("Quantity", out quantity);
                        vendorProduct.Quantity = quantity ?? 0;
                        vendorProduct.IsQuantitySet = quantity != null;

                        int? minPack = null;
                        csvReader.TryGetField<int?>("MinPack", out minPack);
                        vendorProduct.MinPack = minPack ?? 0;
                        vendorProduct.IsMinPackSet = minPack != null;

                        string upcCode = null;
                        csvReader.TryGetField<string>("UPC", out upcCode);
                        vendorProduct.UPC = upcCode;

                        string category = null;
                        csvReader.TryGetField<string>("Category", out category);
                        vendorProduct.Category = category;

                        string weight = null;
                        csvReader.TryGetField<string>("Weight", out weight);
                        vendorProduct.Weight = weight;

                        string weightUnit = null;
                        csvReader.TryGetField<string>("WeightUnit", out weightUnit);
                        vendorProduct.WeightUnit = weightUnit;

                        string shipping = null;
                        csvReader.TryGetField<string>("Shipping", out shipping);
                        vendorProduct.Shipping = shipping;

                        int? vendorMOQ = null;
                        csvReader.TryGetField<int?>("VendorMOQ", out vendorMOQ);
                        vendorProduct.VendorMOQ = vendorMOQ;

                        string vendorMOQType = null;
                        csvReader.TryGetField<string>("VendorMOQType", out vendorMOQType);
                        vendorProduct.VendorMOQType = vendorMOQType;

                        bool? isAutoLinkToEisSKU = null;
                        csvReader.TryGetField<bool?>("IsAutoLinkToEisSKU", out isAutoLinkToEisSKU);
                        vendorProduct.IsAutoLinkToEisSKU = isAutoLinkToEisSKU.HasValue ? isAutoLinkToEisSKU.Value : false;
                        vendorProduct.IsAutoLinkToEisSKUSet = isAutoLinkToEisSKU != null;

                        // continue if it has no required values
                        if (string.IsNullOrEmpty(vendorProduct.SupplierSKU)
                            && string.IsNullOrEmpty(vendorProduct.EisSupplierSKU)
                            && vendorProduct.VendorId == 0)
                            continue;

                        string imageUrl;
                        for (var counter = 1; counter <= 5; counter++)
                        {
                            var isSuccess = csvReader.TryGetField<string>(string.Format("ImageUrl{0}", counter), out imageUrl);
                            if (isSuccess && !string.IsNullOrEmpty(imageUrl))
                                vendorProduct.Images.Add(imageUrl);
                        }

                        vendorProducts.Add(vendorProduct);
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Vendor Product file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }

        public static Message ParseProductFile(string filePath, 
            List<Product> productModels,
            List<ProductAmazon> amazonModels,
            List<ProducteBayDto> eBayModels,
            List<ProductBigCommerceDto> bigCommerceModels,
            bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        productModels.Add(extractProductEis(csvReader));
                        amazonModels.Add(extractProductAmazon(csvReader));
                        eBayModels.Add(extractProducteBay(csvReader));
                        bigCommerceModels.Add(extractProductBigCommerce(csvReader));
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Product file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }

        public static Message ParseShippingRateFile(string filePath, List<ShippingRateDB> shippingRates, bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        var shippingRate = new ShippingRateDB();

                        int ID;
                        csvReader.TryGetField<int>("ID", out ID);
                        shippingRate.Id = ID;

                        int? WeightFrom = null;
                        csvReader.TryGetField<int?>("WeightFrom", out WeightFrom);
                        shippingRate.WeightFrom = WeightFrom;

                        int? WeightTo;
                        csvReader.TryGetField<int?>("WeightTo", out WeightTo);
                        shippingRate.WeightTo = WeightTo;
                        
                        string Unit = null;
                        csvReader.TryGetField<string>("Unit", out Unit);
                        shippingRate.Unit = Unit;

                        decimal? Rate = null;
                        csvReader.TryGetField<decimal?>("Rate", out Rate);
                        shippingRate.Rate = Rate;


                        shippingRates.Add(shippingRate);
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Shipping Rates file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }


        public static Message ParseVendorInventoryFile(string filePath, List<VendorProduct> vendorProducts, bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        var vendorProduct = new VendorProduct();

                        string eisSupplierSKU = null;
                        csvReader.TryGetField<string>("EisSupplierSKU", out eisSupplierSKU);
                        vendorProduct.EisSupplierSKU = eisSupplierSKU;

                        int vendorId;
                        csvReader.TryGetField<int>("VendorId", out vendorId);
                        vendorProduct.VendorId = vendorId;

                        int? quantity = null;
                        csvReader.TryGetField<int?>("Quantity", out quantity);
                        vendorProduct.Quantity = quantity ?? 0;
                        vendorProduct.IsQuantitySet = quantity != null;

                        vendorProducts.Add(vendorProduct);
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Shipping Rates file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }


        public static Message ParsedKitFile(string filePath, List<KitDetailDto> kitDetails, bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        kitDetails.Add(extractKitDetail(csvReader));
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Kit file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }

        public static Message ParsedShadowFile(string filePath, List<Shadow> shadows, bool hasHeader)
        {
            var message = new Message();
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        shadows.Add(extractShadow(csvReader));
                    }
                }
                message.IsSucess = true;
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Shadow file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }


        public static Message ParsedSKUFile ( string filePath, List<BlacklistedSkuDto> blacklistedskus, bool hasHeader ) {
            var message = new Message();
            try {
                using (var reader = new StreamReader(filePath))
                using (var csvReader = new CsvReader(reader)) {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read()) {
                        blacklistedskus.Add(extractBlacklistedSKU(csvReader));
                    }
                }
                message.IsSucess = true;
            } catch (Exception ex) {
                message.SetMessage(false, string.Format("Blacklisted SKU file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }

        private static BlacklistedSkuDto extractBlacklistedSKU(CsvReader csvReader)
        {
            var blacklistsku = new BlacklistedSkuDto();

            var eisSku = string.Empty;
            csvReader.TryGetField<string>("EisSKU", out eisSku);
            blacklistsku.EisSKU = eisSku;

            var isBlacklisted = false;
            csvReader.TryGetField<bool>("IsBlacklisted", out isBlacklisted);
            blacklistsku.IsBlacklisted = isBlacklisted;

            return blacklistsku;
        }

        private static Shadow extractShadow(CsvReader csvReader)
        {
            var shadow = new Shadow();

            string parentSKU = null;
            csvReader.TryGetField<string>("ParentSKU", out parentSKU);
            shadow.ParentSKU = parentSKU;

            string suffixSKU = string.Empty;
            csvReader.TryGetField<string>("ShadowSKU", out suffixSKU);
            shadow.SuffixSKU = suffixSKU;

            int? factorQuantity = null;
            csvReader.TryGetField<int?>("FactorQuantity", out factorQuantity);
            shadow.FactorQuantity = factorQuantity ?? 0;

            bool? isConnected = null;
            csvReader.TryGetField<bool?>("IsConnected", out isConnected);
            shadow.IsConnected = isConnected ?? false;
            shadow.isConnectedSet = isConnected != null;

            string Asin = null;
            csvReader.TryGetField<string>("Asin", out Asin);
            shadow.Asin = Asin;

            return shadow;
        }

        private static KitDetailDto extractKitDetail(CsvReader csvReader)
        {
            var kitDetail = new KitDetailDto();

            string parentKitSku = null;
            csvReader.TryGetField<string>("ParentKitSKU", out parentKitSku);
            kitDetail.ParentKitSKU = parentKitSku;

            string childKitSku;
            csvReader.TryGetField<string>("ChildKitSKU", out childKitSku);
            kitDetail.ChildKitSKU = childKitSku;

            bool? isMain = null;
            csvReader.TryGetField<bool?>("IsMain", out isMain);
            kitDetail.IsMain = isMain ?? false;

            int? quantity = null;
            csvReader.TryGetField<int?>("Quantity", out quantity);
            kitDetail.Quantity = quantity ?? 0;

            return kitDetail;
        }

        private static Product extractProductEis(CsvReader csvReader)
        {
            var productModel = new Product();

            string eisSku = null;
            csvReader.TryGetField<string>("General-EisSKU", out eisSku);
            productModel.EisSKU = eisSku;

            int companyId;
            csvReader.TryGetField<int>("General-CompanyId", out companyId);
            productModel.CompanyId = companyId;
            
            // the product file should contain the EisSKU or the SupplierSku
            if (eisSku == null && companyId == 0)
                throw new ArgumentException("File upload failed: One of the required items not found! \"CompanyId\"!");

            string productName = null;
            csvReader.TryGetField<string>("General-Name", out productName);
            productModel.Name = productName;

            string description = null;
            csvReader.TryGetField<string>("General-Description", out description);
            productModel.Description = description;

            string shortDescription = null;
            csvReader.TryGetField<string>("General-ShortDescription", out shortDescription);
            productModel.ShortDescription = shortDescription;

            string category = null;
            csvReader.TryGetField<string>("General-Category", out category);
            productModel.Category = category;

            string upcCode = null;
            csvReader.TryGetField<string>("General-UPC", out upcCode);
            productModel.UPC = upcCode;
            
            decimal? sellerPrice = null;
            csvReader.TryGetField<decimal?>("General-SellerPrice", out sellerPrice);
            productModel.SellerPrice = sellerPrice ?? 0;
            productModel.isSellerPriceSet = sellerPrice != null;

            decimal? pkgLength = null;
            csvReader.TryGetField<decimal?>("General-PkgLength", out pkgLength);
            productModel.PkgLength = pkgLength;

            decimal? pkgWidth = null;
            csvReader.TryGetField<decimal?>("General-PkgWidth", out pkgWidth);
            productModel.PkgWidth = pkgWidth;

            decimal? pkgHeight = null;
            csvReader.TryGetField<decimal?>("General-PkgHeight", out pkgHeight);
            productModel.PkgHeight = pkgHeight;

            string pkgLenghtUnit = null;
            csvReader.TryGetField<string>("General-PkgLenghtUnit", out pkgLenghtUnit);
            productModel.PkgLenghtUnit = pkgLenghtUnit;

            decimal? pkgWeight = null;
            csvReader.TryGetField<decimal?>("General-PkgWeight", out pkgWeight);
            productModel.PkgWeight = pkgWeight;

            string pkgWeightUnit = null;
            csvReader.TryGetField<string>("General-PkgWeightUnit", out pkgWeightUnit);
            productModel.PkgWeightUnit = pkgWeightUnit;

            decimal? itemLength = null;
            csvReader.TryGetField<decimal?>("General-ItemLength", out itemLength);
            productModel.ItemLength = itemLength;

            decimal? itemWidth = null;
            csvReader.TryGetField<decimal?>("General-ItemWidth", out itemWidth);
            productModel.ItemWidth = itemWidth;

            decimal? itemHeight = null;
            csvReader.TryGetField<decimal?>("General-ItemHeight", out itemHeight);
            productModel.ItemHeight = itemHeight;

            string itemLenghtUnit = null;
            csvReader.TryGetField<string>("General-ItemLenghtUnit", out itemLenghtUnit);
            productModel.ItemLenghtUnit = itemLenghtUnit;

            decimal? itemWeight = null;
            csvReader.TryGetField<decimal?>("General-ItemWeight", out itemWeight);
            productModel.ItemWeight = itemWeight;

            string itemWeightUnit = null;
            csvReader.TryGetField<string>("General-ItemWeightUnit", out itemWeightUnit);
            productModel.ItemWeightUnit = itemWeightUnit;

            decimal? guessedWeight = null;
            csvReader.TryGetField<decimal?>("General-GuessedWeight", out guessedWeight);
            productModel.GuessedWeight = guessedWeight;

            decimal? accurateWeight = null;
            csvReader.TryGetField<decimal?>("General-AccurateWeight", out accurateWeight);
            productModel.AccurateWeight = accurateWeight;

            string guessedWeightUnit = null;
            csvReader.TryGetField<string>("General-GuessedWeightUnit", out guessedWeightUnit);
            productModel.GuessedWeightUnit = guessedWeightUnit;

            string accurateWeightUnit = null;
            csvReader.TryGetField<string>("General-AccurateWeightUnit", out accurateWeightUnit);
            productModel.AccurateWeightUnit = accurateWeightUnit;

            string guessedShipping = null;
            csvReader.TryGetField<string>("General-GuessedShipping", out guessedShipping);
            productModel.GuessedShipping = guessedShipping;

            string accurateShipping = null;
            csvReader.TryGetField<string>("General-AccurateShipping", out accurateShipping);
            productModel.AccurateShipping = accurateShipping;

            bool? isKit = null;
            csvReader.TryGetField<bool?>("General-IsKit", out isKit);
            productModel.IsKit = isKit ?? false;
            productModel.isKitSet = isKit != null;

            bool? isBlacklisted = null;
            csvReader.TryGetField<bool?>("General-IsBlacklisted", out isBlacklisted);
            productModel.IsBlacklisted = isBlacklisted ?? false;
            productModel.isBlacklistedSet = isBlacklisted != null;

            int? skuType = null;
            csvReader.TryGetField<int?>("General-SkuType", out skuType);
            productModel.SkuType = skuType == null ? SkuType.Normal : (SkuType)skuType;
            productModel.isSkuTypeSet = skuType != null;

            string imageUrl;
            for (var counter = 1; counter <= 5; counter++)
            {
                var isSuccess = csvReader.TryGetField<string>(string.Format("ImageUrl{0}", counter), out imageUrl);
                if (isSuccess && !string.IsNullOrEmpty(imageUrl))
                    productModel.Images.Add(imageUrl);
            }

            return productModel;
        }

        private static ProductAmazon extractProductAmazon(CsvReader csvReader)
        {
            var amazon = new ProductAmazon();

            string eisSku = null;
            csvReader.TryGetField<string>("General-EisSKU", out eisSku);
            amazon.EisSKU = eisSku;

            string asin = null;
            csvReader.TryGetField<string>("Amazon-ASIN", out asin);
            amazon.ASIN = asin;

            decimal? price = null;
            csvReader.TryGetField<decimal?>("Amazon-Price", out price);
            amazon.Price = price;

            int? leadtimeShip = null;
            csvReader.TryGetField<int?>("Amazon-LeadtimeShip", out leadtimeShip);
            amazon.LeadtimeShip = leadtimeShip;

            int? packageQty = null;
            csvReader.TryGetField<int?>("Amazon-PackageQty", out packageQty);
            amazon.PackageQty = packageQty;

            int? numOfItems = null;
            csvReader.TryGetField<int?>("Amazon-NumOfItems", out numOfItems);
            amazon.NumOfItems = numOfItems;

            int? maxOrderQty = null;
            csvReader.TryGetField<int?>("Amazon-MaxOrderQty", out maxOrderQty);
            amazon.MaxOrderQty = maxOrderQty;

            string poductTitle = null;
            csvReader.TryGetField<string>("Amazon-ProductTitle", out poductTitle);
            amazon.ProductTitle = poductTitle;

            decimal mapPrice;
            csvReader.TryGetField<decimal>("Amazon-MapPrice", out mapPrice);
            amazon.MapPrice = mapPrice;

            bool? isAllowGiftWrap = null;
            csvReader.TryGetField<bool?>("Amazon-IsAllowGiftWrap", out isAllowGiftWrap);
            amazon.IsAllowGiftWrap = isAllowGiftWrap ?? false;
            amazon.isAllowGiftWrapSet = isAllowGiftWrap != null;

            bool? isAllowGiftMsg = null;
            csvReader.TryGetField<bool?>("Amazon-IsAllowGiftMsg", out isAllowGiftMsg);
            amazon.IsAllowGiftMsg = isAllowGiftMsg ?? false;
            amazon.isAllowGiftMsgSet = isAllowGiftMsg != null;

            string condition = null;
            csvReader.TryGetField<string>("Amazon-Condition", out condition);
            amazon.Condition = condition;

            string conditionNote = null;
            csvReader.TryGetField<string>("Amazon-ConditionNote", out conditionNote);
            amazon.ConditionNote = conditionNote;

            string fullFilledBy = null;
            csvReader.TryGetField<string>("Amazon-FulfilledBy", out fullFilledBy);
            amazon.FulfilledBy = fullFilledBy;

            string fbaSKU = null;
            csvReader.TryGetField<string>("Amazon-FbaSKU", out fbaSKU);
            amazon.FbaSKU = fbaSKU;

            bool? isEnabled = null;
            csvReader.TryGetField<bool?>("Amazon-IsEnabled", out isEnabled);
            amazon.IsEnabled = isEnabled ?? false;
            amazon.isEnabledSet = isEnabled != null;

            string productGroup = null;
            csvReader.TryGetField<string>("Amazon-ProductGroup", out productGroup);
            amazon.ProductGroup = productGroup;

            string productTypeName = null;
            csvReader.TryGetField<string>("Amazon-ProductTypeName", out productTypeName);
            amazon.ProductTypeName = productTypeName;

            string taxCode = null;
            csvReader.TryGetField<string>("Amazon-TaxCode", out taxCode);
            amazon.TaxCode = taxCode;
            
            //decimal? weightBox1 = null;
            //csvReader.TryGetField<decimal?>("Amazon-WeightBox1", out weightBox1);
            //amazon.WeightBox1 = weightBox1;

            //string weightBox1Unit = null;
            //csvReader.TryGetField<string>("Amazon-WeightBox1Unit", out weightBox1Unit);
            //amazon.WeightBox1Unit = weightBox1Unit;

            //decimal? weightBox2 = null;
            //csvReader.TryGetField<decimal?>("Amazon-WeightBox2", out weightBox2);
            //amazon.WeightBox2 = weightBox2;

            //string weightBox2Unit = null;
            //csvReader.TryGetField<string>("Amazon-WeightBox2Unit", out weightBox2Unit);
            //amazon.WeightBox2Unit = weightBox2Unit;

            return amazon;
        }

        private static ProducteBayDto extractProducteBay(CsvReader csvReader)
        {
            var eBay = new ProducteBayDto();

            string eisSku = null;
            csvReader.TryGetField<string>("General-EisSKU", out eisSku);
            eBay.EisSKU = eisSku;

            string itemId = null;
            csvReader.TryGetField<string>("eBay-ItemId", out itemId);
            eBay.ItemId = itemId;

            string title = null;
            csvReader.TryGetField<string>("eBay-Title", out title);
            eBay.Title = title;

            string subTitle = null;
            csvReader.TryGetField<string>("eBay-SubTitle", out subTitle);
            eBay.SubTitle = subTitle;

            string description = null;
            csvReader.TryGetField<string>("eBay-Description", out description);
            eBay.Description = description;

            int? listingQuantity = null;
            csvReader.TryGetField<int?>("eBay-ListingQuantity", out listingQuantity);
            eBay.ListingQuantity = listingQuantity ?? 1;
            eBay.isListingQuantitySet = listingQuantity != null;

            int? safetyQty = null;
            csvReader.TryGetField<int?>("eBay-SafetyQty", out safetyQty);
            eBay.SafetyQty = safetyQty ?? 1;
            eBay.isSafetyQtySet = safetyQty != null;

            int? categoryId = null;
            csvReader.TryGetField<int?>("eBay-CategoryId", out categoryId);
            eBay.CategoryId = categoryId ?? 0;
            eBay.isCategoryIdSet = categoryId != null;

            decimal? startPrice = null;
            csvReader.TryGetField<decimal?>("eBay-StartPrice", out startPrice);
            eBay.StartPrice = startPrice ?? 0;
            eBay.isStartPriceSet = startPrice != null;

            decimal? reservePrice;
            csvReader.TryGetField<decimal?>("eBay-ReservePrice", out reservePrice);
            eBay.ReservePrice = reservePrice ?? 0;
            eBay.isReservePriceSet = reservePrice != null;

            decimal? binPrice = null;
            csvReader.TryGetField<decimal?>("eBay-BinPrice", out binPrice);
            eBay.BinPrice = binPrice ?? 0;
            eBay.isBinPriceSet = binPrice != null;

            string listType = null;
            csvReader.TryGetField<string>("eBay-ListType", out listType);
            eBay.ListType = listType;

            string duration = null;
            csvReader.TryGetField<string>("eBay-Duration", out duration);
            eBay.Duration = duration;

            string location = null;
            csvReader.TryGetField<string>("eBay-Location", out location);
            eBay.Location = location;

            int? condition = null;
            csvReader.TryGetField<int?>("eBay-Condition", out condition);
            eBay.Condition_ = condition ?? 1000;
            eBay.isConditionSet = condition != null;

            int? dispatchTimeMax = null;
            csvReader.TryGetField<int?>("eBay-DispatchTimeMax", out dispatchTimeMax);
            eBay.DispatchTimeMax = dispatchTimeMax ?? 1;
            eBay.isDispatchTimeMaxSet = dispatchTimeMax != null;

            bool? isBoldTitle = null;
            csvReader.TryGetField<bool?>("eBay-IsBoldTitle", out isBoldTitle);
            eBay.IsBoldTitle = isBoldTitle ?? false;
            eBay.isBoldTitleSet = isBoldTitle != null;

            bool? isOutOfStockListing = null;
            csvReader.TryGetField<bool?>("eBay-IsOutOfStockListing", out isOutOfStockListing);
            eBay.IsOutOfStockListing = isOutOfStockListing ?? false;
            eBay.isOutOfStockListingSet = isOutOfStockListing != null;

            bool? isRequireAutoPayment = null;
            csvReader.TryGetField<bool?>("eBay-IsRequireAutoPayment", out isRequireAutoPayment);
            eBay.IsRequireAutoPayment = isRequireAutoPayment ?? false;
            eBay.isRequireAutoPaymentSet = isRequireAutoPayment != null;

            bool? isEnabled = null;
            csvReader.TryGetField<bool?>("eBay-IsEnabled", out isEnabled);
            eBay.IsEnabled = isEnabled ?? false;
            eBay.isEnabledSet = isEnabled != null;

            return eBay;
        }

        private static ProductBigCommerceDto extractProductBigCommerce(CsvReader csvReader)
        {
            var bigCommerce = new ProductBigCommerceDto();

            string eisSku = null;
            csvReader.TryGetField<string>("General-EisSKU", out eisSku);
            bigCommerce.EisSKU = eisSku;

            int? productId = null;
            csvReader.TryGetField<int?>("BigCommerce-ProductId", out productId);
            bigCommerce.ProductId = productId;

            int? categoryId = null;
            csvReader.TryGetField<int?>("BigCommerce-CategoryId", out categoryId);
            bigCommerce.CategoryId = categoryId;

            decimal? price = null;
            csvReader.TryGetField<decimal?>("BigCommerce-Price", out price);
            bigCommerce.Price = price;
            
            string condition = null;
            csvReader.TryGetField<string>("BigCommerce-Condition", out condition);
            bigCommerce.Condition = condition;
            
            string categories = null;
            csvReader.TryGetField<string>("BigCommerce-Categories", out categories);
            bigCommerce.Categories = categories;
            
            decimal? retailPrice = null;
            csvReader.TryGetField<decimal?>("BigCommerce-RetailPrice", out retailPrice);
            bigCommerce.RetailPrice = retailPrice;
            
            string primaryImage = null;
            csvReader.TryGetField<string>("BigCommerce-PrimaryImage", out primaryImage);
            bigCommerce.PrimaryImage = primaryImage;
            
            decimal? fixedCostShippingPrice = null;
            csvReader.TryGetField<decimal?>("BigCommerce-FixedCostShippingPrice", out fixedCostShippingPrice);
            bigCommerce.FixedCostShippingPrice = fixedCostShippingPrice;
            
            int? brand = null;
            csvReader.TryGetField<int?>("BigCommerce-Brand", out brand);
            bigCommerce.Brand = brand;
            
            int? productsType = null;
            csvReader.TryGetField<int?>("BigCommerce-ProductsType", out productsType);
            bigCommerce.ProductsType = productsType;
            
            int? inventoryLevel = null;
            csvReader.TryGetField<int?>("BigCommerce-InventoryLevel", out inventoryLevel);
            bigCommerce.InventoryLevel = inventoryLevel;
            
            int? inventoryWarningLevel = null;
            csvReader.TryGetField<int?>("BigCommerce-InventoryWarningLevel", out inventoryWarningLevel);
            bigCommerce.InventoryWarningLevel = inventoryWarningLevel;
            
            int? inventoryTracking = null;
            csvReader.TryGetField<int?>("BigCommerce-InventoryTracking", out inventoryTracking);
            bigCommerce.InventoryTracking = inventoryTracking;
            
            int? orderQuantityMinimum = null;
            csvReader.TryGetField<int?>("BigCommerce-OrderQuantityMinimum", out orderQuantityMinimum);
            bigCommerce.OrderQuantityMinimum = orderQuantityMinimum;
            
            int? orderQuantityMaximum = null;
            csvReader.TryGetField<int?>("BigCommerce-OrderQuantityMaximum", out orderQuantityMaximum);
            bigCommerce.OrderQuantityMaximum = orderQuantityMaximum;

            bool? isEnabled = null;
            csvReader.TryGetField<bool?>("BigCommerce-IsEnabled", out isEnabled);
            bigCommerce.IsEnabled = isEnabled ?? false;

            string description = null;
            csvReader.TryGetField<string>("BigCommerce-Description", out description);
            bigCommerce.Description = description;

            string title = null;
            csvReader.TryGetField<string>("BigCommerce-Title", out title);
            bigCommerce.Title = title;

            return bigCommerce;
        }
    }
}
