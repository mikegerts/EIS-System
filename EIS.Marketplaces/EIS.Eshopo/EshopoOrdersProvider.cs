using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using EIS.Eshopo.Models;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Eshopo
{
    [Export(typeof(IMarketplaceOrdersProvider))]
    public class EshopoOrdersProvider : IMarketplaceOrdersProvider
    {
        private string _connectionString;
        private CredentialDto _credential;
        private readonly ILogService _logger;
        private readonly IOrderService _orderService;
        private readonly EisInventoryContext _context;
        private EmailNotificationService _emailService;

        public EshopoOrdersProvider ()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
            _logger = Core.Get<ILogService>();
            _orderService = Core.Get<IOrderService>();
            _context = new EisInventoryContext();
            _emailService = new EmailNotificationService();
        }

        public string ChannelName
        {
            get { return "Eshopo"; }
        }

        public CredentialDto MarketplaceCredential
        {
            get { return _credential; }
            set { _credential = value; }
        }

        public MarketplaceOrder GetMarketplaceOrder ( string orderId )
        {
            return null;
        }

        public List<MarketplaceOrder> GetMarketplaceOrders(List<string> orderIds)
        {
            return null;
        }

        public bool ConfirmOrderShimpmentDetails ( MarketplaceOrderFulfillment orderFulfillment, string submittedBy )
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@OrderId", orderFulfillment.OrderId},
                    {"@OrderStatus", (int)OrderStatus.Shipped},
                };

                // let's update first the order items QtyShipped, must be be equal to Qty Ordered
                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE orderitems SET QtyShipped = QtyOrdered WHERE OrderId=@OrderId",
                    parameters);

                // then let's update the orders status
                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE orders SET NumOfItemsShipped=NumOfItemsUnshipped, NumOfItemsUnshipped=0,ShipServiceLevel='Standard',ShipmentServiceCategory='Standard',OrderStatus=@OrderStatus WHERE OrderId=@OrderId",
                    parameters);
            }

            return true;
        }

        public List<Carrier> GetShippingCarriers ()
        {
            return EnumHelper.GetList<CarrierCode>()
                .Select(x => new Carrier
                {
                    Code = x.ToString(),
                    Name = x.GetStringValue()
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public bool CancelOrder(string orderId)
        {
            var order = _context.orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return true;

            // iterate to each order items and set its qty ordered
            foreach (var orderItem in order.orderitems)
            {
                orderItem.QtyOrdered = 0;
                orderItem.QtyShipped = 0;

                // get all the order products for this order and delete and return the quantity
                var orderProducts = orderItem.orderproducts.ToList();
                if (orderProducts.Any())
                {
                    _orderService.UpdateVendorProductInventory(orderProducts, true);

                    // delete the order products for this item
                    _context.orderproducts.RemoveRange(orderProducts);
                    _context.SaveChanges();
                }
            }

            // update also some properties of the order
            order.NumOfItemsShipped = 0;
            order.NumOfItemsUnshipped = 0;
            order.OrderStatus = OrderStatus.Canceled;

            // delete order in shipstation (soft-delete)
            DeleteShipStationOrder(orderId);

            _context.SaveChanges();

            return true;
        }

        public bool UnshippedOrder ( string orderId )
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@OrderId", orderId},
                    {"@OrderStatus", (int)OrderStatus.Unshipped},
                };

                // let's update first the order items QtyShipped, must be be equal to Qty Ordered
                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE orderitems SET QtyShipped = 0 WHERE OrderId=@OrderId",
                    parameters);

                // then let's update the orders status
                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE orders SET NumOfItemsUnshipped=NumOfItemsShipped,NumOfItemsShipped=0,ShipServiceLevel='',ShipmentServiceCategory='',OrderStatus=@OrderStatus WHERE OrderId=@OrderId",
                    parameters);
            }

            return true;
        }

        public void DeleteShipStationOrder ( string orderId )
        {
            //delete order from ship station
            try
            {
                var shipStationService = new ShipStationService();
                var shipStationTask = shipStationService.DeleteOrderByOrderNumber(orderId);
                shipStationTask.Wait();
            }
            catch (Exception ex)
            {
                var description = string.Format("{0} - Error in submitting delete order feed to Shipstation with OrderId: {1}. \nError Message: {2}",
                        ChannelName,
                        orderId,
                        (ex.InnerException != null ? string.Format("{0} <br/>Inner Message: {1}", ex.Message, ex.InnerException.Message) : ex.Message));
                _logger.Add(LogEntrySeverity.Error, LogEntryType.EshopoOrders, description, ex.StackTrace);
                
                _emailService.SendEmailAdminException(subject: "Eshopo - Delete ShipStation Order Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "/orders",
                                                        userName: String.Format("Order Id: {0}", orderId));
            }
        }
    }
}
