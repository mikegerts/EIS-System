using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EIS.OrdersServiceApp.Models;
using BigCommerce4Net.Api;
using BigCommerce4Net.Domain;
using EIS.OrdersServiceApp.Repositories;
using EIS.OrdersServiceApp.Properties;
using EIS.Inventory.Shared.Models;
using Newtonsoft.Json;
using BigCommerce4Net.Domain.Entities.Orders;
using System.ComponentModel.Composition;

namespace EIS.OrdersServiceApp.Marketplaces
{
    [Export(typeof(IMarketplaceOrdersService))]
    public class BigCommerceOrdersService : IMarketplaceOrdersService
    {
        private Configuration _apiConfiguration;
        private Client _client;
        private BigCommerceCredential _credential;
        private LoggerRepository _logger;
        private OrderRepository _orderRepository;

        public BigCommerceOrdersService ()
        {
            _logger = new LoggerRepository();
            _orderRepository = new OrderRepository();
        }
        
        #region Public Methods

        public string ChannelName
        {
            get { return "BigCommerce"; }
        }

        public Credential Credential
        {
            get { return _credential; }
            set {
                _credential = value as Models.BigCommerceCredential;

                // Apply Configuration once credentials are set
                _apiConfiguration = new Configuration()
                {
                    ServiceURL = _credential.ServiceEndPoint,
                    UserName = _credential.Username,
                    UserApiKey = _credential.ApiKey,
                    MaxPageLimit = 250,
                    AllowDeletions = true // Is false by default, must be true to allow deletions
                };

                _client = new Client(_apiConfiguration);
            }
        }

