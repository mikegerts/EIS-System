using AutoMapper;
using EIS.Inventory.Shared.Models.Shippings;
using EIS.Shipping.FedEx.RateServiceWebReference;

namespace EIS.Shipping.FedEx.Configuration
{
    public class EisRateServiceProfile: Profile
    {
        public EisRateServiceProfile()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<EisAddress, Address>();
                cfg.CreateMap<EisCodDetail, CodDetail>();
                cfg.CreateMap<EisContact, Contact>();
                cfg.CreateMap<EisDimensions, Dimensions>();
                cfg.CreateMap<EisLabelSpecification, LabelSpecification>();
                cfg.CreateMap<EisMoney, Money>();
                cfg.CreateMap<EisParty, Party>();
                cfg.CreateMap<EisPayor, Payor>();
                cfg.CreateMap<EisPayment, Payment>();
                cfg.CreateMap<EisRequestedPackageLineItem, RequestedPackageLineItem>();
                cfg.CreateMap<EisShipmentRate, ShipmentRateDetail>();
                cfg.CreateMap<EisShipmentSpecialServicesRequested, ShipmentSpecialServicesRequested>();
                cfg.CreateMap<EisSurcharge, Surcharge>();
                cfg.CreateMap<EisTaxpayerIdentification, TaxpayerIdentification>();
                cfg.CreateMap<EisWeight, Weight>();
                cfg.CreateMap<EisRequestedShipment, RequestedShipment>();
                cfg.CreateMap<EisWebAuthenticationDetail, WebAuthenticationDetail>();
                cfg.CreateMap<EisWebAuthenticationCredential, WebAuthenticationCredential>();
                cfg.CreateMap<EisClientDetail, ClientDetail>();
                cfg.CreateMap<EisVersionId, VersionId>();


                cfg.CreateMap<Address, EisAddress>();
                cfg.CreateMap<CodDetail, EisCodDetail>();
                cfg.CreateMap<Contact, EisContact>();
                cfg.CreateMap<Dimensions, EisDimensions>();
                cfg.CreateMap<LabelSpecification, EisLabelSpecification>();
                cfg.CreateMap<Money, EisMoney>();
                cfg.CreateMap<Party, EisParty>();
                cfg.CreateMap<Payor, EisPayor>();
                cfg.CreateMap<Payment, EisPayment>();
                cfg.CreateMap<RequestedPackageLineItem, EisRequestedPackageLineItem>();
                cfg.CreateMap<ShipmentRateDetail, EisShipmentRate>();
                cfg.CreateMap<ShipmentSpecialServicesRequested, EisShipmentSpecialServicesRequested>();
                cfg.CreateMap<Surcharge, EisSurcharge>();
                cfg.CreateMap<TaxpayerIdentification, EisTaxpayerIdentification>();
                cfg.CreateMap<Weight, EisWeight>();
                cfg.CreateMap<RequestedShipment, EisRequestedShipment>();
                cfg.CreateMap<WebAuthenticationDetail, EisWebAuthenticationDetail>();
                cfg.CreateMap<WebAuthenticationCredential, EisWebAuthenticationCredential>();
                cfg.CreateMap<ClientDetail, EisClientDetail>();


                cfg.CreateMap<RateReply, EisReply>();
                cfg.CreateMap<TransactionDetail, EisTransactionDetail>();
            });
        }
        public override string ProfileName
        {
            get { return "EisRateServiceProfile"; }
        }
    }
}
