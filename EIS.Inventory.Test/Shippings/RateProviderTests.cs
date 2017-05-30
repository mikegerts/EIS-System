using System.Collections.Generic;
using System.Linq;
using Xunit;
using AutoMapper;
using EIS.Inventory.Core.Mappings;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.Endicia;
using EIS.Shipping.FedEx;

namespace EIS.Inventory.Test.Shippings
{
    public class RateProviderTests
    {
        private readonly ILogService _logger;

        public RateProviderTests()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
            _logger = new LogService();
        }

        [Fact]
        public void Should_Endicia_GetShipmentRates()
        {
            // arrange
            var provider = new EndiciaRateProvider(_logger);

            // act
            var rates = doGetShipmentRates(provider);
            
            // assert
            Assert.NotNull(rates);
            Assert.True(rates.Any());

            foreach (var rate in rates)
            {
                Assert.True(rate.TotalAmount > 0);
            }
        }

        [Fact]
        public void Should_FedEx_GetShipmentRates()
        {
            // arrange
            var provider = new FedExRateProvider(_logger);

            // act
            var rates = doGetShipmentRates(provider);

            // assert
            Assert.NotNull(rates);
            Assert.True(rates.Any());

            foreach (var rate in rates)
            {
                Assert.True(rate.TotalAmount > 0);
            }
        }

        private List<ShippingRate> doGetShipmentRates(IRateProvider provider)
        { 
            // arrage
            var from = new Address("Annapolis", "MD", "21401", "US");
            var to = new Address("Fitchburg", "WI", "53711", "US");
            var packageItems = new List<Package>();
            packageItems.Add(new Package(4, 3, 2, 2, 2.2m));
            packageItems.Add(new Package(7, 7, 7, 6, 0));
            var shipment = new Shipment(from, to, packageItems);
            shipment.TransactionId = "10000001";

            // act
            return provider.GetShipmentRates(shipment);
        }
    }
}
