using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models.Shippings;
using EIS.Shipping.FedEx.ShipServiceWebReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace EIS.Shipping.FedEx
{
    public class FedExShippingProvider_v1
    {
        ProcessShipmentRequest _request;
        ProcessShipmentReply _reply;
        PackageRateDetail[] _packageRateDetails;
        CompletedShipmentDetail _completedShipmentDetail;
        CompletedPackageDetail _completedPackageDetail;
        TrackingId[] _trackingIds;

        public FedExShippingProvider_v1()
        {
            _request = new ProcessShipmentRequest();
        }
        public void SetRequestedShipment(RequestedShipment requestedShipment)
        {
            _request.RequestedShipment = requestedShipment;
        }
        public void SetClientDetail(ClientDetail clientDetail)
        {
            _request.ClientDetail = clientDetail;
        }

        public void SetWebAuthenticationDetail(WebAuthenticationDetail webAuthenticationDetail)
        {
            _request.WebAuthenticationDetail = webAuthenticationDetail;
        }

        public void SetVersion(VersionId versionId)
        {
            _request.Version = versionId;
        }


        public void SetTransactionDetail(TransactionDetail transactionDetail)
        {
            _request.TransactionDetail = transactionDetail;
        }

        public IList<PackageRateDetail> GetShipmentRate()
        {
            if (_reply == null)
                ExecuteRequest();

            return _packageRateDetails;
        }

        public void ExecuteRequest()
        {
            ShipService service = new ShipService();
            try
            {
                if (_request.RequestedShipment == null) return;

                _reply = service.processShipment(_request);
                foreach (CompletedPackageDetail packageDetail in _reply.CompletedShipmentDetail.CompletedPackageDetails)
                {
                    _trackingIds = packageDetail.TrackingIds;
                    if (packageDetail.PackageRating != null && packageDetail.PackageRating.PackageRateDetails != null)
                    {
                        _packageRateDetails = packageDetail.PackageRating.PackageRateDetails;
                    }
                    else
                    {
                        Console.WriteLine("No Rating information returned.\n");
                    }
                    _completedPackageDetail = packageDetail;
                    _completedShipmentDetail = _reply.CompletedShipmentDetail;
                }

            }
            catch (SoapException e)
            {
            }
            catch (Exception e)
            {
            }
        }
    }
}