        public bool ConfirmOrdersShipment ()
        {
            // get the unshipped orders with tracking number for confirming its shipment
            var unshippedOrderFeeds = _orderRepository.GetUnshippedOrdersForShipment(ChannelName);
            var bigCommerceOrdersList = new List<BigCommerce4Net.Domain.Order>();
            var successConfirms = 0;

            if (!unshippedOrderFeeds.Any())
            {
                Console.WriteLine("No unshipped orders found from {0} for shipment confirmation.", ChannelName);
                return true;
            }

            try
            {
                Console.WriteLine("Sending {0} orders for shipment confirmation...", unshippedOrderFeeds.Count);
                
                foreach(var marketplaceOrder in unshippedOrderFeeds)
                {
                    if (!marketplaceOrder.OrderItems.Any())
                        return false;

                    var orderid = Convert.ToInt32(marketplaceOrder.OrderId);
                    var bcOrderResponse = _client.Orders.Get(orderid);
                    var bcOrder = bcOrderResponse.Data;

                    var updatedata = new
                    {
                        status_id = OrderStatusEnum.Shipped
                    };

                    // Create BC Order Shipment        
                    var shipmentItems = new List<OrdersShipmentItem>();

                    foreach (var orderitems in marketplaceOrder.OrderItems)
                    {
                        var orderItemId = Convert.ToInt32(orderitems.OrderItemId);

                        shipmentItems.Add(new OrdersShipmentItem { OrderProductId = orderItemId, Quantity = orderitems.Quantity });
                    }

                    var orderAddressID = 0;
                    var shippingAddresses = GetShippingAddresses(bcOrder.Id);

                    if (shippingAddresses.Count > 0)
                    {
                        orderAddressID = shippingAddresses.First().Id;
                    }

                    var shipmentdata = new
                    {
                        order_address_id = orderAddressID,
                        items = shipmentItems,
                        tracking_number = marketplaceOrder.ShipperTrackingNumber,
                        shipping_method = marketplaceOrder.ShippingMethod,
                        shipping_provider = marketplaceOrder.CarrierCode != null ? marketplaceOrder.CarrierCode : "",
                    };

                    // API Create Shipment based on Order
                    var responseShipment = _client.OrdersShipments.Create(orderid, shipmentdata);

                    if(responseShipment.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                    {
                        // API Update on the Order
                        var responseOrder = _client.Orders.Update(orderid, updatedata);

                        if (responseOrder.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            successConfirms++;

                            var tempList = new List<MarketplaceOrderFulfillment>();
                            tempList.Add(marketplaceOrder);

                            // let's add these orders to the shipment history
                            addOrderShipmentHistoryAsync(tempList);
                        }
                    }

                }
                
                _logger.LogInfo(LogEntryType.BigCommerceOrders, 
                    string.Format("Successfully sent confirming order shipment for {0} order items: <br/>Shipment confirmation was done by {1}", successConfirms, Constants.APP_NAME));
                Console.WriteLine("Successfully sent confirming order shipment for {0} order items: ", successConfirms);

                return true;
            }
            catch (Exception ex)
            {

                _logger.LogError(LogEntryType.BigCommerceOrders,
                    string.Format("{4} Error in confirming order shipment for {0} - Number of Orders: {3}. <br/>Error Message: {1} <br/> Access Key: {2}", ChannelName,
                    (ex.InnerException != null ? string.Format("{0} <br/>Inner Message: {1}",
                        ex.Message, ex.InnerException.Message) : ex.Message),
                        _credential.Username,
                        bigCommerceOrdersList.Count,
                        _ApplicationName),
                    ex.StackTrace);

                Console.WriteLine("Error in sending shipment confirmation! Error Message: {0} \nStackTrace: {1} ", ex.Message, ex.StackTrace);

                // let's delete the order confirmation
                deleteOrderShipmentHistoryAsync(unshippedOrderFeeds);

                return false;
            }
        }

        public IEnumerable<MarketplaceOrder> GetMarketplaceOrders ( DateTime createdAfter )
        {
            var marketplaceOrders = new List<MarketplaceOrder>();
            var filter = new FilterOrders
            {
                MinimumDateCreated = createdAfter.Date
            };

            Console.WriteLine("{0} fetching orders for {1}...", ChannelName, createdAfter);

            //API Call to get all Orders from the BigCommerce - paged by 250
            // apply paging if record per day is more than 250
            var bigCommerceOrders = GetBigCommerceOrders(filter);


            foreach (var item in bigCommerceOrders)
            {
                marketplaceOrders.Add(convertOrderResponseToMarketplaceOrder(item));
            }


            return marketplaceOrders;
        }
        
        public List<Category> GetCategoryList()
        {
            var categoryList = new List<Category>();

            categoryList = _client.Categories.GetList().ToList();

            return categoryList;
        }

        public List<OrdersProduct> GetOrdersProduct(int orderId)
        {
            var ordersProductList = new List<OrdersProduct>();

            var response = _client.OrdersProducts.Get(orderId);

            if(response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ordersProductList = response.Data;
            }

            return ordersProductList;
        }

        public List<OrdersShippingAddress> GetShippingAddresses(int orderId)
        {
            var shippingAddressesList = new List<OrdersShippingAddress>();

            var response = _client.OrdersShippingAddresses.Get(orderId);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                shippingAddressesList = response.Data;
            }

            return shippingAddressesList;
        }
        
