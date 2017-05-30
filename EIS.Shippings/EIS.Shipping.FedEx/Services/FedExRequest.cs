using EIS.Inventory.Shared.Models.Shippings;
using System.Collections.Generic;

namespace EIS.Shipping.FedEx.Services
{
    public interface IFedExRequest
    {
        void SetRequest(EisRequestedShipment eisRequest);
        void SetWebAuthenticationDetail(EisWebAuthenticationDetail webAuthenticationDetail);
        void SetClientDetail(EisClientDetail clientDetail);
        IList<EisShipmentRate> GetShipmentRate();
        EisReply Send();
//        ShipServiceWebReference.ProcessShipmentRequest GetRequest();
    }
    //public abstract class FedExRequest
    //{
    //    public virtual Party SetParty(EisParty eisParty)
    //    {
    //        return new Party
    //        {
    //            AccountNumber = eisParty.AccountNumber,
    //            Contact = SetContact(eisParty.Contact),
    //            Address = SetAddress(eisParty.Address),
    //            Tins = SetTaxpayerIdentifications(eisParty.Tins)
    //        };
    //    }

    //    public virtual TaxpayerIdentification[] SetTaxpayerIdentifications(IList<EisTaxpayerIdentification> eisTaxpayerIdentifications)
    //    {
    //        if (eisTaxpayerIdentifications == null) return null;

    //        return eisTaxpayerIdentifications.Select(t => SetTaxpayerIdentification(t)).ToArray();
    //    }

    //    public virtual TaxpayerIdentification SetTaxpayerIdentification(EisTaxpayerIdentification eisTaxpayerIdentification)
    //    {
    //        return new TaxpayerIdentification
    //        {
    //            TinType = (TinType)eisTaxpayerIdentification.TinType,
    //            Number = eisTaxpayerIdentification.Number
    //        };
    //    }

    //    public virtual Payment SetPayment(EisPayment eisPayment)
    //    {
    //        return new Payment
    //        {
    //            PaymentType = (PaymentType)eisPayment.PaymentType,
    //            Payor = new Payor() { ResponsibleParty = SetParty(eisPayment.Payor.ResponsibleParty) }
    //        };
    //    }

    //    public virtual Address SetAddress(EisAddress eisAddress)
    //    {
    //        return new Address
    //        {
    //            City = eisAddress.City,
    //            CountryCode = eisAddress.CountryCode,
    //            PostalCode = eisAddress.PostalCode,
    //            Residential = eisAddress.Residential,
    //            StateOrProvinceCode = eisAddress.StateOrProvinceCode,
    //            StreetLines = eisAddress.StreetLines
    //        };
    //    }

    //    public virtual Contact SetContact(EisContact eisContact)
    //    {
    //        return new Contact
    //        {
    //            CompanyName = eisContact.CompanyName,
    //            ContactId = eisContact.ContactId,
    //            PersonName = eisContact.PersonName,
    //            PhoneNumber = eisContact.PhoneNumber
    //        };
    //    }

    //    public virtual LabelSpecification SetLabelSpecification(EisLabelSpecification eisLabelSpecification)
    //    {
    //        return new LabelSpecification
    //        {
    //            ImageType = (ShippingDocumentImageType)eisLabelSpecification.ImageType,
    //            ImageTypeSpecified = eisLabelSpecification.ImageTypeSpecified,
    //            LabelFormatType = (LabelFormatType)eisLabelSpecification.LabelFormatType,
    //            LabelStockType = (LabelStockType)eisLabelSpecification.LabelStockType,
    //            LabelStockTypeSpecified = eisLabelSpecification.LabelStockTypeSpecified,
    //            LabelPrintingOrientation = (LabelPrintingOrientationType)eisLabelSpecification.LabelPrintingOrientation,
    //            LabelPrintingOrientationSpecified = eisLabelSpecification.LabelPrintingOrientationSpecified,
    //        };
    //    }

    //    public virtual RequestedPackageLineItem[] SetRequestedPackageLineItems(IList<EisRequestedPackageLineItem> eisRequestedPackageLineItems)
    //    {
    //        if (eisRequestedPackageLineItems == null) return null;

    //        return eisRequestedPackageLineItems.Select(e => SetRequestedPackageLineItem(e)).ToArray();
    //    }

    //    public virtual RequestedPackageLineItem SetRequestedPackageLineItem(EisRequestedPackageLineItem eisRequestedPackageLineItem)
    //    {
    //        return new RequestedPackageLineItem
    //        {
    //            SequenceNumber = eisRequestedPackageLineItem.SequenceNumber,
    //            Weight = SetWeight(eisRequestedPackageLineItem.Weight),
    //            Dimensions = SetDimensions(eisRequestedPackageLineItem.Dimensions)
    //        };
    //    }

    //    public virtual Weight SetWeight(EisWeight eisWeight)
    //    {
    //        return new Weight
    //        {
    //            Value = eisWeight.Value,
    //            Units = (WeightUnits)eisWeight.Units
    //        };
    //    }

    //    public virtual Dimensions SetDimensions(EisDimensions eisDimensions)
    //    {
    //        return new Dimensions
    //        {
    //            Height = eisDimensions.Height,
    //            Length = eisDimensions.Length,
    //            Width = eisDimensions.Width,
    //            Units = (LinearUnits)eisDimensions.Units
    //        };
    //    }

    //    public virtual Money SetMoney(EisMoney eisMoney)
    //    {
    //        return new Money
    //        {
    //            Amount = eisMoney.Amount,
    //            Currency = eisMoney.Currency
    //        };
    //    }

    //    public virtual CodDetail SetCodDetail(EisCodDetail eisCodDetail)
    //    {
    //        return new CodDetail
    //        {
    //            CollectionType = (CodCollectionType)eisCodDetail.CollectionType,
    //            CodCollectionAmount = SetMoney(eisCodDetail.CodCollectionAmount)
    //        };
    //    }

    //    public virtual ShipmentSpecialServicesRequested SetShipmentSpecialServicesRequested(EisShipmentSpecialServicesRequested eisSpecialServicesRequested)
    //    {
    //        return new ShipmentSpecialServicesRequested
    //        {
    //            SpecialServiceTypes = SetShipmentSpecialServiceTypes(eisSpecialServicesRequested.SpecialServiceTypes),
    //            CodDetail = SetCodDetail(eisSpecialServicesRequested.CodDetail)
    //        };
    //    }

    //    public virtual ShipmentSpecialServiceType[] SetShipmentSpecialServiceTypes(IList<EisShipmentSpecialServiceType> eisShipmentSpecialServiceTypes)
    //    {
    //        if (eisShipmentSpecialServiceTypes == null) return null;

    //        return eisShipmentSpecialServiceTypes.Select(s => SetShipmentSpecialServiceType(s)).ToArray();
    //    }

    //    public virtual ShipmentSpecialServiceType SetShipmentSpecialServiceType(EisShipmentSpecialServiceType eisShipmentSpecialServiceType)
    //    {
    //        return (ShipmentSpecialServiceType)eisShipmentSpecialServiceType;
    //    }
    //}
}
