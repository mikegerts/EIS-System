using EIS.Shipping.FedEx;
using EIS.Shipping.FedEx.ShipServiceWebReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EIS.Inventory.Test.Shippings
{
    public class FedExServiceTests_v1
    {
        public class FedExServiceTests
        {

            private FedExShippingProvider_v1 _service;
            public FedExServiceTests()
            {
                _service = new FedExShippingProvider_v1();
            }

            [Fact]
            public void ShouldProcessShipmentRateRequest()
            {
                // Arrange
                _service.SetWebAuthenticationDetail(GetWebAuthenticationDetail());
                _service.SetClientDetail(GetClientDetail());
                _service.SetVersion(new VersionId());

                _service.SetTransactionDetail(GetTransactionDetail());
                _service.SetRequestedShipment(GetRequestedShipment());

                // Act
                var rate = _service.GetShipmentRate();

                // Assert
                Assert.NotNull(rate);
                Assert.True(rate.Count > 0);
            }

            private RequestedShipment GetRequestedShipment()
            {
                return  new RequestedShipment
                {
                    ShipTimestamp = DateTime.Now,
                    DropoffType = DropoffType.REGULAR_PICKUP,
                    ServiceType = ServiceType.PRIORITY_OVERNIGHT,
                    PackagingType = PackagingType.YOUR_PACKAGING,
                    TotalWeight = new Weight { Value = 50.0M, Units = WeightUnits.LB },
                    PackageCount = "1",
                    Shipper = GetShipper(),
                    Recipient = GetRecipient(),
                    ShippingChargesPayment = GetShippingChargesPayment(),
                    LabelSpecification = GetLabelSpecification(),
                    RequestedPackageLineItems = GetRequestedPackageLineItems(),
                    SpecialServicesRequested = GetShipmentSpecialServicesRequested(true)
                };
            }

            private WebAuthenticationDetail GetWebAuthenticationDetail()
            {
                return new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential { Key = "ihVYTuZYiLziO0hH", Password = "FUdmnvgQK56GaqQoGg0cTVCVY" },
                    ParentCredential = new WebAuthenticationCredential { Key = "ihVYTuZYiLziO0hH", Password = "FUdmnvgQK56GaqQoGg0cTVCVY" }
                };
            }
            private static ClientDetail GetClientDetail()
            {
                return new ClientDetail
                {
                    AccountNumber = "510087984",
                    MeterNumber = "118766159"
                };
            }
            private TransactionDetail GetTransactionDetail()
            {
                return new TransactionDetail
                {
                    CustomerTransactionId = "***Express International Shipment Request using VC#***" // The client will get the same value back in the response
                };
            }

            private static Party GetShipper()
            {
                return new Party
                {
                    Contact = new Contact { PersonName = "Sender Name", CompanyName = "Sender Company Name", PhoneNumber = "0805522713" },
                    Address = new Address { StreetLines = new string[1] { "Address Line 1" }, City = "Austin", StateOrProvinceCode = "TX", PostalCode = "73301", CountryCode = "US" }
                };
            }
            private static Party GetRecipient()
            {
                return new Party
                {
                    Contact = new Contact { PersonName = "Recipient Name", CompanyName = "Recipient Company Name", PhoneNumber = "9012637906" },
                    Address = new Address { StreetLines = new string[1] { "Address Line 1" }, City = "Windsor", StateOrProvinceCode = "CT", PostalCode = "06006", CountryCode = "US", Residential = true }
                };
            }
            private static Payment GetShippingChargesPayment()
            {
                return new Payment
                {
                    PaymentType = PaymentType.SENDER,
                    Payor = new Payor
                    {
                        ResponsibleParty = new Party { AccountNumber = "510087984", Contact = new Contact(), Address = new Address { CountryCode = "US" } }
                    }
                };
            }
            private static LabelSpecification GetLabelSpecification()
            {
                return new LabelSpecification
                {
                    ImageType = ShippingDocumentImageType.PDF,
                    ImageTypeSpecified = true,
                    LabelFormatType = LabelFormatType.COMMON2D,
                    LabelStockType = LabelStockType.PAPER_7X475,
                    LabelStockTypeSpecified = true,
                    LabelPrintingOrientation = LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST,
                    LabelPrintingOrientationSpecified = true
                };
            }

            private static RequestedPackageLineItem[] GetRequestedPackageLineItems()
            {
                return new RequestedPackageLineItem[] {
                    new RequestedPackageLineItem {
                        SequenceNumber = "1",
                        Weight = new Weight { Value = 50.0M, Units = WeightUnits.LB },
                        Dimensions = new Dimensions { Length = "12", Width = "13", Height = "14", Units = LinearUnits.IN }
                    }
                };
            }

            private static ShipmentSpecialServicesRequested GetShipmentSpecialServicesRequested(bool isCodShipment)
            {
                if (!isCodShipment)
                    return null;

                return new ShipmentSpecialServicesRequested
                {
                    SpecialServiceTypes = new ShipmentSpecialServiceType[] { ShipmentSpecialServiceType.COD },
                    CodDetail = new CodDetail
                    {
                        CollectionType = CodCollectionType.GUARANTEED_FUNDS,
                        CodCollectionAmount = new Money { Amount = 250.00M, Currency = "USD" }
                    }
                };
            }
        }
    }
}
