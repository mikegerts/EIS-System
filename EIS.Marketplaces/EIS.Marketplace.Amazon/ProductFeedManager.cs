using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using EIS.Marketplace.Amazon.Helpers;
using EIS.Marketplace.Amazon.Services;
using EIS.Marketplace.Amazon.Services.Core;
using EIS.Marketplace.Amazon.Services.Models;

namespace EIS.Marketplace.Amazon
{
    public class ProductFeedManager
    {
        private string _merchantId = "A1OPIP5HGU5RID";
        private string _marketplaceId = "ATVPDKIKX0DER";
        private string _developerId = "8467-2553-0197";
        private string _awsAccessKey = "AKIAJ2S4WUDF5FFYKCJA";
        private string _secretKey = "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";


        public void CheckFeedSubmissionResultId(string feedId)
        {
            //50050016655
            var list = new List<String>();
            list.Add("50048016655");

            var request = new GetFeedSubmissionListRequest
            {
                Merchant = _merchantId,
                Marketplace = _marketplaceId,
                FeedSubmissionIdList = new IdList { Id = list },
            };

            var result = _getWebServiceClient().GetFeedSubmissionList(request);
        }

        public void SendProductsFeeds(List<List<object>> products)
        {
            var masterProductList = new List<List<object>>();
            var amazonProducts = CreateAmazonProducts();

            var batchProductList = new List<object>();
            var counter = 0;
            foreach (var product in amazonProducts)
            {
                batchProductList.Add(product);
                counter ++;
                if (counter == 1)
                {
                    masterProductList.Add(batchProductList);
                    batchProductList = new List<object>();
                }
            }

            // add the remaining product list
            if (batchProductList.Any())
                masterProductList.Add(batchProductList);

            // let's send the product feed to the amazon
            var submissionIds = SendAmazonFeeds(masterProductList, AmazonEnvelopeMessageType.Product, AmazonFeedType._POST_PRODUCT_DATA_);
        }