        public List<BigCommerce4Net.Domain.Order> GetBigCommerceOrders(FilterOrders filter)
        {
            //API Call to get all Orders from the BigCommerce - paged by 250
            // apply paging if record per day is more than 250
            var bigCommerceOrders = _client.Orders.GetList(filter);

            return bigCommerceOrders.ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts the response object into MarketplaceOrder object.
        /// Also makes the call to get the items for the order.
        /// </summary>
        /// <param name="order">The given Amazon order object.</param>
        /// <returns>A MarketplaceOrder object</returns>
        private MarketplaceOrder convertOrderResponseToMarketplaceOrder ( BigCommerce4Net.Domain.Order order )
        {
            var marketplaceOrder = new MarketplaceOrder()
            {
                BuyerEmail = order.BillingAddress.Email,
                BuyerName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName),
                OrderTotal = order.SubtotalIncludingTax,
                OrderId = order.Id.ToString(),
                Marketplace = ChannelName,
                ShipServiceLevel = "Standard",
                ShipmentServiceCategory = "Standard",
                NumOfItemsShipped = order.ItemsShipped,
                NumOfItemsUnshipped = order.ItemsTotal - order.ItemsShipped,
                PurchaseDate = order.DateCreated.HasValue ? order.DateCreated.Value : DateTime.Now,
                LastUpdateDate = order.DateModified.HasValue ? order.DateModified.Value : DateTime.Now,
                PaymentMethod = order.PaymentMethod,
                OrderStatus = MapEISOrderStatusBigCommerce(order.StatusId),
                OrderType = order.OrderIsDigital ? "DigitalOrder" : "StandardOrder",
                //EarliestShipDate = order.EarliestShipDate,
                //LatestShipDate = order.LatestShipDate,
                //EarliestDeliveryDate = order.EarliestDeliveryDate,
                //LatestDeliveryDate = order.LatestDeliveryDate,
                //MarketplaceId = order.MarketplaceId,
                //PurchaseOrderNumber = order.PurchaseOrderNumber,
                //SalesChannel = order.SalesChannel,
                //SellerOrderId = order.SellerOrderId,
            };

            var shippingAddresses = GetShippingAddresses(order.Id);

            if (order.ShippingAddressCount > 0 && shippingAddresses.Count > 0)
            {
                marketplaceOrder.ShippingAddressLine1 = shippingAddresses.First().Street1;
                marketplaceOrder.ShippingAddressLine2 = shippingAddresses.First().Street2;
                marketplaceOrder.ShippingAddressLine3 = "";
                marketplaceOrder.ShippingCity = shippingAddresses.First().City;
                marketplaceOrder.ShippingStateOrRegion = shippingAddresses.First().State;
                marketplaceOrder.ShippingPostalCode = shippingAddresses.First().ZipCode;
                marketplaceOrder.ShippingAddressName = string.Format("{0} {1}", shippingAddresses.First().FirstName, shippingAddresses.First().LastName);
                marketplaceOrder.ShippingAddressPhone = shippingAddresses.First().Phone;
                marketplaceOrder.CompanyName = shippingAddresses.First().Company;
            }
            else
            {
                marketplaceOrder.ShippingAddressLine1 = order.BillingAddress.Street1;
                marketplaceOrder.ShippingAddressLine2 = order.BillingAddress.Street2;
                marketplaceOrder.ShippingAddressLine3 = "";
                marketplaceOrder.ShippingCity = order.BillingAddress.City;
                marketplaceOrder.ShippingStateOrRegion = order.BillingAddress.State;
                marketplaceOrder.ShippingPostalCode = order.BillingAddress.ZipCode;
                marketplaceOrder.ShippingAddressName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
                marketplaceOrder.ShippingAddressPhone = order.BillingAddress.Phone;
                marketplaceOrder.CompanyName = order.BillingAddress.Company;
            }

            // Add Order Items
            var orderItems = new List<MarketplaceOrderItem>();

            var orderProducts = GetOrdersProduct(order.Id);

            foreach (var oproduct in orderProducts)
            {
                orderItems.Add(convertOrderItemResponseToMarketplaceOrderItem(oproduct));
            }
            marketplaceOrder.OrderItems = orderItems;

            return marketplaceOrder;
        }

        private decimal GetTotalOrderPrice ( List<OrdersProduct> orderProducts )
        {
            decimal totalPrice = 0;

            foreach (var item in orderProducts)
            {
                totalPrice += item.BaseTotal;
            }

            return totalPrice;
        }


