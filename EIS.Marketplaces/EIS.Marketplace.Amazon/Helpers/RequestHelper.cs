using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using EIS.Inventory.Core.ViewModels;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using AmazonWebServiceModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Marketplace.Amazon.Helpers
{
    public class RequestHelper
    {
        private static string _feedDirectory;
        private static string _amazonObjectsPath;
        static RequestHelper()
        {
            _feedDirectory = ConfigurationManager.AppSettings["MarketplaceFeedRoot"];
            _amazonObjectsPath = ConfigurationManager.AppSettings["ExtensionPath"];
        }

        public static string MerchantId { get; set; } // or seller id
        public static List<string> MarketplaceIdList { get; set; }
        public static string AccessKeyId { get; set; }
        public static string SecretKey { get; set; }
        public static string ServiceUrl { get { return "https://mws.amazonservices.com"; } }

        /// <summary>
        /// Create Amazon Envelope for the Amazon eisProduct specified
        /// </summary>
        /// <param name="products">The Amazon products to be created into Amazon envelope</param>
        /// <returns></returns>
        public static AmazonEnvelope CreateProductsFeedEnvelope(List<MarketplaceProductFeedDto> productPostFeeds,
            AmazonEnvelopeMessageOperationType operationType)
        {
            // create product post feed for Amazon
            var products = new List<AmazonWebServiceModels.Product>();
            productPostFeeds.ForEach(feedItem =>
            {
                products.Add(createProductModelForAmazon(feedItem));
            });

            // create Amazon envelope object
            var amazonEnvelope = new AmazonEnvelope
            {
                Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = MerchantId },
                MessageType = AmazonEnvelopeMessageType.Product,
                PurgeAndReplace = false,
                PurgeAndReplaceSpecified = true
            };

            // iterate to all products to update and convert it to envelope message
            var envelopeMessages = new List<AmazonEnvelopeMessage>();
            for (var i = 0; i < products.Count; i++)
            {
                var message = new AmazonEnvelopeMessage
                {
                    MessageID = string.Format("{0}", i + 1),
                    OperationType = operationType,
                    OperationTypeSpecified = true,
                    Item = products[i],
                };

                envelopeMessages.Add(message);
            }

            // convert to array and set its message
            amazonEnvelope.Message = envelopeMessages.ToArray();

            return amazonEnvelope;
        }

        /// <summary>
        /// Create Amazon envelope for the single eisProduct feed
        /// </summary>
        /// <param name="productPostFeed">The EIS eisProduct object</param>        
        /// <param name="operationType">The type of Amazon operation</param>
        /// <returns></returns>
        public static AmazonEnvelope CreateSingleProductFeedEnvelope(MarketplaceProductFeedDto productPostFeed,
            AmazonEnvelopeMessageOperationType operationType)
        {
            // create Amazon envelope object
            var amazonEnvelope = new AmazonEnvelope
            {
                Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = MerchantId },
                MessageType = AmazonEnvelopeMessageType.Product,
                PurgeAndReplace = false,
                PurgeAndReplaceSpecified = true
            };

            // create the eisProduct envelope message
            var envelopeMessages = new List<AmazonEnvelopeMessage>();
            var message = new AmazonEnvelopeMessage
            {
                MessageID = "1",
                OperationType = operationType,
                OperationTypeSpecified = true,
                Item = createProductModelForAmazon(productPostFeed)
            };

            // convert to array and set its message
            envelopeMessages.Add(message);
            amazonEnvelope.Message = envelopeMessages.ToArray();

            return amazonEnvelope;
        }

        /// <summary>
        /// Create price feed envelope for items which have update pricing
        /// </summary>
        /// <param name="priceItems">The list of items which have updated pricing</param>
        /// <returns></returns>
        public static AmazonEnvelope CreatePriceFeedEnvelope(List<AmazonPriceFeed> priceItems)
        {
            // convert the categorized eisProduct into Price feed
            var priceFeeds = new List<Price>();
            priceItems.ForEach(x =>
            {
                var product = new Price
                {
                    SKU = x.SKU,
                    StandardPrice = new OverrideCurrencyAmount
                    {
                        currency = BaseCurrencyCodeWithDefault.USD,
                        Value = x.StandardPrice
                    }
                };

                if (x.MapPrice > 0)
                {
                    product.MAP = new OverrideCurrencyAmount
                    {
                        currency = BaseCurrencyCodeWithDefault.USD,
                        Value = x.MapPrice
                    };
                }

                priceFeeds.Add(product);
            });
            
            // create Amazon envelope object
            var amazonEnvelope = new AmazonEnvelope
            {
                Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = MerchantId },
                MessageType = AmazonEnvelopeMessageType.Price
            };

            // add all Price feeds as messages to the envelope
            var envelopeMessages = new List<AmazonEnvelopeMessage>();
            for (var i = 0; i < priceFeeds.Count; i++)
            {
                var message = new AmazonEnvelopeMessage
                {
                    MessageID = string.Format("{0}", i + 1),
                    Item = priceFeeds[i]
                };
                envelopeMessages.Add(message);
            }

            // convert to array and set its message
            amazonEnvelope.Message = envelopeMessages.ToArray();

            return amazonEnvelope;
        }


        /// <summary>
        /// Create inventory feed for item's quantity
        /// </summary>
        /// <param name="inventoryItems">The list of items that are to have their quantities updated</param>
        /// <returns></returns>
        public static AmazonEnvelope CreateInventoryFeedEnvelope(List<AmazonInventoryFeed> inventoryItems)
        { 
            // iterate to the EIS products and parsed it into Invetory Feed
            var inventories = new List<AmazonWebServiceModels.Inventory>();
            inventoryItems.ForEach(x =>
            {
                inventories.Add(new AmazonWebServiceModels.Inventory
                {
                    SKU = x.SKU,
                    Item = x.InventoryQuantity.ToString(),
                    // set the default leadtime shipment to 3 days
                    FulfillmentLatency = (x.LeadtimeShip ?? 0) == 0 ? "3" : x.LeadtimeShip.ToString(),
                });
            });

            // create Amazon envelope object
            var amazonEnvelope = new AmazonEnvelope
            {
                Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = MerchantId },
                MessageType = AmazonEnvelopeMessageType.Inventory
            };

            // add all Inventory feeds as messages to the envelope
            var envelopeMessages = new List<AmazonEnvelopeMessage>();
            for (var i = 0; i < inventories.Count; i++)
            {
                var message = new AmazonEnvelopeMessage
                {
                    MessageID = string.Format("{0}", i + 1),
                    OperationType = AmazonEnvelopeMessageOperationType.Update,
                    Item = inventories[i]
                };
                envelopeMessages.Add(message);
            }

            // convert to array and set its message
            amazonEnvelope.Message = envelopeMessages.ToArray();

            return amazonEnvelope;
        }

        /// <summary>
        /// Create GetFeedSubmissionListRequest with the specified submission IDs
        /// </summary>
        /// <param name="submissionIds"></param>
        /// <returns></returns>
        public static GetFeedSubmissionListRequest CreateFeedSubmissionListRequest(List<string> submissionIds)
        {
            var request = new GetFeedSubmissionListRequest
            {
                Merchant = MerchantId,
                FeedSubmissionIdList = new IdList { Id = submissionIds },
            };

            return request;
        }

        /// <summary>
        /// Create GetFeedSubmissionResultRequest object with the submission ID
        /// </summary>
        /// <param name="submissionId">The id of submission</param>
        /// <returns></returns>
        public static GetFeedSubmissionResultRequest CreateFeedSubmissionResultRequest(string submissionId)
        {
            var request = new GetFeedSubmissionResultRequest
            {
                Merchant = MerchantId,
                FeedSubmissionId = submissionId,
            };

            return request;
        }

        /// <summary>
        /// Create SubmitFeedRequest object with the specified XML file path and the type of the feed
        /// </summary>
        /// <param name="filePath">The full file path of the XML</param>
        /// <param name="feedType">The type of Amazon feed</param>
        /// <returns></returns>
        public static SubmitFeedRequest CreateSubmitFeedRequest(string filePath, AmazonFeedType feedType)
        {
            // create submit feed request
            var feedRequest = new SubmitFeedRequest
            {
                Merchant = MerchantId,
                MarketplaceIdList = new IdList { Id = MarketplaceIdList },
                FeedType = feedType.ToString(),
                ContentType = new ContentType(MediaType.OctetStream),
                FeedContent = File.Open(filePath, FileMode.Open, FileAccess.Read)
            };

            feedRequest.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(feedRequest.FeedContent);

            return feedRequest;
        }

        /// <summary>
        /// Set the Amazon client credentials for posting feeds
        /// </summary>
        /// <param name="credential">An object contains the Amazon credentials</param>
        public static void SetCredentials(AmazonCredentialDto credential)
        {
            MerchantId = credential.MerchantId;
            MarketplaceIdList = new List<string> { credential.MarketplaceId };
            AccessKeyId = credential.AccessKeyId;
            SecretKey = credential.SecretKey;
        }

        private static AmazonWebServiceModels.Product createProductModelForAmazon(MarketplaceProductFeedDto productPost)
        {
            var product = new AmazonWebServiceModels.Product();
            try
            {
                product.SKU = productPost.EisSKU;
                product.LaunchDate = DateTime.UtcNow;
                product.ReleaseDate = DateTime.UtcNow;
                product.StandardProductID = new StandardProductID
                {
                    Type = StandardProductIDType.ASIN,
                    Value = productPost.AmazonProductFeed.ASIN
                };
                product.ProductTaxCode = productPost.AmazonProductFeed.TaxCode;
                product.Condition = new ConditionInfo
                {
                    ConditionType = parsedCondition(productPost.AmazonProductFeed.Condition),
                    ConditionNote = productPost.AmazonProductFeed.ConditionNote
                };

                if ((productPost.AmazonProductFeed.PackageQty ?? 0) > 0)
                    product.ItemPackageQuantity = productPost.AmazonProductFeed.PackageQty.ToString();

                if ((productPost.AmazonProductFeed.NumOfItems ?? 0) > 0)
                    product.NumberOfItems = productPost.AmazonProductFeed.NumOfItems.ToString();

                // create the instance for each category and sub-category
                var mainCategoryObj = _magicallyCreateInstance(productPost.ProductType.AmazonMainClassName);
                var subCategoryObj = _magicallyCreateInstance(productPost.ProductType.AmazonSubClassName);
                var productTypeObj = _magicallyCreateProductType(productPost.ProductType.AmazonMainClassName, subCategoryObj);

                // set the eisProduct type for main category
                _setProductTypeForMainCategory(mainCategoryObj, productTypeObj);

                product.ProductData = new ProductProductData { Item = mainCategoryObj, };

                product.DescriptionData = new ProductDescriptionData
                {
                    Title = productPost.Name,
                    Description = productPost.Description,
                    IsGiftWrapAvailable = productPost.AmazonProductFeed.IsAllowGiftWrap,
                    IsGiftWrapAvailableSpecified = true,
                    IsGiftMessageAvailable = productPost.AmazonProductFeed.IsAllowGiftMsg,
                    IsGiftMessageAvailableSpecified = true,
                };

                if (!string.IsNullOrEmpty(productPost.Brand))
                    product.DescriptionData.Brand = productPost.Brand;
                
                if ((productPost.AmazonProductFeed.MaxOrderQty ?? 0) > 0)
                    product.DescriptionData.MaxOrderQuantity = productPost.AmazonProductFeed.MaxOrderQty.ToString();

                // set the product's package dimensions
                if (((productPost.PkgLength ?? 0) != 0) || ((productPost.PkgWidth ?? 0) != 0) || ((productPost.PkgHeight ?? 0) != 0))
                {
                    product.DescriptionData.PackageDimensions = new Dimensions()
                    {
                        Length = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.PkgLenghtUnit),
                            Value = productPost.PkgLength ?? 0
                        },
                        Width = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.PkgLenghtUnit),
                            Value = productPost.PkgWidth ?? 0
                        },
                        Height = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.PkgLenghtUnit),
                            Value = productPost.PkgHeight ?? 0
                        }
                    };
                }

                // set the product's package weight
                if((productPost.PkgWeight ?? 0) != 0)
                {
                    product.DescriptionData.PackageWeight = new PositiveNonZeroWeightDimension
                    {
                        unitOfMeasure = parsedUnitOfWeightMeasurement(productPost.PkgWeightUnit),
                        Value = productPost.PkgWeight ?? 1
                    };
                }

                // set the product's item dimensions
                if (((productPost.ItemLength ?? 0) != 0) || ((productPost.ItemWidth ?? 0) != 0) || ((productPost.ItemHeight ?? 0) != 0))
                {
                    product.DescriptionData.ItemDimensions = new Dimensions()
                    {
                        Length = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.ItemLenghtUnit),
                            Value = productPost.ItemLength ?? 0
                        },
                        Width = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.ItemLenghtUnit),
                            Value = productPost.ItemWidth ?? 0
                        },
                        Height = new LengthDimension
                        {
                            unitOfMeasure = parsedUnitOfLengthMeasurement(productPost.ItemLenghtUnit),
                            Value = productPost.ItemHeight ?? 0
                        }
                    };
                }

                // set the product's item weight ?
                if ((productPost.ItemWeight ?? 0) != 0)
                {
                    product.DescriptionData.ShippingWeight = new PositiveWeightDimension
                    {
                        unitOfMeasure = parsedUnitOfWeightMeasurement(productPost.ItemWeightUnit),
                        Value = productPost.ItemWeight ?? 1
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return product;
        }
        
        private static void _setProductTypeForMainCategory(object mainCategory, object productTypeObj)
        {
            var type = mainCategory.GetType();
            if (type.Name == "Clothing")
            {
                var classificationData = _magicallyCreateInstance("ClothingClassificationData");
                var classifcationData = new ClothingClassificationData();
                classifcationData.ClothingType = (ClothingClassificationDataClothingType)productTypeObj;
                
                //type.InvokeMember("ClothingType", BindingFlags.SetProperty, null, classificationData, new object[] { productTypeObj });
                type.InvokeMember("ClassificationData", BindingFlags.SetProperty, null, mainCategory, new object[] { classificationData });
            }
            else
            {
                type.InvokeMember("ProductType", BindingFlags.SetProperty, null, mainCategory, new object[] { productTypeObj });
            }
        }

        private static object _magicallyCreateProductType(string mainCatClassName, object subCategoryObj)
        {
            object productType = null;

            // create the instance for the product type
            if (mainCatClassName == "Miscellaneous")
            {
                productType = _magicallyCreateInstance("MiscType");             
            }
            else if (mainCatClassName == "Clothing")
            {
                productType = _magicallyCreateInstance("ClothingClassificationDataClothingType");
            }
            else
            {
                productType = _magicallyCreateInstance(string.Format("{0}ProductType", mainCatClassName));
            }

            // set the value for the product type
            var type = productType.GetType();
            if (type.IsEnum)
            {
                var enumValue = subCategoryObj.GetType() == typeof(string) ? subCategoryObj.ToString() : subCategoryObj.GetType().Name;
                productType = Enum.Parse(type, enumValue);
            }
            else
            {
                type.InvokeMember("Item", BindingFlags.SetProperty, null, productType, new object[] { subCategoryObj });
            }

            return productType;
        }

        private static object _magicallyCreateInstance(string className)
        {
            // var assembly = Assembly.GetExecutingAssembly();
            var assembly = Assembly.LoadFrom(string.Format("{0}/AmazonWebServiceModels.dll", _amazonObjectsPath));
            var type = assembly.GetTypes().FirstOrDefault(x => x.Name == className);
            if (type == null)
                return className;

            return Activator.CreateInstance(type);
        }

        private static ConditionType parsedCondition(string condition)
        {
            switch (condition)
            {
                case "New":
                    return ConditionType.New;
                case "Used":
                    return ConditionType.UsedLikeNew;
                case "Collectible":
                    return ConditionType.CollectibleLikeNew;
                case "Refurbished":
                    return ConditionType.Refurbished;
                case "Club":
                    return ConditionType.Club;
                default:
                    return ConditionType.New;
            }
        }

        private static WeightUnitOfMeasure parsedUnitOfWeightMeasurement(string unit)
        {
            switch (unit)
            {
                case "pounds":
                    return WeightUnitOfMeasure.LB;
                case "ounces":
                    return WeightUnitOfMeasure.OZ;
                case "grams":
                    return WeightUnitOfMeasure.GR;
                case "kilograms":
                    return WeightUnitOfMeasure.KG;
                default:
                    return WeightUnitOfMeasure.GR;
            }
        }

        private static LengthUnitOfMeasure parsedUnitOfLengthMeasurement(string unit)
        {
            switch (unit)
            {
                case "inches":
                   return LengthUnitOfMeasure.inches;
                case "feet":
                   return LengthUnitOfMeasure.feet;
                case "millimeters":
                   return LengthUnitOfMeasure.MM;
                case "centimeters":
                   return LengthUnitOfMeasure.CM;
                case "decimeters":
                   return LengthUnitOfMeasure.decimeters;
                case "meters":
                   return LengthUnitOfMeasure.M;
                default:
                   return LengthUnitOfMeasure.MM;
            }
        }
    }
}
