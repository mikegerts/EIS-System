using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.Shippings;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Managers
{
    public class ShippingRateManager : IShippingRateManager
    {
        private readonly ILogService _logger;
        private readonly string _shippingMode;

        public ShippingRateManager(ILogService logger)
        {
            _logger = logger;
            _shippingMode = ConfigurationManager.AppSettings["ShippingMode"];
            Core.Container.ComposeParts(this);
        }


        [ImportMany(typeof(IRateProvider))]
        protected List<IRateProvider> _rateProviders { get; set; }

        public List<ShippingRate> GetShipmentRates(string providerName, Shipment shipment)
        {
            var provider = getRateProvider(providerName);
            if(provider == null)
            {
                _logger.LogError(LogEntryType.General, string.Format("Rate provider for \'{0}\' was not found!", providerName), string.Empty);
                return null;
            }

            return provider.GetShipmentRates(shipment);
        }

        private IRateProvider getRateProvider(string providerName)
        {
            return _rateProviders.FirstOrDefault(x => x.ProviderName == providerName);
        }
    }
}
