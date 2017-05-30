using System;
using System.Configuration;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.FedEx.RateServiceWebReference;

namespace EIS.Shipping.FedEx.Helpers
{
    public static class RequestHelper
    {
        private static string _mode = "TEST";
        private static string _key = "E3UuJjVs7BDqcCi0";
        private static string _password = "Zd4qb3bWxKLiXq8QZ4FABQcUo";
        private static string _accontNumber = "510087720";
        private static string _meterNumber = "118789010";
        private static bool _allowInsuredValues = false;

        static RequestHelper()
        {
            _mode = ConfigurationManager.AppSettings["FedEx.Mode"];
            //if (_mode == "TEST")
            //    return;

            _key = ConfigurationManager.AppSettings["FedEx.Key"];
            _password = ConfigurationManager.AppSettings["FedEx.Password"];
            _accontNumber = ConfigurationManager.AppSettings["FedEx.AccountNumber"];
            _meterNumber = ConfigurationManager.AppSettings["FedEx.MeterNumber"];
        }

        public static RateRequest CreateRateRequest(Shipment detail)
        {
            var request = new RateRequest
            {
                // set the credentails
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = getWebAuthenticationCredential(),
                    ParentCredential = getWebAuthenticationCredential(),
                },
                ClientDetail = getClientDetail(),
                // set the transaction details
                TransactionDetail = new TransactionDetail
                {
                    CustomerTransactionId = detail.TransactionId,
                },
                Version = new VersionId(),
                ReturnTransitAndCommit = true,
                ReturnTransitAndCommitSpecified = true,
                // set the shipment details
                RequestedShipment = new RequestedShipment
                {
                    ShipTimestamp = DateTime.Now,
                    ShipTimestampSpecified = true,
                    //ServiceType = ServiceType.FEDEX_GROUND, // Service types are STANDARD_OVERNIGHT, PRIORITY_OVERNIGHT, FEDEX_GROUND ...
                    //ServiceTypeSpecified = true,
                    // set the shipping origin or the sender
                    Shipper = new Party
                    {
                        Address = new RateServiceWebReference.Address
                        {
                            StreetLines = new string[] { detail.OriginAddress.Line1, detail.OriginAddress.Line2 },
                            City = detail.OriginAddress.City,
                            StateOrProvinceCode = detail.OriginAddress.StateOrRegion,
                            PostalCode = detail.OriginAddress.PostalCode,
                            CountryCode = detail.OriginAddress.CountryCode
                        }
                    },
                    // set the recepient or the destination information
                    Recipient = new Party
                    {
                        Address = new RateServiceWebReference.Address
                        {
                            StreetLines = new string[] { detail.DestinationAddress.Line1, detail.DestinationAddress.Line2 },
                            City = detail.DestinationAddress.City,
                            StateOrProvinceCode = detail.DestinationAddress.StateOrRegion,
                            PostalCode = detail.DestinationAddress.PostalCode,
                            CountryCode = detail.DestinationAddress.CountryCode
                        }
                    },
                },
            };

            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[detail.Packages.Count];

            // iterate to each package items and add it
            for (var i=0; i < detail.Packages.Count; i++)
            {
                var package = detail.Packages[i];
                request.RequestedShipment.RequestedPackageLineItems[i] = new RequestedPackageLineItem();
                request.RequestedShipment.RequestedPackageLineItems[i].SequenceNumber = (i + 1).ToString(); // package sequence number
                request.RequestedShipment.RequestedPackageLineItems[i].GroupPackageCount = "1"; // group counter
                // package weight
                request.RequestedShipment.RequestedPackageLineItems[i].Weight = new Weight();
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Units = WeightUnits.LB;
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.UnitsSpecified = true;
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Value = package.Weight;
                request.RequestedShipment.RequestedPackageLineItems[i].Weight.ValueSpecified = true;
                // package dimensions
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions = new Dimensions();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Length = package.Length.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Width = package.Width.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Height = package.Height.ToString();
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Units = LinearUnits.IN;
                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.UnitsSpecified = true;

                if (_allowInsuredValues)
                {
                    // package insured value
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue = new Money();
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Amount = package.InsuredValue;
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.AmountSpecified = true;
                    request.RequestedShipment.RequestedPackageLineItems[i].InsuredValue.Currency = "USD";
                }

                if (package.SignatureRequiredOnDelivery)
                {
                    var signatureOptionDetail = new SignatureOptionDetail { OptionType = SignatureOptionType.DIRECT };
                    var specialServicesRequested = new PackageSpecialServicesRequested() { SignatureOptionDetail = signatureOptionDetail };
                    request.RequestedShipment.RequestedPackageLineItems[i].SpecialServicesRequested = specialServicesRequested;
                }
            }

            // set the total package items to ship
            request.RequestedShipment.PackageCount = detail.Packages.Count.ToString();
            request.RequestedShipment.RateRequestTypes = new RateRequestType[1];
            request.RequestedShipment.RateRequestTypes[0] = RateRequestType.LIST; // multiple rates
           
            return request;
        }

        private static WebAuthenticationCredential getWebAuthenticationCredential()
        {
            return new WebAuthenticationCredential { Key = _key, Password = _password };
        }

        private static ClientDetail getClientDetail()
        {
            return new ClientDetail { AccountNumber = _accontNumber, MeterNumber = _meterNumber };
        }
    }
}
