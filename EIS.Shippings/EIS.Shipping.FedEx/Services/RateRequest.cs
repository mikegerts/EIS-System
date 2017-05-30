using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models.Shippings;
using EIS.Shipping.FedEx.Services;
using AutoMapper;
using System.Web.Services.Protocols;
using EIS.Shipping.FedEx.ShipServiceWebReference;

namespace EIS.Shipping.FedEx.RateServiceWebReference
{ 
    public partial class RateRequest : IFedExRequest
    {
        private IList<EisShipmentRate> _shipmentRate;
        public RateRequest()
        {
            _shipmentRate = new List<EisShipmentRate>();

            TransactionDetail = new TransactionDetail { CustomerTransactionId = "***Rate Request***" };
            ReturnTransitAndCommit = true;
            ReturnTransitAndCommitSpecified = true;
            Version = new VersionId();
        }
        public void SetClientDetail(EisClientDetail clientDetail)
        {
            ClientDetail = Mapper.Map<ClientDetail>(clientDetail);
        }
        public void SetRequest(EisRequestedShipment eisRequest)
        {
            RequestedShipment = Mapper.Map<RequestedShipment>(eisRequest);
        }
        public void SetWebAuthenticationDetail(EisWebAuthenticationDetail webAuthenticationDetail)
        {
            WebAuthenticationDetail = Mapper.Map<WebAuthenticationDetail>(webAuthenticationDetail);
        }
        public IList<EisShipmentRate> GetShipmentRate()
        {
            if (_shipmentRate.Count == 0)
                ExecuteRequest();

            return _shipmentRate;
        }

        public EisReply Send()
        {
            RateService service = new RateService();

            return Mapper.Map<EisReply>(service.getRates(this));
        }

        public void ExecuteRequest()
        {
            RateService service = new RateService();
            try
            {
                if (RequestedShipment == null) return;

                RateReply reply = service.getRates(this);
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    foreach (var rateReplyDetail in reply.RateReplyDetails)
                    {
                        foreach(var shipmentDetail in rateReplyDetail.RatedShipmentDetails)
                        {
                            if (shipmentDetail == null) continue;
                            if (shipmentDetail.ShipmentRateDetail == null) continue;

                            _shipmentRate.Add(Mapper.Map<EisShipmentRate>(shipmentDetail.ShipmentRateDetail));
                        }
                    }
                }
            }
            catch (SoapException e)
            {
            }
            catch (Exception e)
            {
            }
        }

        public ProcessShipmentRequest GetRequest()
        {
            throw new NotImplementedException();
        }
    }
}
