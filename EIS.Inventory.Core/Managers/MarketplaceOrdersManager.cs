using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Managers
{
    public class MarketplaceOrdersManager : IMarketplaceOrdersManager
    {
        private readonly string _marketplaceMode;
        private readonly IOrderService _orderService;
        private readonly ICredentialService _credentialService;

        public MarketplaceOrdersManager(IOrderService orderService,
            ICredentialService credentialService)
        {
            _orderService = orderService;
            _credentialService = credentialService;
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
            Core.Container.ComposeParts(this);
        }

        [ImportMany(typeof(IMarketplaceOrdersProvider))]
        protected List<IMarketplaceOrdersProvider> _marketplaceOrdersProviders { get; set; }

        public List<Carrier> GetShippingCarriers(string marketplace)
        {
            var orderProvider = getMarketplaceOrderProvider(marketplace);

            return orderProvider.GetShippingCarriers();
        }

        public int ImportMarketplaceOrdersData(string marketplace, List<string> marketplaceOrderIds)
        {
            // get the order provider
            var orderProvider = getMarketplaceOrderProvider(marketplace);
            marketplaceOrderIds = marketplaceOrderIds.Select(id => id.Trim().TrimEnd('\r', '\n')).ToList();

            // TODO: this should get the marketplace credentials to which order come (MerchantID might require)
            var credential = _credentialService.GetCredential(marketplace, MarketplaceMode.LIVE.ToString());

            orderProvider.MarketplaceCredential = credential;
            var orderResults = orderProvider.GetMarketplaceOrders(marketplaceOrderIds);
            if (orderResults == null)
                return 0;

            // TODO: Implementation for saving the marketplace orders
            return _orderService.SaveMarketplaceOrders(orderResults);
        }

        public OrderListViewModel UpdateMarketplaceOrderData(string marketplace, string marketplaceOrderId)
        {
            // get the order provider
            var orderProvider = getMarketplaceOrderProvider(marketplace);

            // TODO: this should get the marketplace credentials to which order come (MerchantID might require)
            var credential = _credentialService.GetCredential(marketplace, _marketplaceMode);

            // set the credential and get the latest order data
            orderProvider.MarketplaceCredential = credential;
            var orderResult = orderProvider.GetMarketplaceOrder(marketplaceOrderId);
            if (orderResult == null)
                return null;

            // update the order information in EIS
            return _orderService.UpdateMarketplaceOrder(marketplaceOrderId, orderResult);
        }

        public async Task ConfirmOrderShipmentAsync(OrderShipmentViewModel shipmentModel, string userName)
        {
            // log the order shipment order first to the database
            shipmentModel.SubmittedBy = userName;
            _orderService.LogOrderShipment(shipmentModel);

            // convert the model into marketplace object
            var marketplaceOrder = Mapper.Map<OrderShipmentViewModel, MarketplaceOrderFulfillment>(shipmentModel);

            // get the marketplace credential define for the company
            var credential = _credentialService.GetCredential(shipmentModel.Marketplace, _marketplaceMode);

            var ordersProvider = getMarketplaceOrderProvider(shipmentModel.Marketplace);
            ordersProvider.MarketplaceCredential = credential;

            bool isSucceed = false;
            await Task.Run(() =>
            {
                isSucceed = ordersProvider.ConfirmOrderShimpmentDetails(marketplaceOrder, userName);
            });
            
            // update the shipment order log
            shipmentModel.IsSucceed = isSucceed;
            _orderService.UpdateOrderShipment(shipmentModel);
        }

        public bool CancelOrder(string orderId, string marketplace)
        {
            // get the marketplace credential define for the company
            var credential = _credentialService.GetCredential(marketplace, _marketplaceMode);

            // get the order provider and set its credentials
            var orderProvider = getMarketplaceOrderProvider(marketplace);
            orderProvider.MarketplaceCredential = credential;

            return orderProvider.CancelOrder(orderId);
        }

        public bool UnshippedOrder(string orderId, string marketplace)
        {
            // get the marketplace credential define for the company
            var credential = _credentialService.GetCredential(marketplace, _marketplaceMode);

            //get the order provider and set its credentials
            var orderProvider = getMarketplaceOrderProvider(marketplace);
            orderProvider.MarketplaceCredential = credential;

            return orderProvider.UnshippedOrder(orderId);
        }

        private IMarketplaceOrdersProvider getMarketplaceOrderProvider(string marketplace)
        {
            return _marketplaceOrdersProviders.FirstOrDefault(x => x.ChannelName == marketplace);
        }
    }
}