        private List<string> SendAmazonFeeds(List<List<object>> amazonUpdateList, AmazonEnvelopeMessageType messageType, AmazonFeedType feedType)
        {
            {
                var masterCounter = 1;
                var returnResult = new List<string>();
                foreach (var amazonUpdateGroup in amazonUpdateList)
                {
                    var amazonEnvelope = new AmazonEnvelope
                    {
                        Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = _merchantId, },
                        MessageType = messageType,
                        PurgeAndReplace = false,
                        PurgeAndReplaceSpecified = true
                    };

                    var updates = new List<AmazonEnvelopeMessage>();
                    var counter = 1;
                    foreach (var amazonUpdate in amazonUpdateGroup)
                    {
                        var curUpdate = new AmazonEnvelopeMessage
                        {
                            MessageID = counter.ToString(),
                            OperationType = AmazonEnvelopeMessageOperationType.Update,
                            //OperationTypeSpecified = true,
                            Item = amazonUpdate
                        };
                        updates.Add(curUpdate);
                        counter++;
                    }
                    amazonEnvelope.Message = updates.ToArray();
                    var xmlString = MarketplaceHelper.ParseObjectToXML(amazonEnvelope);
                    var path = "D:\\logs\\";
                    var fileName = string.Format("Amazon{0}Feed_{1}{3}.{2}", messageType, masterCounter, "xml", DateTime.Now.Second);
                    var documentFileName = Path.Combine(path, fileName);
                    File.WriteAllText(documentFileName, xmlString);
                    if (!File.Exists(documentFileName))
                    {
                        throw new ArgumentException("SendFeed document not generated properly");
                    }
                    var feedRequest = new SubmitFeedRequest
                    {
                        Merchant = _merchantId,
                        MarketplaceIdList =
                        new IdList { Id = new List<string>(new[] { _marketplaceId }) },
                        FeedType = feedType.ToString(),
                        ContentType = new ContentType(MediaType.OctetStream),
                        FeedContent = File.Open(documentFileName, FileMode.Open, FileAccess.Read)
                    };
                    feedRequest.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(feedRequest.FeedContent);
                    var feedConfig = new MarketplaceWebServiceConfig { ServiceURL = "https://mws.amazonservices.com" };
                    var feedService = new MarketplaceWebServiceClient(_awsAccessKey, _secretKey, "Demac", "1.01", feedConfig);
                    var uploadSuccess = false;
                    var retryCount = 0;
                    while (!uploadSuccess)
                    {
                        try
                        {
                            var feedResponse = feedService.SubmitFeed(feedRequest);
                            var submissionId = feedResponse.SubmitFeedResult.FeedSubmissionInfo.FeedSubmissionId;
                            returnResult.Add(submissionId);
                            uploadSuccess = true;
                            masterCounter++;
                            //Thread.Sleep(120000);
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount == 3) break;
                            //Thread.Sleep(18000);
                            if (ex.ToString().ToLowerInvariant().Contains("request is throttled")) continue;
                            returnResult.Add(string.Format("ERROR: {0}", ex));
                        }
                    }
                }
                return returnResult;
            }
        }

        public SubmitFeedResult SubmitFeed(MarketplaceWebService service, FileInfo fileInfo, AmazonFeedType feedType)
        {
            var response = new SubmitFeedResponse();

            var sfRequest = new SubmitFeedRequest();
            sfRequest.Merchant = _merchantId;
            sfRequest.MarketplaceIdList = new IdList { Id = new List<string>(new[] { _marketplaceId }) };

            using (var stream = new FileStream(fileInfo.Name, FileMode.Open, FileAccess.ReadWrite))
            {
                sfRequest.FeedContent = stream;
                sfRequest.ContentMD5 = MarketplaceWebServiceClient.CalculateContentMD5(sfRequest.FeedContent);
                sfRequest.FeedContent.Position = 0;
                sfRequest.FeedType = feedType.ToString();

                response = service.SubmitFeed(sfRequest);
            }

            return response.SubmitFeedResult;
        }


        public List<Product> CreateAmazonProducts()
        {
            var products = new List<Product>();

            var p1 = new Product();
            p1.Condition = new ConditionInfo { ConditionType = ConditionType.New };
            p1.SKU = string.Format("KI{0}", DateTime.Now.Second);
            p1.StandardProductID = new StandardProductID
            {
                Type = StandardProductIDType.ASIN,
                Value = "B005S25V5Z", //"B002YUF8ZG"
            };
            p1.ProductTaxCode = "A_GEN_NOTAX";
            p1.NumberOfItems = "23";

            var mainCategory = new Home();
            var subCategory = new Kitchen();

            mainCategory.ProductType = new HomeProductType { Item = subCategory };

            p1.ProductData = new ProductProductData
            {
                Item = mainCategory
            };

            p1.DescriptionData = new ProductDescriptionData
            {
                Brand = "SONY ERRICCSON",
                BulletPoint = new string[] { "Awesome phone", "Water resistant", "Take great picture", "Make you feel awesome" },
                Description = "This is very fanstastic phone that ever produced in this era!",
                Title = "SONY XPERIA Z3 COMPACT",
                MSRP = new CurrencyAmount { currency = BaseCurrencyCode.USD, Value = 111 },
                ItemDimensions = new Dimensions
                {
                    Height = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters },
                    Length = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters },
                    Width = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters }
                },
                ShippingWeight = new PositiveWeightDimension { unitOfMeasure = WeightUnitOfMeasure.KG, Value = 1.3m },

            };

            var p2 = new Product();
            p2.Condition = new ConditionInfo { ConditionType = ConditionType.New };
            p2.SKU = string.Format("MUSHI{0}", DateTime.Now.Second);
            p2.StandardProductID = new StandardProductID
            {
                Type = StandardProductIDType.ASIN,
                Value = "B0EXAMPLEG", // "B002YUF8ZG"
            };
            p2.ProductTaxCode = "A_GEN_NOTAX";

            var mainCategory2 = new Health();
            var subCategory2 = new HealthMisc
            {
                Ingredients = new string[] { "Sugar Coffe" },
                Directions = "Mix the ingredients and stir thoroughly!"
            };

            mainCategory2.ProductType = new HealthProductType { Item = subCategory2 };
            
            p2.ProductData = new ProductProductData
            {
                Item = mainCategory2
            };

            p2.DescriptionData = new ProductDescriptionData
            {
                Brand = "SOny Ericcson",
                BulletPoint = new string[] { "Not-so awesome phone", "non - Water resistant", "Take better picture", "Make you feel better" },
                Description = "This is verry fanstastic phone that ever produced in this era!",
                Title = "Sony Xperia Z3",
                MSRP = new CurrencyAmount { currency = BaseCurrencyCode.USD, Value = 123 },                
            };


            var p3 = new Product();
            p3.Condition = new ConditionInfo { ConditionType = ConditionType.New };
            p3.SKU = string.Format("KIGWA{0}", DateTime.Now.Second);
            p3.StandardProductID = new StandardProductID
            {
                Type = StandardProductIDType.UPC,
                Value = "4015643103921"
            };
            p3.ProductTaxCode = "A_GEN_NOTAX";

            var mainCategory3 = new Health();
            var subCategory3 = new HealthMisc
            {
                Ingredients = new string[] { "Sugar Coffe" },
                Directions = "Mix the ingredients and stir thoroughly!"
            };

            mainCategory3.ProductType = new HealthProductType { Item = subCategory3 };

            p3.ProductData = new ProductProductData
            {
                Item = mainCategory3
            };

            p3.DescriptionData = new ProductDescriptionData
            {
                Brand = "KIGWA PRODUCT BRAND",
                BulletPoint = new string[] { "This is kigwa bullet 1", "This is kigwa bullet 2", "This is kigwa bullet 3", "This is kigwa bullet 4" },
                Description = "This is very fanstastic kigwa product on this era!",
                Title = "KIGWA PRODUCT TITLE",
                MSRP = new CurrencyAmount { currency = BaseCurrencyCode.USD, Value = 1987 },
                Manufacturer = "MUSHI KIGWA INC",
                ItemType = "kigwa-item-type",
                ItemDimensions = new Dimensions
                {
                    Height = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters },
                    Length = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters },
                    Width = new LengthDimension { Value = 12, unitOfMeasure = LengthUnitOfMeasure.millimeters }
                },
                ShippingWeight = new PositiveWeightDimension { unitOfMeasure = WeightUnitOfMeasure.KG, Value = 1.3m },

            };


            //products.Add(p1);
            products.Add(p2);
            //products.Add(p3);

            return products;
        }

        private MarketplaceWebServiceClient GetMwsClient()
        {
            var mwsConfig = new MarketplaceWebServiceConfig();
            mwsConfig.ServiceURL = "https://mws.amazonservices.com";
            mwsConfig.SetUserAgentHeader("Eshopo LLC", "1.0", "C#", null);

            return new MarketplaceWebServiceClient(_awsAccessKey, _secretKey, mwsConfig);
        }

        private  MarketplaceWebServiceClient _getWebServiceClient()
        {
            var feedConfig = new MarketplaceWebServiceConfig
            {
                ServiceURL = "https://mws.amazonservices.com"
            };

            return new MarketplaceWebServiceClient(_awsAccessKey, _secretKey, "EIS", "1.01", feedConfig);
        }
    }
}
