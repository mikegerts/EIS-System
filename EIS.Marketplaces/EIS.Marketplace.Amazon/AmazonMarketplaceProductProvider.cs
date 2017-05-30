using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Marketplace.Amazon.Helpers;
using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Model;

namespace EIS.Marketplace.Amazon
{    
    public class AmazonMarketplaceProductProvider : IMarketplaceProductProvider
    {
        /// <summary>
        /// The local instance of the Amazon service client
        /// </summary>
        private MarketplaceWebServiceProductsClient _amazonClient;
        private ILogService _logger;

        public AmazonMarketplaceProductProvider()
        {
            _logger = Core.Get<ILogService>();
        }

        public string ChannelName
        {
            get { return "Amazon"; }
        }

        private MarketplaceSettingViewModel _marketplaceCredential;
        public IMarketplaceCredential MarketplaceCredential
        {
            get { return _marketplaceCredential; }
            set
            {
                _marketplaceCredential = value as MarketplaceSettingViewModel;
                RequestHelper.SetCredentials(_marketplaceCredential);

                // create configuratin to use US marketplace
                var config = new MarketplaceWebServiceProductsConfig { ServiceURL = RequestHelper.ServiceUrl };
                config.SetUserAgentHeader("EIS Inventory System", "3.0", "C#");
                _amazonClient = new MarketplaceWebServiceProductsClient(_marketplaceCredential.AccessKeyId, _marketplaceCredential.SecretKey, config);
            }
        }

        public MarketplaceProduct GetProductInfo(string asin)
        {
            var request = new GetMatchingProductForIdRequest
            {
                SellerId = _marketplaceCredential.MerchantId,
                MarketplaceId = _marketplaceCredential.MarketplaceIds.First(),
                IdType = "ASIN",
                IdList = new IdListType { Id = { asin } }
            };

            // log info
            _logger.LogInfo(LogEntryType.AmazonProduct, string.Format("GetMatchingProductForIdRequest for \'{0}\' was unsuccessful!", asin));

            var response = _amazonClient.GetMatchingProductForId(request);
            var product = parsedToMarketplaceProduct(response);
            if (product == null)
            {
                _logger.LogWarning(LogEntryType.AmazonProduct, string.Format("GetMatchingProductForIdRequest: Product ASIN \'{0}\' was not found!", asin));
                return null;
            }

            // set other market place produt info 
            //setMarketplaceProductPrice(product);
            //setMarketplaceProductOffer(product);

            return product;
        }

        public List<MarketplaceProduct> GetProductsInfo(List<string> asins)
        {
            var request = new GetMatchingProductForIdRequest
            {
                SellerId = _marketplaceCredential.MerchantId,
                MarketplaceId = _marketplaceCredential.MarketplaceIds.First(),
                IdType = "ASIN",
                IdList = new IdListType { Id = asins }
            };

            // log info
            _logger.LogInfo(LogEntryType.AmazonProduct, string.Format("GetMatchingProductForIdRequest for \'{0}\' was unsuccessful!", asins));

            var response = _amazonClient.GetMatchingProductForId(request);
            var product = parsedToMarketplaceProduct(response);
            if (product == null)
            {
                _logger.LogWarning(LogEntryType.AmazonProduct, string.Format("GetMatchingProductForIdRequest: Product ASIN \'{0}\' was not found!", asins));
                return null;
            }

            // set other market place produt info 
            //setMarketplaceProductPrice(product);
            //setMarketplaceProductOffer(product);

            return null;
        }

        private void setMarketplaceProductOffer(ProductAmazon product)
        {
            var request = new GetLowestOfferListingsForSKURequest
            {
                SellerId = _marketplaceCredential.MerchantId,
                MarketplaceId = _marketplaceCredential.MarketplaceIds.First(),
                SellerSKUList = new SellerSKUListType { SellerSKU = { product.EisSKU } }
            };

            var response = _amazonClient.GetLowestOfferListingsForSKU(request);
            if (response.GetLowestOfferListingsForSKUResult.First().status != "Success")
            {
                _logger.LogWarning(LogEntryType.AmazonProduct, string.Format("GetLowestOfferListingsForSKURequest for \'{0}\' was unsuccessful!",
                    product.EisSKU));
                return;
            }

            var offerResult = response.GetLowestOfferListingsForSKUResult.First().Product;

            // set eisProduct's offers details
            //product.LeadtimeShip = offerResult.LowestOfferListings.LowestOfferListing.First().Qualifiers.ShippingTime.Max;
        }

