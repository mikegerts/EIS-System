using System;
using System.Collections.Generic;
using System.Linq;
using EIS.SystemJobApp.AmazonService;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Helpers
{
    public class AmazonResponseHelper
    {
        private static string _channelName = "Amazon";

        public static AmazonService.Item GetValidItemResult(Items[] responseItems)
        {
            AmazonService.Item itemResult = null;
            bool isFound = false;
            foreach (var itemResponse in responseItems)
            {
                if (itemResponse == null) continue;

                // set the initial value for the item result
                itemResult = itemResponse.Item[0];

                foreach (var item in itemResponse.Item)
                {
                    if (item.LargeImage != null)
                    {
                        itemResult = item;
                        isFound = true;
                        break;
                    }
                }

                if (isFound) break;
            }

            return itemResult;
        }

        public static ProductAmazon ConstructProductAmazonFromLookupReult(Item item, AmazonInfoFeed infoFeed)
        {
            return createProductAmazon(item, infoFeed.EisSKU);
        }

        private static ProductAmazon createProductAmazon(AmazonService.Item item, string eisSku)
        {
            var eProduct = new ProductAmazon
            {
                EisSKU = eisSku,
                ASIN = item.ASIN,
                Brand = item.ItemAttributes.Brand,
                Color = item.ItemAttributes.Color,
                EAN = item.ItemAttributes.EAN,
                Label = item.ItemAttributes.Label,
                Manufacturer = item.ItemAttributes.Manufacturer,
                Model = item.ItemAttributes.Model,
                NumOfItems = Convert.ToInt32(item.ItemAttributes.NumberOfItems),
                PackageQty = Convert.ToInt32(item.ItemAttributes.PackageQuantity),
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
                            Caption = _channelName,
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
                var weight = new DecimalWithUnit(parseDimensionValue(itemDimensionNode.Weight), parseDimensionUnit(itemDimensionNode.Weight));
                eProduct.ItemDimension = new Dimension(length, width, height, weight);
            }

            // the the product's package dimenstion
            var pkgDimensionNode = item.ItemAttributes.PackageDimensions;
            if (pkgDimensionNode != null)
            {
                var length = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Length), parseDimensionUnit(pkgDimensionNode.Length));
                var width = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Width), parseDimensionUnit(pkgDimensionNode.Width));
                var height = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Height), parseDimensionUnit(pkgDimensionNode.Height));
                var weight = new DecimalWithUnit(parseDimensionValue(pkgDimensionNode.Weight), parseDimensionUnit(pkgDimensionNode.Weight));
                eProduct.PackageDimension = new Dimension(length, width, height, weight);
            }

            // get the condition type and leadtime ship of the prouduct
            if (item.Offers.Offer != null)
            {
                var offerNode = item.Offers.Offer.FirstOrDefault();
                eProduct.Condition = offerNode == null ? string.Empty : offerNode.OfferAttributes.Condition;
            }

            return eProduct;
        }

        private static decimal parseDimensionValue(DecimalWithUnits dimension)
        {
            return dimension == null ? 0 : dimension.Value;
        }

        private static string parseDimensionUnit(DecimalWithUnits dimension)
        {
            return dimension == null ? string.Empty : dimension.Units;
        }
    }
}
