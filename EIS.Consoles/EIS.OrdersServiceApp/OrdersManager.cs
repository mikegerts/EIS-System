using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using EIS.OrdersServiceApp.Repositories;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;

namespace EIS.OrdersServiceApp
{
    public class OrdersManager
    {
        [ImportMany(typeof(IMarketplaceOrdersService))]
        private List<IMarketplaceOrdersService> _marketplaceOrdersServices;

        private readonly CredentialRepository _credentialRepository;
        private readonly string _marketplaceMode;
        private readonly LoggerRepository _logger;
        private EmailNotificationService _emailService;

        public OrdersManager(CompositionContainer container)
        {
            container.ComposeParts(this);
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
            _credentialRepository = new CredentialRepository();
            _logger = new LoggerRepository();
            _emailService = new EmailNotificationService();
        }

        public List<MarketplaceOrder> DownloadMarketplaceOrders(DateTime createdAfter)
        {
            // create list for the task for marketplace services
            var tasks = new List<Task>();
            var ordersResults = new List<MarketplaceOrder>();

            for (var index = 0; index < _marketplaceOrdersServices.Count; index++)
            {
                // create temp for the counter
                var tempIndex = index; 
                var ordersService = _marketplaceOrdersServices[tempIndex];
                var credential = _credentialRepository.GetDefaultCredential(ordersService.ChannelName, _marketplaceMode);
                if (credential == null)
                {
                    _logger.LogWarning(LogEntryType.OrderService,
                        string.Format("No default credentials found for {0} - {1} mode.",
                        ordersService.ChannelName, _marketplaceMode));
                    continue;
                }

                var task = Task.Factory.StartNew(() =>
                {   
                    ordersService.Credential = credential;

                    var worker = new OrdersServiceWorker(_marketplaceOrdersServices[tempIndex], _logger);
                    var orders = worker.GetMarketplaceOrders(createdAfter);

                    if (orders != null)
                        ordersResults.AddRange(orders);
                });

                tasks.Add(task);
            }

            //Parent thread will be terminated after timeout anyway
            Task.WaitAll(tasks.ToArray());
            tasks.ForEach(t => t.Dispose());

            // insert the orders into database            
            insertOrUpdateMarketplaceOrdersToDb(ordersResults);

            // Update Shipping Service (ShipStation) for cancelled orders
            DeleteCancelledOrders(ordersResults);

            return ordersResults;
        }

        public void ConfirmOrdersShipment()
        {
            // create list for the task for marketplace providers
            var tasks = new List<Task>();

            for (var index = 0; index < _marketplaceOrdersServices.Count; index++)
            {
                // create temp for the counter
                var tempIndex = index;
                var task = Task.Factory.StartNew(() =>
                {
                    var orderManager = _marketplaceOrdersServices[tempIndex];
                    orderManager.Credential = _credentialRepository
                        .GetDefaultCredential(orderManager.ChannelName, _marketplaceMode);

                    orderManager.ConfirmOrdersShipment();
                });

                tasks.Add(task);
            }

            //Parent thread will be terminated after timeout anyway
            Task.WaitAll(tasks.ToArray());
            tasks.ForEach(t => t.Dispose());
        }

        private void insertOrUpdateMarketplaceOrdersToDb(List<MarketplaceOrder> orders)
        {
            try
            {
                if (!orders.Any())
                {
                    Console.WriteLine("No marketplace orders record to be inserted.");
                    return;
                }

                Console.WriteLine("Inserting {0} orders into database...", orders.Count);
                var repo = new OrderRepository();

                // iterate to orders and add it to database
                foreach (var order in orders)
                {
                    // insert or update first the order
                    repo.DoInsertOrupdateOrder(order);

                    // then, its order items
                    foreach (var orderItem in order.OrderItems)
                        repo.DoInsertOrUpdateOrderItem(orderItem);                    
                }

                Console.WriteLine("Done inserting {0} orders into database.", orders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.OrderService,
                    string.Format("Error occured in inserting order item to DB <br/>Error message: {0}",
                    EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
            }
            
        }

        private async void DeleteCancelledOrders(List<MarketplaceOrder> ordersResults)
        {
            var shipStationService = new ShipStationService();
            var orderId = "";
            var marketPlace = "";

            try
            {
                ordersResults = ordersResults.Where(o => o.OrderStatus == OrderStatus.Canceled).ToList();

                if (ordersResults.Count > 0)
                {
                    marketPlace = ordersResults.FirstOrDefault().Marketplace;

                    foreach (var order in ordersResults)
                    {
                        orderId = order.OrderId;

                        var shipStationTask = await shipStationService.DeleteOrderByOrderNumber(orderId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.General,
                    string.Format("Error in deleting orders for {1} to ShipStation. Order ID: {2},  Error message: {0}",
                                EisHelper.GetExceptionMessage(ex), 
                                marketPlace,
                                orderId),
                    ex.StackTrace);

                _emailService.SendEmailAdminException(subject: string.Format("{0} - Delete ShipStation Order From Marketplaces Error", marketPlace),
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "DeleteCancelledOrders Method",
                                                        userName: string.Format("OrdersService , Order Id: {0}", orderId));
            }
        }

        internal class OrdersServiceWorker
        {
            private readonly IMarketplaceOrdersService _ordersService;
            private readonly LoggerRepository _logger;

            public OrdersServiceWorker(IMarketplaceOrdersService ordersService, LoggerRepository logger)
            {
                _ordersService = ordersService;
                _logger = logger;
            }

            public IEnumerable<MarketplaceOrder> GetMarketplaceOrders(DateTime createdAfter)
            {
                try
                {
                    return _ordersService.GetMarketplaceOrders(createdAfter);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error Message: " + EisHelper.GetExceptionMessage(ex) + "\n" + ex.StackTrace);
                    _logger.LogError(LogEntryType.OrderService,
                        string.Format("Error occured in fetching order for {0} <br/>Error message: {1}",
                        _ordersService.ChannelName, EisHelper.GetExceptionMessage(ex)),
                        ex.StackTrace);
                    return null;
                }
            }
        }
    }
}
