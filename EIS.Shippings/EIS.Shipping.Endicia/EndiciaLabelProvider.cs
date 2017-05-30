using System;
using System.Configuration;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models;
using EIS.Shipping.Endicia.Helpers;
using EIS.Shipping.Endicia.Service;

namespace EIS.Shipping.Endicia
{
    public class EndiciaLabelProvider : ILabelProvider
    {
        private readonly ILogService _logger;

        public EndiciaLabelProvider(ILogService logger)
        {
            _logger = logger;
        }

        public string ProviderName
        {
            get { return "Endicia"; }
        }

        public ChangePassPhraseRequestResponse ChangePassPhrase(string newPassPhrase)
        {
            // create request object
            var requestObject = RequestHelper.CreateChangePassPhraseRequest(newPassPhrase);

            // send the request to change the passphrase
            var response = SoapHelper.ProcessRequest<ChangePassPhraseRequestResponse>(requestObject);

            return response;
        }

        public PostageLabel GetPostageLabel(PackageDetail packageDetail)
        {
            // create request object
            var requestObject = RequestHelper.CreateLabelRequest(packageDetail);

            // send the request to change the passphrase
            var response = SoapHelper.ProcessRequest<LabelRequestResponse>(requestObject);
            if (response.Status != 0)
                return new PostageLabel { ErrorMessage = response.ErrorMessage };

            return new PostageLabel
            {
                Base64LabelImage = response.Base64LabelImage,
                TrackingNumber = response.TrackingNumber,
                TransactionId = response.TransactionID,
                TransactionDateTime = response.TransactionDateTime,
                PostmarkDate = response.PostmarkDate,
                PostageBalance = response.PostageBalance,
                PostageTotalPrice = response.PostagePrice.TotalAmount
            };
        }
    }
}
