using AutoMapper;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models.Shippings;
using EIS.Shipping.FedEx;
using EIS.Shipping.FedEx.Configuration;
using System;
using System.Collections.Generic;
using Xunit;

namespace EIS.Inventory.Test.Shippings
{
    public class FedExServiceTests
    {
        private readonly ILogService _logger;

        public FedExServiceTests()
        {
            AutoMapperConfiguration.Configure();
            _logger = new LogService();
        }

        [Fact]
        public void ShouldProcessShipmentRequest()
        {
            // Arrange
            var provider = new FedExShippingProvider(EIS.Shipping.FedEx.Services.FedExRequestFactory.RequestType.Shipment, _logger);
            var requestedShipment = new EisRequestedShipment
            {
                ShipTimestamp = DateTime.Now,
                DropoffType = EisDropoffType.REGULAR_PICKUP,
                ServiceType = EisServiceType.PRIORITY_OVERNIGHT,
                PackagingType = EisPackagingType.YOUR_PACKAGING,
                TotalWeight = new EisWeight { Value = 50.0M, Units = EisWeightUnits.LB },
                PackageCount = "1",
                Shipper = FedExTestData.GetShipper(),
                Recipient = FedExTestData.GetRecipient(),
                ShippingChargesPayment = FedExTestData.GetShippingChargesPayment(),
                LabelSpecification = FedExTestData.GetLabelSpecification(),
                RequestedPackageLineItems = FedExTestData.GetRequestedPackageLineItems(),
                SpecialServicesRequested = FedExTestData.GetShipmentSpecialServicesRequested(true)
            };

            provider.SetRequest(requestedShipment);

            // Act
            var reply = provider.Send();

            // Assert
            Assert.NotNull(reply);
        }


        [Fact]
        public void ShouldProcessRateRequest()
        {
            // Arrange
            var provider = new FedExShippingProvider(EIS.Shipping.FedEx.Services.FedExRequestFactory.RequestType.Rate, _logger);

            var request = new EisRequestedShipment
            {
                ShipTimestamp = DateTime.Now,
                DropoffType = EisDropoffType.REGULAR_PICKUP,
                ServiceType = EisServiceType.INTERNATIONAL_PRIORITY,
                ServiceTypeSpecified = true,
                PackagingType = EisPackagingType.YOUR_PACKAGING,
                PackagingTypeSpecified = true,
                TotalInsuredValue = new EisMoney { Amount = 100, Currency = "USD" },
                TotalWeight = new EisWeight { Value = 50.0M, Units = EisWeightUnits.LB },
                PackageCount = "2",
                Shipper = FedExTestData.GetRateOrigin(),
                Recipient = FedExTestData.GetRateDestination(),
                RequestedPackageLineItems = FedExTestData.GetRateRequestedPackageLineItems(),
            };

            provider.SetRequest(request);

            // Act
            var rate = provider.GetShipmentRates(null);

            // Assert
            Assert.NotNull(rate);
            Assert.True(rate.Count > 0);
        }

        [Fact]
        public void ShouldProcessLabel()
        {
            //Arrange
            var provider = new FedExShippingProvider(EIS.Shipping.FedEx.Services.FedExRequestFactory.RequestType.Shipment, _logger);
            var requestedShipment = new EisRequestedShipment
            {
                ShipTimestamp = DateTime.Now,
                DropoffType = EisDropoffType.REGULAR_PICKUP,
                ServiceType = EisServiceType.PRIORITY_OVERNIGHT,
                PackagingType = EisPackagingType.YOUR_PACKAGING,
                TotalWeight = new EisWeight { Value = 50.0M, Units = EisWeightUnits.LB },
                PackageCount = "1",
                Shipper = FedExTestData.GetShipper(),
                Recipient = FedExTestData.GetRecipient(),
                ShippingChargesPayment = FedExTestData.GetShippingChargesPayment(),
                LabelSpecification = FedExTestData.GetLabelSpecification(),
                RequestedPackageLineItems = FedExTestData.GetRequestedPackageLineItems(),
                SpecialServicesRequested = FedExTestData.GetShipmentSpecialServicesRequested(true)
            };

            provider.SetRequest(requestedShipment);

            // Act
            var label = provider.GetShipmentLabel();

            // Assert
            Assert.NotNull(label);
        }
    }
}
