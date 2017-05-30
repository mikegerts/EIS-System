using EIS.Shipping.FedEx.Services;
using System;
using System.Collections.Generic;
using EIS.Inventory.Shared.Models.Shippings;
using AutoMapper;

namespace EIS.Shipping.FedEx.ShipServiceWebReference
{
    public partial class ProcessShipmentRequest : IFedExRequest
    {
        public ProcessShipmentRequest()
        {
            TransactionDetail = new TransactionDetail { CustomerTransactionId = "***Ship Request***" };
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

        public EisReply Send()
        {
            ShipService service = new ShipService();

            return Mapper.Map<EisReply>(service.processShipment(this));
        }

        #region NotImplemented
        public IList<EisShipmentRate> GetShipmentRate()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