        private Inventory.Shared.Models.OrderStatus MapEISOrderStatusBigCommerce ( int bigCommerceOrderStatusId )
        {
            var retStatus = Inventory.Shared.Models.OrderStatus.None;

            switch (bigCommerceOrderStatusId)
            {
                case 1:  // BigCommerce API - Pending
                    retStatus = Inventory.Shared.Models.OrderStatus.Pending;
                    break;
                case 2:  // BigCommerce API - Shipped
                    retStatus = Inventory.Shared.Models.OrderStatus.Shipped;
                    break;
                case 3:  // BigCommerce API - Partially Shipped
                    retStatus = Inventory.Shared.Models.OrderStatus.PartiallyShipped;
                    break;
                case 4:  // BigCommerce API - Refunded
                case 5:  // BigCommerce API - Cancelled
                    retStatus = Inventory.Shared.Models.OrderStatus.Canceled;
                    break;
                case 7:  // BigCommerce API - Awaiting Payment
                    retStatus = Inventory.Shared.Models.OrderStatus.InvoiceUnconfirmed;
                    break;
                case 8:  // BigCommerce API - Awaiting Pickup
                case 9:  // BigCommerce API - Awaiting Shipment
                case 10: // BigCommerce API - Completed
                case 11: // BigCommerce API - Awaiting Fulfillment
                case 12: // BigCommerce API - Manual Verification Required
                    retStatus = Inventory.Shared.Models.OrderStatus.Unshipped;
                    break;
                case 6:  // BigCommerce API - Declined
                case 13: // BigCommerce API - Disputed
                    retStatus = Inventory.Shared.Models.OrderStatus.Unfulfillable;
                    break;
                default:
                    retStatus = Inventory.Shared.Models.OrderStatus.None;
                    break;
            }

            return retStatus;
        }



        /// <summary>
        /// Converts the response object into MarketplaceOrderItem object.
        /// </summary>
        /// <param name="orderorderItem">The given BigCommerce order product object.</param>
        /// <returns>A MarketplaceOrderItem object</returns>
        private MarketplaceOrderItem convertOrderItemResponseToMarketplaceOrderItem(BigCommerce4Net.Domain.OrdersProduct orderItem)
        {
            var marketplaceOrderItem = new MarketplaceOrderItem()
            {
                OrderId = orderItem.OrderId.ToString(),
                MarketplaceItemId = orderItem.ProductId.ToString(),
                OrderItemId = orderItem.Id.ToString(),
                SKU = orderItem.Sku,
                Title = orderItem.ProductName,
                QtyOrdered = Convert.ToInt32(orderItem.Quantity),
                QtyShipped = Convert.ToInt32(orderItem.QuantityShipped)
            };

            marketplaceOrderItem.Price = orderItem.BasePrice;
            marketplaceOrderItem.ShippingPrice = orderItem.FixedShippingCost;
            marketplaceOrderItem.GiftWrapPrice = orderItem.BaseWrappingCost;
            marketplaceOrderItem.Tax = orderItem.PriceTax;
            marketplaceOrderItem.GiftWrapTax = orderItem.WrappingCostTax;
            //marketplaceOrderItem.ShippingDiscount = orderItem.;
            marketplaceOrderItem.PromotionDiscount = orderItem.DiscountAmount;

            return marketplaceOrderItem;
        }

        #endregion

        #region helper properties call to BigCommerce Settings value
        private string _ApplicationName
        {
            get { return Settings.Default.ApplicationName; }
        }
        private string _Version
        {
            get { return Settings.Default.Version; }
        }
        #endregion

        //need to transfer to a common Helper class (duplicate with Amazon Order Manager class
        #region Helpers
        private Task addOrderShipmentHistoryAsync ( List<MarketplaceOrderFulfillment> orderFeeds )
        {
            Task.Run(() =>
            {
                Console.WriteLine("Inserting order shipment histories...");
                var records = _orderRepository.InsertOrderShipmentHistory(orderFeeds);
                Console.WriteLine("Done inserting order shipment histories: {0} items", orderFeeds.Count);
            });
            return Task.FromResult(true);
        }
        

        private Task deleteOrderShipmentHistoryAsync ( List<MarketplaceOrderFulfillment> orderFeeds )
        {
            Task.Run(() =>
            {
                Console.WriteLine("Deleting order shipment histories...");
                var records = _orderRepository.DeleteOrderShipmentHistory(orderFeeds);
                Console.WriteLine("Done deleting order shipment histories: {0} items", orderFeeds.Count);
            });
            return Task.FromResult(true);
        }
        #endregion
    }
}
