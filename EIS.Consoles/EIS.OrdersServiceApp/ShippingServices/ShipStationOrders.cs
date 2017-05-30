using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;

namespace EIS.OrdersServiceApp.ShippingServices
{
    public class ShipStationOrders : IDisposable
    {
        ShipStationService _shipStationService;
        private readonly string _marketplaceMode;

        public ShipStationOrders ()
        {
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
            _shipStationService = new ShipStationService();
        }

        #region Public Methods

        public List<order> GetUnshippedOrders ()
        {
            return _shipStationService.GetUnshippedOrders();
        }

        public async Task<int> PostOrderToShippingStation ()
        {
            if (!_shipStationService.HasValidCredentials)
                return -1;

            if (_marketplaceMode == MarketplaceMode.TEST.ToString())
                return 0;

            return await _shipStationService.PostOrderToShipStation();
        }

        public async Task<int> PostOrderToShippingStationByOrderNumber ( string orderNumber )
        {
            if (_marketplaceMode == MarketplaceMode.TEST.ToString())
                return 0;

            return await _shipStationService.PostOrderToShipStationByOrderNumber(orderNumber);
        }

        public async Task<List<ShipStationOrdersViewModel>> GetOrders ()
        {
            return await _shipStationService.GetOrders();
        }

        public async Task<ShipStationOrdersViewModel> GetOrderById ( string ID )
        {
            return await _shipStationService.GetOrderById(ID);
        }

        public async Task<ShipStationOrdersViewModel> GetOrderByNumber ( string orderNumber )
        {
            return await _shipStationService.GetOrderByNumber(orderNumber);
        }

        public async Task<List<ShipStationShipmentViewModel>> GetShipments ()
        {
            return await _shipStationService.GetShipments();
        }

        public async Task<List<ShipStationShipmentViewModel>> GetShipmentsByDate ( DateTime LastDate )
        {
            return await _shipStationService.GetShipmentsByDate(LastDate);
        }
        
        public async Task<ShipStationShipmentViewModel> GetShipmentsByOrderNumber ( string orderNumber )
        {
            return await _shipStationService.GetShipmentsByOrderNumber(orderNumber);
        }

        public async Task<int> PostTrackingNumberAndCost ()
        {
            if (!_shipStationService.HasValidCredentials)
                return -1;

            if (_marketplaceMode == MarketplaceMode.TEST.ToString())
                return 0;

            return await _shipStationService.PostTrackingNumberAndCost();
        }

        public async Task<int> PostTrackingNumberAndCostByOrderNumber (string orderNumber)
        {
            if (!_shipStationService.HasValidCredentials)
                return -1;

            return await _shipStationService.PostTrackingNumberAndCostByOrderNumber(orderNumber);
        }

        public string GetImageURLPublic(string eisSKU)
        {
            return _shipStationService.GetProductImageUrl(eisSKU);
        }

        public async Task<bool> DeleteOrderByOrderNumber ( string orderNumber )
        {
            return await _shipStationService.DeleteOrderByOrderNumber(orderNumber);
        }

        // use for one time only when getting weights based on end date (LastDate)
        public async Task<int> GetAccurateWeight_PreviousDates ( DateTime LastDate )
        {
            return await _shipStationService.GetAccurateWeight_PreviousDates(LastDate);
        }

        public async Task<string> CreateShipmentLabelByOrderNumber ( string orderNumber )
        {
            return await _shipStationService.CreateShipmentLabel(orderNumber);
        }


        #endregion

        #region IDisposable
        public void Dispose()
        {
            // Dispose other managed resources.
            _shipStationService.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion
    }



}
