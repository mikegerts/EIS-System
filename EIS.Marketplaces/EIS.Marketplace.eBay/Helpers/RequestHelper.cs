using System;
using System.Linq;
using eBay.Service.Core.Soap;
using eBay.Service.Core.Sdk;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Marketplace.eBay.Helpers
{
    public static class RequestHelper
    {
        private static ApiCredential _apiCredential;
        private static string _payPalEmailAddress;
        private static string _mode;

        public static void SetCredentials(eBayCredentialDto credential)
        {
            _apiCredential = new ApiCredential();
            _apiCredential.ApiAccount = new ApiAccount
            {
                Application = credential.ApplicationId,
                Developer = credential.DeveloperId,
                Certificate = credential.CertificationId
            };
            _apiCredential.eBayToken = credential.UserToken;
            _payPalEmailAddress = credential.PayPalEmailAddress;
            _mode = credential.Mode;
        }

        public static ApiCredential ApiCredential
        {
            get { return _apiCredential; }
        }

        public static string ServiceUrl
        {
            get
            {
                return _mode == "LIVE" ? "https://api.ebay.com/wsapi" : "https://api.sandbox.ebay.com/wsapi";
            }
        }

        public static ItemType CreateItemType(MarketplaceProductFeedDto productFeed, string eBayDescriptionTemplate)
        {
            try
            {
                var item = new ItemType();

                // set-up the defaults
                item.Currency = CurrencyCodeType.USD;
                item.CurrencySpecified = true;
                item.Country = CountryCodeType.US;
                item.CountrySpecified = true;

                item.PaymentMethods = new BuyerPaymentMethodCodeTypeCollection();
                item.PaymentMethods.AddRange(new BuyerPaymentMethodCodeType[] { BuyerPaymentMethodCodeType.PayPal });
                item.PayPalEmailAddress = _payPalEmailAddress;
                item.AutoPay = productFeed.eBayProductFeed.IsRequireAutoPayment;
                item.RegionID = "0";

                // set the specified values from the passed objects
                item.SKU = productFeed.EisSKU;
                item.Title = productFeed.eBayProductFeed.Title;
                item.InventoryTrackingMethod = InventoryTrackingMethodCodeType.SKU;
                item.InventoryTrackingMethodSpecified = true;

                // init the item description from the template
                if (!string.IsNullOrEmpty(eBayDescriptionTemplate))
                    item.Description = getParsedeBayDescriptionTemplate(productFeed, eBayDescriptionTemplate);
                else
                    item.Description = productFeed.eBayProductFeed.Description;

                if (!string.IsNullOrEmpty(productFeed.eBayProductFeed.SubTitle))
                {
                    item.SubTitle = productFeed.eBayProductFeed.SubTitle;
                }

                // set the product quantity if its more than ZERO
                if (productFeed.eBayInventoryFeed.InventoryQuantity > 0)
                {
                    item.Quantity = productFeed.eBayInventoryFeed.InventoryQuantity;
                    item.QuantitySpecified = true;
                }

                // set the product's category
                item.PrimaryCategory = new CategoryType();
                item.PrimaryCategory.CategoryID = productFeed.eBayProductFeed.CategoryId != null 
                    ? productFeed.eBayProductFeed.CategoryId.ToString() : productFeed.ProductType.EbaySubCategoryCode; 

                // set the product's condion
                item.ConditionID = productFeed.eBayProductFeed.Condition_; // 1000 - New, 3000 - Used
                item.ConditionIDSpecified = true;
                item.Location = productFeed.eBayProductFeed.Location; // should be the postal code

                if (!string.IsNullOrEmpty(productFeed.UPC))
                {
                    item.ProductListingDetails = new ProductListingDetailsType
                    {
                        UPC = productFeed.UPC,
                    };
                }

                // set the listing type for this product
                item.ListingType = EnumHelper.ParseEnum<ListingTypeCodeType>(productFeed.eBayProductFeed.ListType); // should we explicitly set to FixedPriceItem
                item.ListingTypeSpecified = true;
                item.ListingDuration = productFeed.eBayProductFeed.Duration;

                // set its start and reserver price for the Auction listing type
                if (productFeed.eBayProductFeed.ListType == ListingTypeCodeType.Auction.ToString())
                {
                    item.StartPrice = new AmountType
                    {
                        currencyID = CurrencyCodeType.USD,
                        Value = (double)productFeed.eBayInventoryFeed.StartPrice
                    };
                    item.BuyItNowPrice = new AmountType
                    {
                        currencyID = CurrencyCodeType.USD,
                        Value = (double)productFeed.eBayInventoryFeed.BinPrice
                    };
                    item.ReservePrice = new AmountType
                    {
                        currencyID = CurrencyCodeType.USD,
                        Value = (double)productFeed.eBayInventoryFeed.ReservePrice
                    };
                }else
                {
                    // otherwise,it's a fixed price item, we will use the BinPrice/BuyItNowPrice for its StartPrice
                    item.StartPrice = new AmountType
                    {
                        currencyID = CurrencyCodeType.USD,
                        Value = (double)productFeed.eBayInventoryFeed.BinPrice
                    };
                }
                
                // add the product images
                if (productFeed.ImageUrls.Any())
                {
                    // set the picture details type
                    item.PictureDetails = new PictureDetailsType();
                    item.PictureDetails.GalleryType = GalleryTypeCodeType.None;

                    item.PictureDetails.PictureURL = new StringCollection();
                    for (var index = 0; index < productFeed.ImageUrls.Count; index++)
                    {
                        var imageUrl = productFeed.ImageUrls[index];
#if DEBUG
                        imageUrl = "https://d1wj636tbzfwwf.cloudfront.net/static/images/studios/homepage/stories-brandon-1.jpg";
#endif
                        item.PictureDetails.PictureURL.Add(imageUrl);
                    }
                }

                // add the product enhancements
                var enhancements = new ListingEnhancementsCodeTypeCollection();
                if (productFeed.eBayProductFeed.IsBoldTitle)
                    enhancements.Add(ListingEnhancementsCodeType.BoldTitle);

                if (enhancements.Count > 0)
                    item.ListingEnhancement = enhancements;
                
                item.DispatchTimeMax = productFeed.eBayProductFeed.DispatchTimeMax;

                // add the item shipping information
                item.ShippingDetails = createShippingDetails(productFeed.eBayProductFeed);

                // add the policy for the item
                item.ReturnPolicy = createPolicyForUS(productFeed.eBayProductFeed);

                return item;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a policy for US site only
        /// </summary>
        /// <returns></returns>
        private static ReturnPolicyType createPolicyForUS(eBayProductFeed eBayInfo)
        {
            return new ReturnPolicyType
            {
                Refund = string.IsNullOrEmpty(eBayInfo.RefundOption) ? "MoneyBack" : eBayInfo.RefundOption,
                ReturnsWithinOption = string.IsNullOrEmpty(eBayInfo.ReturnsWithinOption) ? "Days_14" : eBayInfo.ReturnsWithinOption,
                ReturnsAcceptedOption = string.IsNullOrEmpty(eBayInfo.ReturnsAcceptedOption) ? "ReturnsAccepted" : eBayInfo.ReturnsAcceptedOption,
                ShippingCostPaidByOption = string.IsNullOrEmpty(eBayInfo.ShippingCostPaidByOption) ? "Buyer" : eBayInfo.ShippingCostPaidByOption,
                Description = string.IsNullOrEmpty(eBayInfo.ReturnPolicyDescription) ? string.Empty : eBayInfo.ReturnPolicyDescription // "We accept returns only if there was a mistake on our end. Your order must be returned in the same condition as received to receive a refund.",                
            };
        }

        /// <summary>
        /// Set the item's shipping information
        /// </summary>
        /// <returns></returns>
        private static ShippingDetailsType createShippingDetails(eBayProductFeed eBayInfo)
        {
            // let's determine the shipping type
            var shippingType = EnumHelper.ParseEnum<ShippingTypeCodeType>(eBayInfo.ShippingType, ShippingTypeCodeType.Free);

            // create shipping details and set its shipping type
            var shippingDetails = new ShippingDetailsType();
            var serviceOption = new ShippingServiceOptionsType();

            if (shippingType == ShippingTypeCodeType.Flat)
            {
                shippingDetails.ShippingType = ShippingTypeCodeType.Flat;

                // create the shipping service
                serviceOption.ShippingService = eBayInfo.ShippingService;
                serviceOption.ShippingServicePriority = 1;
                serviceOption.ShippingServiceCost = new AmountType
                {
                    currencyID = CurrencyCodeType.USD,
                    Value = eBayInfo.ShippingServiceCost
                };
            }
            else // shipping type is free
            {
                // force it to flat but this is for free shipping
                //shippingDetails.ShippingType = ShippingTypeCodeType.Free; 

                // create the shipping service and let's set the shipping cost to 0
                serviceOption.ShippingService = ShippingServiceCodeType.ShippingMethodStandard.ToString(); // this is just arbitrary shippng service only                
                serviceOption.ShippingServicePriority = 1;
                serviceOption.ShippingServiceCost = new AmountType
                {
                    currencyID = CurrencyCodeType.USD,
                    Value = 0 // eBayInfo.ShippingServiceCost
                };
            }

            // set the shipping service type
            shippingDetails.ShippingServiceOptions = new ShippingServiceOptionsTypeCollection(
                new ShippingServiceOptionsType[] { serviceOption });

            return shippingDetails;
        }

        private static string getParsedeBayDescriptionTemplate(MarketplaceProductFeedDto productFeed, string eBayDescriptionTemplate)
        {
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[Product.EisSKU]", productFeed.EisSKU);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[Product.Name]", productFeed.Name);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[Product.Description]", productFeed.Description);

            // replace the images if there's any
            for (var index = 0; index < 5 && index < productFeed.ImageUrls.Count; index++)
            {
                var imageUrl = productFeed.ImageUrls[index];
#if DEBUG
                imageUrl = "https://d1wj636tbzfwwf.cloudfront.net/static/images/studios/homepage/stories-brandon-1.jpg";
#endif
                eBayDescriptionTemplate = eBayDescriptionTemplate.Replace(string.Format("[Product.ImageURL{0}]", index + 1), imageUrl);
            }

            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[ProducteBay.ItemId]", productFeed.eBayProductFeed.ItemId);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[ProducteBay.Title]", productFeed.eBayProductFeed.Title);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[ProducteBay.SubTitle]", productFeed.eBayProductFeed.SubTitle);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[ProducteBay.Description]", productFeed.eBayProductFeed.Description);
            eBayDescriptionTemplate = eBayDescriptionTemplate.Replace("[Product.FactorQty]", productFeed.FactorQuantity.ToString());

            return eBayDescriptionTemplate;
        }
    }
}
