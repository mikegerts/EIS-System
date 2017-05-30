using Xunit;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.eBay;

namespace EIS.Inventory.Test
{
    public class eBayProductProviderTests
    {
        //[TestMethod]
        public void ShouldGetSuggestedCategories()
        {
            var keyword = "Necklaces"; // "Digital Camera";

            // live credentials
            var credential = new eBayCredentialDto
            {
                Mode = "LIVE",
                DeveloperId = "d8e0124a-5a63-4ed9-837e-7007ab23dc97",
                ApplicationId = "MikeGert-EshopoIn-PRD-c99eeec0d-398c2471",
                CertificationId = "PRD-99eeec0d62f3-d4b0-452e-975e-3260",
                UserToken = "AgAAAA**AQAAAA**aAAAAA**+693Vw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYunCpGAog6dj6x9nY+seQ**b1gDAA**AAMAAA**YLcBqF5Immyn9wi6om4+5CzSH1cTG1FaiVyYEN7Zj6++w6ccm8gvrnw06sUcHeGDPoYXaut5r5O1hVj3/uX+qbuibMSaO2Qd+K3rZkCeHy/cNRwsaOwx7ierR9bE5l6dITf/RS5VfFIZ9b4f4ARXmzycnuCuIveceOwPInSJOuFhALyCT6gkrgZ5N1iFDNN8KqaiIIusDNAUAmOfdHByCqeEv8E44ktkJGgmrvYSCMaLnjQYn5VYVFLeOQmhtQVwZWOUxnnUTOEvDUdBRiuHTrqVIUy7CP0LGKpuWeqay2xLSYLcgvcKTeZykHJErimP/suhOMHB9PlQAc+OIeVzsp95cusuMJNXOqb3XwPKzJHMa3zUiLdr17lIkkTe83uEXVhHv0DG5tWIp1+1JWQxCTbRInoEFN6wat+kijS2nwWjPDDPylI8ToYefc2jVdhEwkUwPFv0OXPmp4QQoNMh437f+1xwKi6p6KEP+Q21pr4krDsE0XLh36aHNmg0WTj3Wf1ehMTmeeLoExK+YG2TnPKBWIzXsd67Q2e1ZONe/efQZaUMbTy3UR2uqTPcZsWEpiShiCOppWGc3zCX7C/XgmMjgSWTMejhLyh8Y2eqf+xkXFfj3iQGuKr50kAp0TzpzBZG1LSJuLNBI+gxfXwteT8yX5NCHXX/MCWSIobL+Kb7Cq0r2x63oumUJkkCi2hXK5rckLbar+ukpR/1hpAxlOUckgs87qOimwN+p7z27LelPoohzTfJR3RJ9Ccq+fjg"
            };

            var provider = new eBayProductProvider();
            provider.MarketplaceCredential = credential;
            provider.GetSuggestedCategories(keyword);
        }

        //[Fact]
        public void Get_eBay_UserPreference()
        {
            // arrange
            var credential = new eBayCredentialDto
            {
                Mode = "LIVE",
                DeveloperId = "d8e0124a-5a63-4ed9-837e-7007ab23dc97",
                ApplicationId = "MikeGert-EshopoIn-PRD-c99eeec0d-398c2471",
                CertificationId = "PRD-99eeec0d62f3-d4b0-452e-975e-3260",
                UserToken = "AgAAAA**AQAAAA**aAAAAA**+693Vw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYunCpGAog6dj6x9nY+seQ**b1gDAA**AAMAAA**YLcBqF5Immyn9wi6om4+5CzSH1cTG1FaiVyYEN7Zj6++w6ccm8gvrnw06sUcHeGDPoYXaut5r5O1hVj3/uX+qbuibMSaO2Qd+K3rZkCeHy/cNRwsaOwx7ierR9bE5l6dITf/RS5VfFIZ9b4f4ARXmzycnuCuIveceOwPInSJOuFhALyCT6gkrgZ5N1iFDNN8KqaiIIusDNAUAmOfdHByCqeEv8E44ktkJGgmrvYSCMaLnjQYn5VYVFLeOQmhtQVwZWOUxnnUTOEvDUdBRiuHTrqVIUy7CP0LGKpuWeqay2xLSYLcgvcKTeZykHJErimP/suhOMHB9PlQAc+OIeVzsp95cusuMJNXOqb3XwPKzJHMa3zUiLdr17lIkkTe83uEXVhHv0DG5tWIp1+1JWQxCTbRInoEFN6wat+kijS2nwWjPDDPylI8ToYefc2jVdhEwkUwPFv0OXPmp4QQoNMh437f+1xwKi6p6KEP+Q21pr4krDsE0XLh36aHNmg0WTj3Wf1ehMTmeeLoExK+YG2TnPKBWIzXsd67Q2e1ZONe/efQZaUMbTy3UR2uqTPcZsWEpiShiCOppWGc3zCX7C/XgmMjgSWTMejhLyh8Y2eqf+xkXFfj3iQGuKr50kAp0TzpzBZG1LSJuLNBI+gxfXwteT8yX5NCHXX/MCWSIobL+Kb7Cq0r2x63oumUJkkCi2hXK5rckLbar+ukpR/1hpAxlOUckgs87qOimwN+p7z27LelPoohzTfJR3RJ9Ccq+fjg"
            };
            var provider = new eBayProductProvider(new LogService());
            provider.MarketplaceCredential = credential;

            // act 
            var isSuccess = provider.GetMarketplaceUserPreference();

            // assert
            Assert.True(isSuccess);
        }

