using EIS.Inventory.Shared.Models.Shippings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Test.Shippings
{
    public static class FedExTestData
    {

        #region ShipServiceData
        public static EisParty GetShipper()
        {
            return new EisParty
            {
                Contact = new EisContact { PersonName = "Sender Name", CompanyName = "Sender Company Name", PhoneNumber = "0805522713" },
                Address = new EisAddress { StreetLines = new string[1] { "Address Line 1" }, City = "Austin", StateOrProvinceCode = "TX", PostalCode = "73301", CountryCode = "US" }
            };
        }
        public static EisParty GetRecipient()
        {
            return new EisParty
            {
                Contact = new EisContact { PersonName = "Recipient Name", CompanyName = "Recipient Company Name", PhoneNumber = "9012637906" },
                Address = new EisAddress { StreetLines = new string[1] { "Address Line 1" }, City = "Windsor", StateOrProvinceCode = "CT", PostalCode = "06006", CountryCode = "US", Residential = true }
            };
        }
        public static EisPayment GetShippingChargesPayment()
        {
            return new EisPayment
            {
                PaymentType = EisPaymentType.SENDER,
                Payor = new EisPayor
                {
                    ResponsibleParty = new EisParty { AccountNumber = "510087984", Contact = new EisContact(), Address = new EisAddress { CountryCode = "US" } }
                }
            };
        }
        public static EisLabelSpecification GetLabelSpecification()
        {
            return new EisLabelSpecification
            {
                ImageType = EisShippingDocumentImageType.PDF,
                ImageTypeSpecified = true,
                LabelFormatType = EisLabelFormatType.COMMON2D,
                LabelStockType = EisLabelStockType.PAPER_7X475,
                LabelStockTypeSpecified = true,
                LabelPrintingOrientation = EisLabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST,
                LabelPrintingOrientationSpecified = true
            };
        }

        public static EisRequestedPackageLineItem[] GetRequestedPackageLineItems()
        {
            return new EisRequestedPackageLineItem[] {
                new EisRequestedPackageLineItem {
                    SequenceNumber = "1",
                    Weight = new EisWeight { Value = 50.0M, Units = EisWeightUnits.LB },
                    Dimensions = new EisDimensions { Length = "12", Width = "13", Height = "14", Units = EisLinearUnits.IN }
                }
            };
        }

        public static EisShipmentSpecialServicesRequested GetShipmentSpecialServicesRequested(bool isCodShipment)
        {
            if (!isCodShipment)
                return null;

            return new EisShipmentSpecialServicesRequested
            {
                SpecialServiceTypes = new List<EisShipmentSpecialServiceType> { EisShipmentSpecialServiceType.COD },
                CodDetail = new EisCodDetail
                {
                    CollectionType = EisCodCollectionType.GUARANTEED_FUNDS,
                    CodCollectionAmount = new EisMoney { Amount = 250.00M, Currency = "USD" }
                }
            };
        }
        #endregion


        #region RateServiceData
        public static EisParty GetRateOrigin()
        {
            return new EisParty
            {
                Address = new EisAddress { StreetLines = new string[1] { "SHIPPER ADDRESS LINE 1" }, City = "COLLIERVILLE", StateOrProvinceCode = "TN", PostalCode = "38017", CountryCode = "US" }
            };
        }
        public static EisParty GetRateDestination()
        {
            return new EisParty
            {
                Address = new EisAddress { StreetLines = new string[1] { "RECIPIENT ADDRESS LINE 1" }, City = "Montreal", StateOrProvinceCode = "PQ", PostalCode = "H1E1A1", CountryCode = "CA" }
            };
        }
        public static EisRequestedPackageLineItem[] GetRateRequestedPackageLineItems()
        {
            return new EisRequestedPackageLineItem[] {
                new EisRequestedPackageLineItem {
                    SequenceNumber = "1",
                    GroupPackageCount = "1",
                    Weight = new EisWeight { Value = 15.0M, ValueSpecified = true, Units = EisWeightUnits.LB, UnitsSpecified = true },
                    Dimensions = new EisDimensions { Length = "10", Width = "13", Height = "4", Units = EisLinearUnits.IN, UnitsSpecified = true },
                    InsuredValue = new EisMoney { Amount = 100, Currency = "USD" }
                },
                new EisRequestedPackageLineItem {
                    SequenceNumber = "2",
                    GroupPackageCount = "1",
                    Weight = new EisWeight { Value = 25.0M, ValueSpecified = true, Units = EisWeightUnits.LB, UnitsSpecified = true },
                    Dimensions = new EisDimensions { Length = "20", Width = "13", Height = "4", Units = EisLinearUnits.IN, UnitsSpecified = true },
                    InsuredValue = new EisMoney { Amount = 500, Currency = "USD" }
                },
            };
        } 
        #endregion

    }
}
