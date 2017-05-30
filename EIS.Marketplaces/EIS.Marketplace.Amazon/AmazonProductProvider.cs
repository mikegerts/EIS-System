using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using EIS.Inventory.Core;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Marketplace.Amazon.AmazonService;
using EIS.Marketplace.Amazon.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Marketplace.Amazon
{
    [Export(typeof(IMarketplaceProductProvider))]
    public class AmazonProductProvider : IMarketplaceProductProvider
    {
        private readonly ILogService _logger;

        public AmazonProductProvider()
        {
            _logger = Core.Get<ILogService>();
        }

        public string ChannelName
        {
            get { return "Amazon"; }
        }

        private AmazonCredentialDto _credential;
        public CredentialDto MarketplaceCredential
        {
            set { _credential = value as AmazonCredentialDto; }
        }

        public MarketplaceProduct GetProductInfo(AmazonInfoFeed infoFeed)
        {
            // create a WCF Amazon ECS client
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.MaxReceivedMessageSize = int.MaxValue;
            var client = new AWSECommerceServicePortTypeClient(
                binding,
                new EndpointAddress("https://webservices.amazon.com/onca/soap?Service=AWSECommerceService"));
            var requests = new List<ItemLookupRequest>();

            // add authentication to the ECS client
            client.ChannelFactory.Endpoint.Behaviors
                .Add(new AmazonSigningEndpointBehavior(_credential.SearchAccessKeyId, _credential.SearchSecretKey));

            // create request item type for asin - WE ONLY ALLOW ASIN ONLY
            if (!string.IsNullOrEmpty(infoFeed.ASIN))
                requests.Add(createItemRequestLookup(infoFeed.ASIN, ItemLookupRequestIdType.ASIN));
            
            // return null if there is no request available
            if (!requests.Any())
                return null;
            
            // create an ItemSearch wrapper
            var itemLookUp = new ItemLookup();
            itemLookUp.AssociateTag = _credential.AssociateId;
            itemLookUp.AWSAccessKeyId = _credential.SearchAccessKeyId;
            itemLookUp.Request = requests.ToArray();

            // send the ItemLookup request
            var response = client.ItemLookup(itemLookUp);
            if (response == null || response.Items == null)
            {
                _logger.LogError(LogEntryType.AmazonProduct, string.Format("Item Lookup for {0} failed! Error Message: {1}", infoFeed.EisSKU, response.OperationRequest.Errors[0].Message), string.Empty);
                return null;
            }

            AmazonService.Item itemResult = null;
            var errorMessages = new List<string>();
            for (var i = 0; i < requests.Count; i++)
            {
                if (response.Items[i].Request.Errors != null)
                    errorMessages.Add(response.Items[i].Request.Errors[0].Message);

                if (response.Items[i].Item != null)
                {
                    // set the initial value for the item result
                    itemResult = response.Items[i].Item[0];

                    foreach (var item in response.Items[i].Item)
                    {
                        // let's find from the result which has large image available
                        if(item.LargeImage != null)
                        {
                            itemResult = item;
                            break;
                        }
                    }
                }
            }

            // exit if there is no really result
            if (itemResult == null)
            {
                _logger.LogError(LogEntryType.AmazonProduct, string.Join("\n", errorMessages), string.Empty);
                return null;
            }

            var product = parsedToMarketplaceProduct(itemResult);
            product.EisSKU = infoFeed.EisSKU;

            // log info
            _logger.LogInfo(LogEntryType.AmazonProduct, string.Format("Get product info for \'{0}\' was successful!", infoFeed.ASIN));

            return product;
        }

        private ItemLookupRequest createItemRequestLookup(string itemId, ItemLookupRequestIdType requestIdType)
        {
            // prepare the ItemLookup request
            var request =  new ItemLookupRequest
            {
                Condition = Condition.All,
                ConditionSpecified = true,
                IdType = requestIdType,
                IdTypeSpecified = true,
                ItemId = new string[] { itemId },
                ResponseGroup = new string[] { "Images", "ItemAttributes", "Offers" },
            };

            // don't add search index if request type is ASIN
            if (requestIdType != ItemLookupRequestIdType.ASIN)
                request.SearchIndex = "All";

            return request;
        }

        public List<MarketplaceCategoryDto> GetSuggestedCategories(string keyword)
        {
            throw new NotImplementedException();
        }

        private MarketplaceProduct parsedToMarketplaceProduct(AmazonService.Item item)
        {
            try
            {
                var eProduct = new ProductAmazon
                {
                    ASIN = item.ASIN,
                    Brand = item.ItemAttributes.Brand,
                    Color = item.ItemAttributes.Color,
                    EAN = item.ItemAttributes.EAN,
                    Label = item.ItemAttributes.Label,
                    Manufacturer = item.ItemAttributes.Manufacturer,
                    Model = item.ItemAttributes.Model,
                    NumOfItems = Convert.ToInt16(item.ItemAttributes.NumberOfItems),
                    PackageQty = Convert.ToInt16(item.ItemAttributes.PackageQuantity),                    
                    ProductGroup = item.ItemAttributes.ProductGroup,
                    ProductTypeName = item.ItemAttributes.ProductTypeName,
                    Size = item.ItemAttributes.Size,
                    ProductTitle = item.ItemAttributes.Title,
                    MapPrice = item.ItemAttributes.ListPrice == null ? 0 : (Convert.ToDecimal(item.ItemAttributes.ListPrice.Amount) / 100),
                };
                                
                // set the product images
                if (item.ImageSets != null)
                {
                    var primaryImage = item.ImageSets.FirstOrDefault(x => x.Category == "primary");
                    if (primaryImage != null)
                    {
                        var images = new List<MediaContent>()
                    {
                        new MediaContent {
                            Url = primaryImage.LargeImage.URL,
                            Type = "LARGE",
                            Caption = ChannelName,
                            Id = 1
                        }
                    };

                        eProduct.Images = images;
                    }
                }
                
                // set the product' item dimension
                var itemDimensionNode = item.ItemAttributes.ItemDimensions;
                if (itemDimensionNode != null)
                {
                    var length = new DecimalWithUnit(parseDimensionValue(itemDimensionNode.Length), parseDimensionUnit(itemDimensionNode.Length));
                    var width = new DecimalWithUnit(parseDimensionValue(itemDimensionNode.Width), parseDimensionUnit(itemDimensionNode.Width));
                    var height = new DecimalWithUnit(parseDimensionValue(itemDimensionNode.Height), parseDimensionUnit(itemDimensionNode.Height));
                    var itemWeight = new DecimalWithUnit(parseDimensionValue(itemDimensionNode.Weight), parseDimensionUnit(itemDimensionNode.Weight));
                    //eProduct.WeightBox1 = itemWeight.Value;
                    //eProduct.WeightBox1Unit = itemWeight.Unit;
                    eProduct.ItemDimension = new Dimension(length, width, height, itemWeight);
                }

                // the the product's package dimenstion
                var pkgDimensionNode = item.ItemAttributes.PackageDimensions;
                if (pkgDimensionNode != null)
                {
                    var length = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Length), parseDimensionUnit(pkgDimensionNode.Length));
                    var width = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Width), parseDimensionUnit(pkgDimensionNode.Width));
                    var height = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Height), parseDimensionUnit(pkgDimensionNode.Height));
                    var pkgWeight = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Weight), parseDimensionUnit(pkgDimensionNode.Weight));
                    //eProduct.WeightBox2 = pkgWeight.Value;
                    //eProduct.WeightBox2Unit = pkgWeight.Unit;
                    eProduct.PackageDimension = new Dimension(length, width, height, pkgWeight);
                }

                // get the condition type and leadtime ship of the prouduct
                if (item.Offers.Offer != null)
                {
                    var offerNode = item.Offers.Offer.FirstOrDefault();
                    eProduct.Condition = offerNode == null ? string.Empty : offerNode.OfferAttributes.Condition;
                }

                return eProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonProduct, "Error in parsing the product item -> " + EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return null;
            }
        }

        private decimal parseDimensionValue(DecimalWithUnits dimension)
        {
            return dimension == null ? 0 : dimension.Value;
        }

        private string parseDimensionUnit(DecimalWithUnits dimension)
        {
            return dimension == null ? string.Empty : dimension.Units;
        }        
    }
}