        //[Fact]
        public void Set_eBay_UserPreference_OutOfStock_To_True()
        {
            // arrange
            var userPreference = new UserPreferenceFeed
            {
                OutOfStockControlPreference = false,
                isOutOfStockControlPreference = true
            };
            var credential = new eBayCredentialDto
            {
                Mode = "LIVE",
                DeveloperId = "d8e0124a-5a63-4ed9-837e-7007ab23dc97",
                ApplicationId = "MikeGert-EshopoIn-PRD-c99eeec0d-398c2471",
                CertificationId = "PRD-99eeec0d62f3-d4b0-452e-975e-3260",
                UserToken = "AgAAAA**AQAAAA**aAAAAA**+693Vw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYunCpGAog6dj6x9nY+seQ**b1gDAA**AAMAAA**YLcBqF5Immyn9wi6om4+5CzSH1cTG1FaiVyYEN7Zj6++w6ccm8gvrnw06sUcHeGDPoYXaut5r5O1hVj3/uX+qbuibMSaO2Qd+K3rZkCeHy/cNRwsaOwx7ierR9bE5l6dITf/RS5VfFIZ9b4f4ARXmzycnuCuIveceOwPInSJOuFhALyCT6gkrgZ5N1iFDNN8KqaiIIusDNAUAmOfdHByCqeEv8E44ktkJGgmrvYSCMaLnjQYn5VYVFLeOQmhtQVwZWOUxnnUTOEvDUdBRiuHTrqVIUy7CP0LGKpuWeqay2xLSYLcgvcKTeZykHJErimP/suhOMHB9PlQAc+OIeVzsp95cusuMJNXOqb3XwPKzJHMa3zUiLdr17lIkkTe83uEXVhHv0DG5tWIp1+1JWQxCTbRInoEFN6wat+kijS2nwWjPDDPylI8ToYefc2jVdhEwkUwPFv0OXPmp4QQoNMh437f+1xwKi6p6KEP+Q21pr4krDsE0XLh36aHNmg0WTj3Wf1ehMTmeeLoExK+YG2TnPKBWIzXsd67Q2e1ZONe/efQZaUMbTy3UR2uqTPcZsWEpiShiCOppWGc3zCX7C/XgmMjgSWTMejhLyh8Y2eqf+xkXFfj3iQGuKr50kAp0TzpzBZG1LSJuLNBI+gxfXwteT8yX5NCHXX/MCWSIobL+Kb7Cq0r2x63oumUJkkCi2hXK5rckLbar+ukpR/1hpAxlOUckgs87qOimwN+p7z27LelPoohzTfJR3RJ9Ccq+fjg"
            };
            var provider = new eBayProductProvider(new LogService());
            provider.MarketplaceCredential = credential;

            // act 
            var isSuccess = provider.SetMarketplaceUserPreference(userPreference);

            // assert
            Assert.True(isSuccess);
        }
    }
}
