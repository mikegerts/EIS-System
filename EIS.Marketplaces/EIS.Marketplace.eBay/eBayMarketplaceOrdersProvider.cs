using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Marketplace.eBay
{
    [Export(typeof(IMarketplaceOrdersProvider))]
    public class eBayMarketplaceOrdersProvider : IMarketplaceOrdersProvider
    {
        private eBayCredentialDto _credential;
        private ApiCredential _apiCredential;
        private ILogService _logger;
        private readonly string _apiServiceUrl = "https://api.ebay.com/wsapi";

        public eBayMarketplaceOrdersProvider()
        {
            _logger = Core.Get<ILogService>();
        }

        public string ChannelName
        {
            get { return "eBay"; }
        }

        public CredentialDto MarketplaceCredential
        {
            get { return _credential; }
            set
            {
                _credential = value as eBayCredentialDto;

                // let's init and set the eBay API credentials
                _apiCredential = new ApiCredential();
                _apiCredential.ApiAccount = new ApiAccount
                {
                    Application = _credential.ApplicationId,
                    Developer = _credential.DeveloperId,
                    Certificate = _credential.CertificationId
                };
                _apiCredential.eBayToken = _credential.UserToken;
            }
        }

        public MarketplaceOrder GetMarketplaceOrder(string orderId)
        {
            var results = getMarketplaceOrdersData(new List<string> { orderId });

            return results == null ? null : results.FirstOrDefault();
        }

        public List<MarketplaceOrder> GetMarketplaceOrders(List<string> orderIds)
        {
            return getMarketplaceOrdersData(orderIds);
        }

        public bool ConfirmOrderShimpmentDetails(MarketplaceOrderFulfillment orderFulfillment, string submittedBy)
        {
            throw new NotImplementedException();
        }

        public List<Carrier> GetShippingCarriers()
        {
            throw new NotImplementedException();
        }

        public bool CancelOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        public bool UnshippedOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        private List<MarketplaceOrder> getMarketplaceOrdersData(List<string> orderIds)
        {
            var context = new ApiContext();
            var results = new List<MarketplaceOrder>();

            try
            {
                // let's set the credentials for our eBay API service
                context.ApiCredential = _apiCredential;
                context.SoapApiServerUrl = _apiServiceUrl;

                // init the GetOrdersCall
                var apiCall = new GetOrdersCall(context);
                apiCall.DetailLevelList = new DetailLevelCodeTypeCollection();
                apiCall.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);
                apiCall.OrderRole = TradingRoleCodeType.Seller;
                apiCall.OrderStatus = OrderStatusCodeType.Completed;
                apiCall.Site = SiteCodeType.US;
                apiCall.OrderIDList = new StringCollection(orderIds.ToArray());

                apiCall.Execute();

                // check if the call is not success
                if (apiCall.ApiResponse.Ack != AckCodeType.Success)
                {
                    _logger.Add(LogEntrySeverity.Error, LogEntryType.eBayOrders,
                        string.Format("{0} - Error in retrieving orders for eBay. Error message: {1}", apiCall.ApiResponse.Errors[0].LongMessage),
                        apiCall.ApiResponse.Errors.ToString());
                    return null;
                }

                var ordersResult = apiCall.ApiResponse.OrderArray;
                if (ordersResult.Count == 0)
                    return null;

                // parsed the orders results
                foreach(OrderType item in ordersResult)
                    results.Add(parsedMarketplaceOrder(item));

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonOrdersProvider,
                     string.Format("{0} - Error in getting latest order data for Order Id: {1}. Error Message: {2}", ChannelName, string.Join(",", orderIds), EisHelper.GetExceptionMessage(ex)),
                     ex.StackTrace);
                return null;
            }
        }

        private MarketplaceOrder parsedMarketplaceOrder(OrderType orderType)
        {
            var order = new MarketplaceOrder();
            order.SellerOrderId = orderType.OrderID; // eBay OrderId is the SalesRecordNumber
            order.Marketplace = ChannelName;
            order.OrderTotal = (decimal)orderType.Total.Value;
            order.NumOfItemsShipped = 0; // let's init to 0 and reevaluate the actual value in transaction
                                         //NumOfItemsUnshipped =1,
            order.OrderStatus = parseOrderStatus(orderType.OrderStatus);
            order.PurchaseDate = orderType.CreatedTime;
            order.LastUpdateDate = orderType.PaidTime;

            // get the shipping details
            var address = orderType.ShippingAddress;
            if (address != null)
            {
                order.ShippingAddressPhone = address.Phone;
                order.ShippingAddressName = address.Name;
                order.ShippingAddressLine1 = address.Street1;
                order.ShippingAddressLine2 = address.Street2;
                order.ShippingAddressLine3 = address.Street; // this is correct
                order.ShippingCity = address.CityName;
                order.ShippingStateOrRegion = address.StateOrProvince;
                order.ShippingPostalCode = address.PostalCode;
            }

            // get th min and max of shipping date and shipping details
            if (orderType.ShippingDetails != null && orderType.ShippingDetails.ShippingServiceOptions != null)
            {
                // get the sales record number which is the OrderId in our database for eBay
                order.OrderId = orderType.ShippingDetails.SellingManagerSalesRecordNumber.ToString();

                var shippingOption = orderType.ShippingDetails.ShippingServiceOptions;
                var createdDate = orderType.CreatedTime;
                order.EarliestShipDate = createdDate.AddDays(shippingOption[0].ShippingTimeMin);
                order.LatestShipDate = createdDate.AddDays(shippingOption[0].ShippingTimeMax);

                // set the order shipping service
                order.ShipServiceLevel = shippingOption[0].ShippingService;
                order.ShipmentServiceCategory = shippingOption[0].ShippingServicePriority.ToString();
            }

            // set the order's payment method used
            if (orderType.CheckoutStatus.PaymentMethodSpecified)
            {
                order.PaymentMethod = orderType.CheckoutStatus.PaymentMethod.ToString();
            }

            // get the order transactions
            var orderItems = new List<MarketplaceOrderItem>();
            var orderTransactions = orderType.TransactionArray;
            foreach (TransactionType tran in orderTransactions)
            {
                var orderItem = new MarketplaceOrderItem
                {
                    OrderId = order.OrderId,
                    MarketplaceItemId = tran.Item.ItemID,
                    OrderItemId = tran.TransactionID,
                    SKU = tran.Item.SKU,
                    Title = tran.Item.Title,
                    QtyOrdered = tran.QuantityPurchased,
                    ConditionNote = tran.Item.ConditionDisplayName
                };

                // if the shipped time specified means it's already shipped?
                if (orderType.ShippedTimeSpecified)
                {
                    orderItem.QtyShipped = tran.QuantityPurchased;
                    order.NumOfItemsShipped += orderItem.QtyShipped;
                }

                orderItem.Price = (decimal)tran.TransactionPrice.Value;
                orderItem.ShippingPrice = (decimal)tran.ActualShippingCost.Value;
                orderItem.GiftWrapPrice = (decimal)tran.ActualHandlingCost.Value;
                orderItem.Tax = (decimal)tran.Taxes.TotalTaxAmount.Value;
                //orderItem.ShippingTax = orderItem.IsSetShippingTax() ? decimal.Parse(orderItem.ShippingTax.Amount) : 0;
                //orderItem.GiftWrapTax = orderItem.IsSetGiftWrapTax() ? decimal.Parse(orderItem.GiftWrapTax.Amount) : 0;
                //orderItem.ShippingDiscount = orderItem.IsSetShippingDiscount() ? decimal.Parse(orderItem.ShippingDiscount.Amount) : 0;
                //orderItem.PromotionDiscount = orderItem.IsSetPromotionDiscount() ? decimal.Parse(orderItem.PromotionDiscount.Amount) : 0;

                orderItems.Add(orderItem);

                // set the order's buyer information
                order.BuyerName = string.Format("{0} {1}", tran.Buyer.UserFirstName, tran.Buyer.UserLastName);
                order.BuyerEmail = tran.Buyer.Email;
                order.MarketplaceId = tran.TransactionSiteID.ToString();
                order.SalesChannel = tran.Platform.ToString();
            }

            // get the adjustment and also the payment or refund payment
            order.AmountPaid = (decimal)orderType.AmountPaid.Value;
            order.AdjustmentAmount = (decimal)orderType.AdjustmentAmount.Value;
            foreach (ExternalTransactionType extTran in orderType.ExternalTransaction)
            {
                order.PaymentOrRefundAmount += (decimal)extTran.PaymentOrRefundAmount.Value;
            }

            // set the order items for this order
            order.OrderItems = orderItems;

            return order;
        }

        private OrderStatus parseOrderStatus(OrderStatusCodeType orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatusCodeType.Active: return OrderStatus.Pending;
                case OrderStatusCodeType.Inactive: return OrderStatus.PendingAvailability;
                case OrderStatusCodeType.Completed: return OrderStatus.Shipped;
                case OrderStatusCodeType.Cancelled: return OrderStatus.Canceled;
                case OrderStatusCodeType.Shipped: return OrderStatus.Shipped;
                case OrderStatusCodeType.InProcess: return OrderStatus.PendingAvailability;
                case OrderStatusCodeType.CancelPending: return OrderStatus.Canceled;
                default: return OrderStatus.None;
            }
        }
    }
}
