using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.eBay;
using System.Collections.Generic;
using System.Configuration;
using Xunit;

namespace EIS.Inventory.Test
{
    public class UnitTest_eBayMarketplaceInventoryProvider
    {
        [Fact]
        public void Should_SubmitSingleProductListingFeed()
        {
            // arrange
            var provider = new eBayMarketplaceInventoryProvider(new LogService());
            var products = new List<MarketplaceProductFeedDto>();
            var credential = new eBayCredentialDto
            {
                Mode = "LIVE",
                DeveloperId = "d8e0124a-5a63-4ed9-837e-7007ab23dc97",
                ApplicationId = "MikeGert-EshopoIn-PRD-c99eeec0d-398c2471",
                CertificationId = "PRD-99eeec0d62f3-d4b0-452e-975e-3260",
                UserToken = "AgAAAA**AQAAAA**aAAAAA**+693Vw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYunCpGAog6dj6x9nY+seQ**b1gDAA**AAMAAA**YLcBqF5Immyn9wi6om4+5CzSH1cTG1FaiVyYEN7Zj6++w6ccm8gvrnw06sUcHeGDPoYXaut5r5O1hVj3/uX+qbuibMSaO2Qd+K3rZkCeHy/cNRwsaOwx7ierR9bE5l6dITf/RS5VfFIZ9b4f4ARXmzycnuCuIveceOwPInSJOuFhALyCT6gkrgZ5N1iFDNN8KqaiIIusDNAUAmOfdHByCqeEv8E44ktkJGgmrvYSCMaLnjQYn5VYVFLeOQmhtQVwZWOUxnnUTOEvDUdBRiuHTrqVIUy7CP0LGKpuWeqay2xLSYLcgvcKTeZykHJErimP/suhOMHB9PlQAc+OIeVzsp95cusuMJNXOqb3XwPKzJHMa3zUiLdr17lIkkTe83uEXVhHv0DG5tWIp1+1JWQxCTbRInoEFN6wat+kijS2nwWjPDDPylI8ToYefc2jVdhEwkUwPFv0OXPmp4QQoNMh437f+1xwKi6p6KEP+Q21pr4krDsE0XLh36aHNmg0WTj3Wf1ehMTmeeLoExK+YG2TnPKBWIzXsd67Q2e1ZONe/efQZaUMbTy3UR2uqTPcZsWEpiShiCOppWGc3zCX7C/XgmMjgSWTMejhLyh8Y2eqf+xkXFfj3iQGuKr50kAp0TzpzBZG1LSJuLNBI+gxfXwteT8yX5NCHXX/MCWSIobL+Kb7Cq0r2x63oumUJkkCi2hXK5rckLbar+ukpR/1hpAxlOUckgs87qOimwN+p7z27LelPoohzTfJR3RJ9Ccq+fjg"
            };
            var product = new MarketplaceProductFeedDto
            {
                UPC = "784034224341",
                EisSKU = "CJ10800000000891",
                Name = "K-Jewelry11® Women's Fashion Lime & Emerald Mixed Stone Silver Stretch Bracelet Gift For Her",
                Description = "K-Jewelry11® Women's Fashion Lime & Emerald Mixed Stone Silver Stretch",
                
                eBayProductFeed = new eBayProductFeed
                {
                    Title = "Jewelry11® Women's Fashion Lime ",
                    SubTitle = "Jewelry11® Women's Fashion Lime",
                    CategoryId = 75040,
                    Duration = "Days_5",
                    Location = "11221",
                    ListType = "FixedPriceItem"
                },
                ImageUrls = new List<string>
                {
                    "http://www.costumejewelry1.com/media/catalog/product/cache/1/small_image/125x/9df78eab33525d08d6e5fb8d27136e95/0/8/0805470051992.jpg"
                }
            };
            products.Add(product);

            // act
            provider.Credential = credential;
            provider.SubmitSingleProductListingFeed(product, "KigwaTest");

            // assert
        }

        //[TestMethod]
        public void ShouldAddItemContainer()
        {
            var logger = new LogService();

            var fee = new Fee
            {
                ActionFeedType = "ItemPosted",
                Amount = 3,
                CurrencyCode = "D",
                Name= "Test Fee Name"
            };

            var itemContainer = new ItemContainer
            {
                EisSKU = "CJ10800000000440",
                ItemId = "31312312",
                Fees = new List<Fee> { fee }
            };

            logger.AddProductItemFees(itemContainer);
        }
    }
}
