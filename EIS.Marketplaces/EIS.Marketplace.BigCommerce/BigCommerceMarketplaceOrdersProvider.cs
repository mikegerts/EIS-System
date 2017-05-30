using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigCommerce4Net.Api;
using BigCommerce4Net.Domain;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;
using BigCommerce4Net.Api.ResourceClients;
using BigCommerce4Net.Domain.Entities.Orders;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core;
using Newtonsoft.Json;
using EIS.Inventory.Shared.Helpers;
using System.ComponentModel.Composition;

namespace EIS.Marketplace.BigCommerce
{
    [Export(typeof(IMarketplaceOrdersProvider))]
    public class BigCommerceMarketplaceOrdersProvider : IMarketplaceOrdersProvider
    {

        private Configuration _apiConfiguration;
        private Client _client;
        private BigCommerceCredentialDto _credential;
        private ILogService _logger;

        public BigCommerceMarketplaceOrdersProvider()
        {
            _logger = Core.Get<ILogService>();
        }

        #region Public Methods

        public CredentialDto MarketplaceCredential
        {
            get { return _credential; }
            set
            {
                _credential = value as BigCommerceCredentialDto;

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

        public string ChannelName
        {
            get { return "BigCommerce"; }
        }

        public bool CancelOrder(string orderId)
        {
            return true;
        }

        public bool UnshippedOrder(string orderId)
        {
            return true;
        }

        public bool ConfirmOrderShimpmentDetails(MarketplaceOrderFulfillment marketplaceOrder, string submittedBy)
        {
            // TODO: need to determine what fields need to update to BigCommerce in order to confirm shipment

            try
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

                var isSuccess = false;

                // API Create Shipment based on Order
                var responseShipment = _client.OrdersShipments.Create(orderid, shipmentdata);

                if(responseShipment.RestResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    // API Update on the Order
                    var responseOrder = _client.Orders.Update(orderid, updatedata);

                    if (responseOrder.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.Add(LogEntrySeverity.Information, LogEntryType.BigCommerceOrders, string.Format("{0} - Successfully sent confirming order shipment for Order ID \'{1}\'", ChannelName, marketplaceOrder.OrderId));
                        isSuccess = true;
                    }
                } 

                return isSuccess;

            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.BigCommerceOrders,
                    string.Format("{0} - Error in confirming order shipment for OrderId: {2}. <br/>Error Message: {1} <br/>",
                                    ChannelName,
                                    EisHelper.GetExceptionMessage(ex),
                                    marketplaceOrder.OrderId),
                    ex.StackTrace);

                return false;
            }

        }

        public MarketplaceOrder GetMarketplaceOrder(string orderId)
        {
            return getMarketplaceOrderById(Convert.ToInt32(orderId));
        }

        public List<MarketplaceOrder> GetMarketplaceOrders(List<string> orderIds)
        {
            return getMarketplaceOrderListById(orderIds.Select(int.Parse).ToList());
        }

        public List<Carrier> GetShippingCarriers()
        {
            return EnumHelper.GetList<ShippingProviderEnum>()
                .Select(x => new Carrier
                {
                    Code = x.ToString(),
                    Name = x.GetStringValue()
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public string GetBCOrderStatus()
        {
            var retString = new StringBuilder();

            var response = _client.OrderStatuses.GetList();

            foreach (var data in response)
            {
                retString.AppendLine(string.Format("{0} - {1}", data.Id, data.Name));
            }

            return retString.ToString();
        }

        public List<OrdersProduct> GetOrdersProduct(int orderId)
        {
            var ordersProductList = new List<OrdersProduct>();

            var response = _client.OrdersProducts.Get(orderId);

            if (response.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
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

        #endregion

        #region Private Methods

        private MarketplaceOrder getMarketplaceOrderById(int orderId)
        {
            var marketplaceOrder = new MarketplaceOrder();

            var bcClientOrder = _client.Orders.Get(orderId);

            if (bcClientOrder.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                marketplaceOrder = convertOrderResponseToMarketplaceOrder(bcClientOrder.Data);
            }

            return marketplaceOrder;
        }

        private List<MarketplaceOrder> getMarketplaceOrderListById(List<int> orderIds)
        {
            var marketplaceOrders = new List<MarketplaceOrder>();

            foreach (var orderId in orderIds)
            {
                var bcClientOrder = _client.Orders.Get(orderId);
                var marketplaceOrder = new MarketplaceOrder();

                if (bcClientOrder.RestResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    marketplaceOrder = convertOrderResponseToMarketplaceOrder(bcClientOrder.Data);
                }

                marketplaceOrders.Add(marketplaceOrder);
            }

            return marketplaceOrders;
        }

        /// <summary>
        /// Converts the response object into MarketplaceOrder object.
        /// Also makes the call to get the items for the order.
        /// </summary>
        /// <param name="order">The given BigCommerce order object.</param>
        /// <returns>A MarketplaceOrder object</returns>
        private MarketplaceOrder convertOrderResponseToMarketplaceOrder(BigCommerce4Net.Domain.Order order)
        {
            var marketplaceOrder = new MarketplaceOrder()
            {
                BuyerEmail = order.BillingAddress.Email,
                BuyerName =  string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName),
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
                OrderStatus = getMarketPlaceOrderStatus(order.StatusId),
                OrderType = order.OrderIsDigital ? "DigitalOrder" : "StandardOrder",
                //EarliestShipDate = order.,
                //LatestShipDate = order.LatestShipDate,
                //EarliestDeliveryDate = order.EarliestDeliveryDate,
                //LatestDeliveryDate = order.LatestDeliveryDate,
                //MarketplaceId = order.,
                //PurchaseOrderNumber = order.,
                //SalesChannel = ,
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

        private Inventory.Shared.Models.OrderStatus getMarketPlaceOrderStatus(int statusid)
        {
            var retStatus = Inventory.Shared.Models.OrderStatus.None;
            
            switch (statusid)
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

        #endregion
    }
}
