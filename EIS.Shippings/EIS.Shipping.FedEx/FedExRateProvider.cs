using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel.Composition;
using System.Diagnostics;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.FedEx.Helpers;
using EIS.Shipping.FedEx.RateServiceWebReference;
using EIS.Inventory.Core;

namespace EIS.Shipping.FedEx
{
    [Export(typeof(IRateProvider))]
    public class FedExRateProvider : IRateProvider
    {
        private readonly RateService _service;
        private readonly ILogService _logger;
        
        public FedExRateProvider() : this (Core.Get<ILogService>())
        {
        }

        public FedExRateProvider(ILogService logger)
        {
            var mode = ConfigurationManager.AppSettings["FedEx.Mode"];
            _service = new RateService(mode);

            _logger = logger;
        }

        public string ProviderName
        {
            get { return "FedEx"; }
        }

        public List<ShippingRate> GetShipmentRates(Shipment shippingDetail)
        {
            System.Threading.Thread.Sleep(3000);
            return new List<ShippingRate>() {
                new ShippingRate { PackageType = "Package", MailClass = "FedEx Ground", MailService = "Parcel", Zone = "5", Pricing="Retail", TotalAmount = 23.44m, DeliveryDate = new System.DateTime().AddDays(3)},
                new ShippingRate { PackageType = "Package", MailClass = "FedEx Express", MailService = "Letter", Zone = "5", Pricing="Retail", TotalAmount = 11.32m, DeliveryDate = new System.DateTime().AddDays(2)},
                new ShippingRate { PackageType = "Package", MailClass = "FedEx Mail", MailService = "Enveloope", Zone = "5", Pricing="Retail", TotalAmount = 12.25m, DeliveryDate = new System.DateTime().AddDays(5)},
            };

            // create request object
            var requetObject = RequestHelper.CreateRateRequest(shippingDetail);

            // send the request to get the rates
            var response = _service.getRates(requetObject);

            var results = new List<ShippingRate>();
            if (response.HighestSeverity == NotificationSeverityType.SUCCESS 
                || response.HighestSeverity == NotificationSeverityType.NOTE
                || response.HighestSeverity == NotificationSeverityType.WARNING)
            {
                foreach(var rateReplyDetail in response.RateReplyDetails)
                {
                    foreach(var shipmentDetail in rateReplyDetail.RatedShipmentDetails)
                    {
                        if (shipmentDetail == null) continue;
                        if (shipmentDetail.ShipmentRateDetail == null) continue;
                        var rateDetail = shipmentDetail.ShipmentRateDetail;

                        results.Add(new ShippingRate
                        {
                            PackageType = rateReplyDetail.PackagingType.ToString(),
                            TotalAmount = rateDetail.TotalNetCharge.Amount,
                            MailClass = rateReplyDetail.ServiceType.ToString(),
                            MailService = parseServiceTypeCode(rateReplyDetail.ServiceType),
                            Pricing = rateDetail.PricingCode.ToString(),
                            Zone = rateDetail.RateZone,
                            DeliveryDate = rateReplyDetail.DeliveryTimestamp
                        });
                    }
                }
            }

            // log the notifications
            logNotifications(response);

            return results;
        }

        private void logNotifications(RateReply reply)
        {
            Debug.WriteLine("Notifications");
            for (var i = 0; i < reply.Notifications.Length; i++)
            {
                var notification = reply.Notifications[i];
                Debug.WriteLine("Notification no. {0}", i);
                Debug.WriteLine(" Severity: {0}", notification.Severity);
                Debug.WriteLine(" Code: {0}", notification.Code);
                Debug.WriteLine(" Message: {0}", notification.Message);
                Debug.WriteLine(" Source: {0}", notification.Source);
            }
        }

        private string parseServiceTypeCode(RateServiceWebReference.ServiceType serviceTypeCode)
        {
            switch (serviceTypeCode)
            {
                case RateServiceWebReference.ServiceType.PRIORITY_OVERNIGHT:
                    return "FedEx Priority Overnight";
                case RateServiceWebReference.ServiceType.FEDEX_2_DAY:
                    return "FedEx 2nd Day";
                case RateServiceWebReference.ServiceType.FEDEX_2_DAY_AM:
                    return "FedEx 2nd Day A.M.";
                case RateServiceWebReference.ServiceType.STANDARD_OVERNIGHT:
                    return "FedEx Standard Overnight";
                case RateServiceWebReference.ServiceType.FIRST_OVERNIGHT:
                    return "FedEx First Overnight";
                case RateServiceWebReference.ServiceType.FEDEX_EXPRESS_SAVER:
                    return "FedEx Express Saver";
                case RateServiceWebReference.ServiceType.FEDEX_GROUND:
                    return "FedEx Ground";
                case RateServiceWebReference.ServiceType.GROUND_HOME_DELIVERY:
                    return "FedEx Ground Residential";
                case RateServiceWebReference.ServiceType.INTERNATIONAL_ECONOMY:
                    return "FedEx International Economy";
                case RateServiceWebReference.ServiceType.INTERNATIONAL_PRIORITY:
                    return "FedEx International Priority";
                default: return serviceTypeCode.ToString();
            }
        }
    }
}
