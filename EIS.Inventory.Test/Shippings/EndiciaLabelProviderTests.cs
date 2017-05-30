using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.Endicia;
using Xunit;

namespace EIS.Inventory.Test.Shippings
{
    public class EndiciaLabelProviderTests
    {
        [Fact]
        public void Should_Change_PassPhrase()
        {
            // arrange
            var newPassPhrase = "Denden.Mushi";
            var logger = new LogService();
            var provider = new EndiciaLabelProvider(logger);

            // act
            var response = provider.ChangePassPhrase(newPassPhrase);

            // assert
            Assert.NotNull(response);
        }

        [Fact]
        public void Should_Get_Postage_Label()
        {
            // arrange
            var dimension = new Dimension(4, 2, 1, 2m);
            var packageDetail = new PackageDetail
            {
                RequestedBy = "unit_test_user",
                EisOrderId = 123,
                OrderId = "10000001",
                OrderItemId = "10000001-XXX-123",
                MailClass = MailClass.PriorityExpress,
                FromName = "CRIS DUANE GENITA",
                //FromCompany = "ALL ISLAND PACKAGE",
                FromAddress = new Address
                {
                    Line1 = "BUENAVISTA UABY BOHOL",
                    Line2 = "MANLAYAG DANAO CITY",
                    City = "TAGBILARAN CITY",
                    StateOrRegion = "NY",
                    CountryCode = "US",
                    PostalCode = "10001"
                },
                FromPhone = "5045838417",
                FromEmail = "egdayondon@gmail.com",
                ToName = "STATEFARM GISELE PLESSY",
                ToCompany = "TAGA COMPANY ABC",
                ToAddress = new Address
                {
                    Line1 = "6325 ELYSIAN FIELDS AVE STE A",
                    City = "NEW ORLEANS",
                    StateOrRegion = "LA",
                    CountryCode = "US",
                    PostalCode = "55435",
                },
                ToPhone = "504-583-8417",
                ToEmail = "egenita@marketplace.amazon.com",
                ItemDimension = dimension
            };
            var logger = new LogService();
            var provider = new EndiciaLabelProvider(logger);

            // act
            var response = provider.GetPostageLabel(packageDetail);

            // assert
            Assert.NotNull(response);
        }
    }
}
