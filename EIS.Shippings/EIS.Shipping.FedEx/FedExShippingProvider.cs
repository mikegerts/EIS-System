using System;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using EIS.Inventory.Core.Shippings;
using EIS.Shipping.FedEx.RateServiceWebReference;
using EIS.Shipping.FedEx.ShipServiceWebReference;
using EIS.Inventory.Shared.Models.Shippings;
using AutoMapper;
using EIS.Shipping.FedEx.Services;

namespace EIS.Shipping.FedEx
{
    public class FedExShippingProvider : IShippingProvider
    {
        IFedExRequest _request;
        public FedExShippingProvider(EIS.Shipping.FedEx.Services.FedExRequestFactory.RequestType requestType)
        {
            _request = CreateRequest(requestType);

            _request.SetWebAuthenticationDetail(GetWebAuthenticationDetail());
            _request.SetClientDetail(GetClientDetail());
        }

        private IFedExRequest CreateRequest(FedExRequestFactory.RequestType requestType)
        {
            throw new NotImplementedException();
        }

        private static EisWebAuthenticationDetail GetWebAuthenticationDetail()
        {
            var wad = new EisWebAuthenticationDetail();
            wad.UserCredential = new EisWebAuthenticationCredential();
            wad.UserCredential.Key = "ihVYTuZYiLziO0hH"; //TODO: should get from config file
            wad.UserCredential.Password = "FUdmnvgQK56GaqQoGg0cTVCVY"; //TODO: should get from config file

            wad.ParentCredential = new EisWebAuthenticationCredential();
            wad.ParentCredential.Key = "ihVYTuZYiLziO0hH"; //TODO: should get from config file
            wad.ParentCredential.Password = "FUdmnvgQK56GaqQoGg0cTVCVY"; //TODO: should get from config file

            return wad;
        }

        private static EisClientDetail GetClientDetail()
        {
            return new EisClientDetail
            {
                AccountNumber = "510087984",
                MeterNumber = "118766159"
            };
        }
        public void SetRequest(EisRequestedShipment eisRequest)
        {
            _request.SetRequest(eisRequest);
        }
        public IList<EisShipmentRate> GetShipmentRate()
        {
            return _request.GetShipmentRate();
        }

        public EisReply Send()
        {
            return _request.Send();
        }
    }
}
