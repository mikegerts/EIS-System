using System.Configuration;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.Endicia.Service;
using System.Linq;
using System;

namespace EIS.Shipping.Endicia.Helpers
{
    public static class RequestHelper
    {
        private static string _mode = "TEST";
        private static string _requesterId = "lxxx";
        private static string _accountId = "2527126";
        private static string _passPhrase = "Denden.Mushi";

        static RequestHelper()
        {
            _mode = ConfigurationManager.AppSettings["Endicia.Mode"];
            if (_mode == "TEST")
                return;

            _requesterId = ConfigurationManager.AppSettings["Endicia.RequesterId"];
            _accountId = ConfigurationManager.AppSettings["Endicia.AccountId"];
            _passPhrase = ConfigurationManager.AppSettings["Endicia.PassPhrase"];
        }

        public static ChangePassPhraseRequest CreateChangePassPhraseRequest(string newPassPhrase)
        {
            return new ChangePassPhraseRequest
            {
                RequesterID = "lxxx", // test requester id
                RequestID = "101",
                CertifiedIntermediary = new CertifiedIntermediary
                {
                    AccountID = _accountId,
                    PassPhrase = _passPhrase
                },
                NewPassPhrase = newPassPhrase,
                TokenRequested = false // we will just use the AcccountId and PassPhrase
            };
        }

        public static RecreditRequest CreateRecreditRequest(decimal amount)
        {
            return new RecreditRequest
            {
                RequesterID = _requesterId, // test requester id
                RequestID = "101",
                CertifiedIntermediary = new CertifiedIntermediary
                {
                    AccountID = _accountId,
                    PassPhrase = _passPhrase
                },
                RecreditAmount = amount.ToString()
            };
        }

        public static PostageRateRequest CreatePostageRateRequest(Shipment detail)
        {
            return new PostageRateRequest
            {
                ResponseVersion = RespVersion.Item0,
                RequesterID = _requesterId, // test requester id
                CertifiedIntermediary = new CertifiedIntermediary
                {
                    AccountID = _accountId,
                    PassPhrase = _passPhrase
                },
                MailClass = detail.MailClass.ToString(),
                WeightOz = (double)detail.Packages.FirstOrDefault().Weight,
                MailpieceDimensions = new Dimensions
                {
                    Length = (double)detail.Packages.FirstOrDefault().Length,
                    Width = (double)detail.Packages.FirstOrDefault().Width,
                    Height = (double)detail.Packages.FirstOrDefault().Height
                },
                FromCountryCode = detail.OriginAddress.CountryCode,
                FromPostalCode = detail.OriginAddress.PostalCode,
                ToCountryCode = detail.DestinationAddress.CountryCode,
                ToPostalCode = detail.DestinationAddress.PostalCode,
                ResponseOptions = new ResponseOptions
                {
                    PostagePrice = "TRUE"
                },
                ShipDate = detail.ShipDate.ToString("MM/DD/YYYY"),
                EstimatedDeliveryDate = "TRUE",
                DeliveryTimeDays = "TRUE"
            };
        }

        public static PostageRatesRequest CreatePostageRatesRequest(Shipment detail)
        {
            return new PostageRatesRequest
            {
                ResponseVersion = RespVersion.Item0,
                RequesterID = _requesterId, // test requester id
                CertifiedIntermediary = new CertifiedIntermediary
                {
                    AccountID = _accountId,
                    PassPhrase = _passPhrase
                },
                MailClass = "Domestic",
                WeightOz = (double)detail.Packages.FirstOrDefault().Weight,
                MailpieceDimensions = new Dimensions
                {
                    Length = (double)detail.Packages.FirstOrDefault().Length,
                    Width = (double)detail.Packages.FirstOrDefault().Width,
                    Height = (double)detail.Packages.FirstOrDefault().Height
                },
                FromPostalCode = detail.OriginAddress.PostalCode,
                ToCountryCode = detail.DestinationAddress.CountryCode,
                ToPostalCode = detail.DestinationAddress.PostalCode,
                EstimatedDeliveryDate = "TRUE",
                DeliveryTimeDays = "TRUE",               
            };
        }

        public static LabelRequest CreateLabelRequest(PackageDetail detail)
        {
            return new LabelRequest
            {
                Test = (_mode == "LIVE" ? "NO" : "YES"),
                RequesterID = _requesterId, // test requester id
                AccountID = _accountId,
                PassPhrase = _passPhrase,
                MailClass = detail.MailClass.ToString(),
                WeightOz = (double)detail.ItemDimension.Weight.Value, // Must be greater than zero and cannot exceed 1120 ounces (70 pounds).
                //MailpieceDimensions = new Dimensions // optional
                //{
                //    Length = (double)detail.ItemDimension.Length.Value,
                //    Width = (double)detail.ItemDimension.Width.Value,
                //    Height = (double)detail.ItemDimension.Height.Value
                //},
                ValidateAddress = "TRUE",
                IncludePostage = "TRUE",
                ShowReturnAddress = "TRUE",
                Description = "Test request label",
                ReferenceID = detail.OrderId,
                PartnerCustomerID = detail.RequestedBy,
                PartnerTransactionID = detail.OrderItemId,
                FromName = detail.FromName,
                FromCompany = detail.FromCompany,
                ReturnAddress1 = detail.FromAddress.Line1,
                ReturnAddress2 = detail.FromAddress.Line2,
                FromCity = detail.FromAddress.City,
                FromState = detail.FromAddress.StateOrRegion,
                FromCountryCode = detail.FromAddress.CountryCode,
                FromPostalCode = detail.FromAddress.PostalCode,
                FromPhone = detail.FromPhone,
                FromEMail = detail.FromEmail,
                ToName = detail.ToName,
                ToCompany = detail.ToCompany,
                ToAddress1 = detail.ToAddress.Line1,
                ToAddress2 = detail.ToAddress.Line2,
                ToCity = detail.ToAddress.City,
                ToState = detail.ToAddress.StateOrRegion,
                ToCountryCode = detail.ToAddress.CountryCode,
                ToPostalCode = detail.ToAddress.PostalCode,
                ToEMail = detail.ToEmail,
                ToPhone = detail.ToPhone,
                ResponseOptions = new ResponseOptions
                {
                    PostagePrice = "TRUE"
                }
            };
        }
    }
}