        private void setMarketplaceProductPrice(ProductAmazon product)
        {
            var request = new GetMyPriceForSKURequest
            {
                SellerId = _marketplaceCredential.MerchantId,
                MarketplaceId = _marketplaceCredential.MarketplaceIds.First(),
                SellerSKUList = new SellerSKUListType { SellerSKU = { product.EisSKU } }
            };

            var response = _amazonClient.GetMyPriceForSKU(request);
            if (response.GetMyPriceForSKUResult.First().status != "Success")
            {
                _logger.LogWarning(LogEntryType.AmazonProduct, string.Format("GetMyPriceForSKURequest for \'{0}\' was unsuccessful!",
                    product.EisSKU));
                return;
            }
            
            // set the eisProduct's details
            var productOffer = response.GetMyPriceForSKUResult.First().Product.Offers.Offer.FirstOrDefault();
            product.FulfilledBy = productOffer.FulfillmentChannel;
            product.Condition = productOffer.ItemCondition;
            product.ConditionNote = productOffer.ItemSubCondition;
            product.MapPrice = productOffer.BuyingPrice.LandedPrice.Amount;           
        }

        private ProductAmazon parsedToMarketplaceProduct(GetMatchingProductForIdResponse response)
        {
            var xmlDoc = new XmlDocument();
            var matchProductResult = response.GetMatchingProductForIdResult.First();
            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("ns", "http://mws.amazonservices.com/schema/Products/2011-10-01");
            nsmgr.AddNamespace("ns2", "http://mws.amazonservices.com/schema/Products/2011-10-01/default.xsd");

            if (matchProductResult.status != "Success")
            {
                _logger.LogWarning(LogEntryType.AmazonProduct, string.Format("GetMatchingProductForIdResponse was unsuccessful! - Id Type: {0} - {1}",
                    matchProductResult.IdType, matchProductResult.Id));
                return null;
            }

            xmlDoc.LoadXml(matchProductResult.ToXML());
            var rowNode = xmlDoc.ChildNodes[0];
            var productAttrNode = rowNode.SelectSingleNode("//ns:Products/ns:Product/ns:AttributeSets/ns2:ItemAttributes", nsmgr);
            var featureNodes = rowNode.SelectNodes("//ns:Products/ns:Product/ns:AttributeSets/ns2:ItemAttributes/ns2:Feature", nsmgr);
            var smallImageNodes = rowNode.SelectNodes("//ns:Products/ns:Product/ns:AttributeSets/ns2:ItemAttributes/ns2:SmallImage", nsmgr);
                        
            var binding = productAttrNode["ns2:Binding"] == null ? "0" : productAttrNode["ns2:Binding"].InnerText;
            var brand = productAttrNode["ns2:Brand"] == null ? "0" : productAttrNode["ns2:Brand"].InnerText;
            var color = productAttrNode["ns2:Color"] == null ? string.Empty : productAttrNode["ns2:Color"].InnerText;
            var department = productAttrNode["ns2:Department"] == null ? "0" : productAttrNode["ns2:Department"].InnerText;
            var gemType = productAttrNode["ns2:GemType"] == null ? "0" : productAttrNode["ns2:GemType"].InnerText;
            var label = productAttrNode["ns2:Label"] == null ? "0" : productAttrNode["ns2:Label"].InnerText;
            var manufacturer = productAttrNode["ns2:Manufacturer"] == null ? string.Empty : productAttrNode["ns2:Manufacturer"].InnerText;
            var metypeType = productAttrNode["ns2:MetalType"] == null ? "0" : productAttrNode["ns2:MetalType"].InnerText;
            var model = productAttrNode["ns2:Model"] == null ? string.Empty : productAttrNode["ns2:Model"].InnerText;
            var numOfItems = productAttrNode["ns2:NumberOfItems"] == null ? "0" : productAttrNode["ns2:NumberOfItems"].InnerText;
            var packageQty = productAttrNode["ns2:PackageQuantity"] == null ? "0" : productAttrNode["ns2:PackageQuantity"].InnerText;
            var partNumber = productAttrNode["ns2:PartNumber"] == null ? string.Empty : productAttrNode["ns2:PartNumber"].InnerText;
            var productGroup = productAttrNode["ns2:ProductGroup"] == null ? string.Empty : productAttrNode["ns2:ProductGroup"].InnerText;
            var productTypeName = productAttrNode["ns2:ProductTypeName"] == null ? string.Empty : productAttrNode["ns2:ProductTypeName"].InnerText;
            var publisher = productAttrNode["ns2:Publisher"] == null ? string.Empty : productAttrNode["ns2:Publisher"].InnerText;
            var title = productAttrNode["ns2:Title"] == null ? string.Empty : productAttrNode["ns2:Title"].InnerText;

            // get the product's features
            var features = new List<string>();
            foreach (XmlNode node in featureNodes)
                features.Add(node.InnerText);

            // parse the product's images
            var smallImageUrls = new List<MediaContent>();
            foreach(XmlNode node in smallImageNodes)
            {
                var imageNode = node["ns2:URL"];
                smallImageUrls.Add(new MediaContent
                {
                    Url = imageNode == null ? string.Empty : imageNode.InnerText,
                    Caption = ChannelName,
                });
            }

            var eProduct = new ProductAmazon
            {
                EisSKU = matchProductResult.Id,
                ProductTitle = title,
                NumOfItems = Convert.ToInt16(numOfItems),
                PackageQty = Convert.ToInt16(packageQty),
                ASIN = matchProductResult.Products.Product.First().Identifiers.MarketplaceASIN.ASIN,
                Brand = brand,
                Manufacturer = manufacturer,
                Model = model,
                ProductGroup = productGroup,
                ProductTypeName = productTypeName,
                Features = features,
                Images = smallImageUrls,
            };

            // get the list price for MAP price
            var listPriceNode = productAttrNode["ns2:ListPrice"];
            if (listPriceNode != null)
            {
                var amountNode = listPriceNode["ns2:Height"];
                eProduct.MapPrice = amountNode == null ? 0 : Convert.ToDecimal(amountNode.InnerText);
            }

            // get the eisProduct dimension
            var itemDimensionNode = productAttrNode["ns2:ItemDimensions"];
            if (itemDimensionNode != null)
            {
                var heightNode = itemDimensionNode["ns2:Height"];
                var height = new Measurement();
                if (heightNode != null)
                {
                    height.Value = Convert.ToDecimal(heightNode.InnerText);
                    height.Unit = heightNode.Attributes["Units"].Value;
                }

                var lengthNode = itemDimensionNode["ns2:Length"];
                var length = new Measurement();
                if (lengthNode != null)
                {
                    length.Value = Convert.ToDecimal(lengthNode.InnerText);
                    length.Unit = lengthNode.Attributes["Units"].Value;
                }

                var widthhNode = itemDimensionNode["ns2:Width"];
                var width = new Measurement();
                if (widthhNode != null)
                {
                    width.Value = Convert.ToDecimal(widthhNode.InnerText);
                    width.Unit = widthhNode.Attributes["Units"].Value;
                }

                var weightNode = itemDimensionNode["ns2:Weight"];
                var weight = new Measurement();
                if (weightNode != null)
                {
                    weight.Value = Convert.ToDecimal(weightNode.InnerText);
                    weight.Unit = weightNode.Attributes["Units"].Value;
                }

                eProduct.ItemDimension = new Dimension(length, width, height, weight);
            }

            // get the eisProduct's package dimension
            var packageDimensionNode = productAttrNode["ns2:PackageDimensions"];
            if (packageDimensionNode != null)
            {
                var heightNode = packageDimensionNode["ns2:Height"];
                var height = new Measurement();
                if (heightNode != null)
                {
                    height.Value = Convert.ToDecimal(heightNode.InnerText);
                    height.Unit = heightNode.Attributes["Units"].Value;
                }

                var lengthNode = packageDimensionNode["ns2:Length"];
                var length = new Measurement();
                if (lengthNode != null)
                {
                    length.Value = Convert.ToDecimal(lengthNode.InnerText);
                    length.Unit = lengthNode.Attributes["Units"].Value;
                }

                var widthhNode = packageDimensionNode["ns2:Width"];
                var width = new Measurement();
                if (widthhNode != null)
                {
                    width.Value = Convert.ToDecimal(widthhNode.InnerText);
                    width.Unit = widthhNode.Attributes["Units"].Value;
                }

                var weightNode = packageDimensionNode["ns2:Weight"];
                var weight = new Measurement();
                if (weightNode != null)
                {
                    weight.Value = Convert.ToDecimal(weightNode.InnerText);
                    weight.Unit = weightNode.Attributes["Units"].Value;
                }

                eProduct.PackageDimension = new Dimension(length, width, height, weight);
            }

            return eProduct;
        }
    }
}
