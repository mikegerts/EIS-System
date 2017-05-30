using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.Endicia.Helpers;
using EIS.Shipping.Endicia.Service;
using EIS.Inventory.Core;

namespace EIS.Shipping.Endicia
{
    [Export(typeof(IRateProvider))]
    public class EndiciaRateProvider : IRateProvider
    {
        private readonly ILogService _logger;

        public EndiciaRateProvider() : this(Core.Get<ILogService>())
        {
        }

        public EndiciaRateProvider(ILogService logger)
        {
            _logger = logger;
        }

        public string ProviderName
        {
            get { return "Endicia"; }
        }

        public List<ShippingRate> GetShipmentRates(Shipment shipment)
        {
            System.Threading.Thread.Sleep(1500);
            return new List<ShippingRate>() {
                new ShippingRate { PackageType = "Package", MailClass = "Endicia Ground", MailService = "Parcel", Zone = "5", Pricing="Retail", TotalAmount = 23.44m, DeliveryDate = new System.DateTime().AddDays(3)},
                new ShippingRate { PackageType = "Package", MailClass = "Endicia Express", MailService = "Letter", Zone = "5", Pricing="Retail", TotalAmount = 11.32m, DeliveryDate = new System.DateTime().AddDays(2)},
                new ShippingRate { PackageType = "Package", MailClass = "Endicia Mail", MailService = "Enveloope", Zone = "5", Pricing="Retail", TotalAmount = 12.25m, DeliveryDate = new System.DateTime().AddDays(5)},
            };

            // create request object
            var requestObject = RequestHelper.CreatePostageRatesRequest(shipment);

            // send the request to change the passphrase
            var response = SoapHelper.ProcessRequest<PostageRatesResponse>(requestObject);
            if (response.Status != 0)
                throw new Exception(response.ErrorMessage);

            var results = new List<ShippingRate>();
            foreach (var rate in response.PostagePrice)
            {
                results.Add(new ShippingRate
                {
                    TotalAmount = rate.TotalAmount,
                    MailClass = rate.MailClass,
                    MailService = rate.Postage.MailService,
                    Pricing = rate.Postage.Pricing,
                    Zone = rate.Postage.Zone
                });
            }

            return results;
        }
    }
}
