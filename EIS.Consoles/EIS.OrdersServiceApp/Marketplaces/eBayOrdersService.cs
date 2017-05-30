using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.OrdersServiceApp.Models;
using EIS.OrdersServiceApp.Repositories;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.Services;

namespace EIS.OrdersServiceApp.Marketplaces
{
    [Export(typeof(IMarketplaceOrdersService))]
    public class eBayOrdersService : IMarketplaceOrdersService
    {
         private readonly string _apiServiceUrl = "https://api.ebay.com/wsapi";
         private eBayCredential _credential;
         private ApiContext _context;
         private ApiCredential _apiCredential;
         private readonly LoggerRepository _logger;
        private EmailNotificationService _emailService;

        public eBayOrdersService()
         {
             _context = new ApiContext();
             _logger = new LoggerRepository();
            _emailService = new EmailNotificationService();
         }

        public string ChannelName
        {
            get { return "eBay"; }
        }

        public Models.Credential Credential
        {
            set { 
                _credential = value as Models.eBayCredential;
                
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

        //https://ebaydts.com/eBayKBDetails?KBid=1679
        public IEnumerable<MarketplaceOrder> GetMarketplaceOrders(DateTime createdAfter)
        {
            var orderResults = new List<MarketplaceOrder>();
            try
            {
                // create the API call for get orders, then execute
                var apiCall = createGetOrdersCall(createdAfter);
                apiCall.Execute();

                // check if the call is not success
                if (apiCall.ApiResponse.Ack != AckCodeType.Success)
                {
                    Console.WriteLine("Error in getting orders for {0}! Error message: {1}", ChannelName, apiCall.ApiResponse.Errors[0].LongMessage);
                    _logger.LogError(LogEntryType.eBayOrders,
                        string.Format("Error in retrieving orders for eBay. Error message: {0}", apiCall.ApiResponse.Errors[0].LongMessage),
                        apiCall.ApiResponse.Errors.ToString());
                    return null;
                }

                // check if there are order items
                var orders = apiCall.ApiResponse.OrderArray;
                Console.WriteLine("{0} retrieved {1} orders.", ChannelName, orders.Count);
                if (orders.Count == 0)
                    return null;

                // iterate and parse the order results
                foreach (OrderType item in orders)
                {
                    orderResults.Add(parsedMarketplaceOrder(item));
                }

                Console.WriteLine("{0} done fetching orders: {1} items", ChannelName, orders.Count);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in getting orders for {0}! Error message: {1}", ChannelName, ex.Message);
                _logger.LogError(LogEntryType.eBayOrders,
                    string.Format("Error in retrieving orders for eBay. Error message: {0}", ex.Message),
                    ex.StackTrace);

                _emailService.SendEmailAdminException(subject: "eBay - Get Marketplace Orders Error", 
                                                        exParam: ex, 
                                                        useDefaultTemplate: true, 
                                                        url: "GetMarketplaceOrders Method", 
                                                        userName:"OrdersService");
            }

            return orderResults;
        }        

        public bool ConfirmOrdersShipment()
        {
            return true;
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

                // set the Order Status to Shipped if it has shipping tracking details
                if(orderType.TransactionArray[0].ShippingDetails.ShipmentTrackingDetails.Count > 0
                    && !string.IsNullOrEmpty(orderType.TransactionArray[0].ShippingDetails.ShipmentTrackingDetails[0].ShipmentTrackingNumber))
                {
                    order.OrderStatus = OrderStatus.Shipped;
                }
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

                    if(tran.ActualShippingCost != null)
                    {
                        orderItem.ShippingPrice = (decimal)tran.ActualShippingCost.Value;
                    }

                    if(tran.ActualHandlingCost != null)
                    {
                        orderItem.GiftWrapPrice = (decimal)tran.ActualHandlingCost.Value;
                    }
                }

                orderItem.Price = (decimal)tran.TransactionPrice.Value;
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

        private GetOrdersCall createGetOrdersCall(DateTime createdAfter)
        {
            // let's set the credentials for our eBay API service
            _context.ApiCredential = _apiCredential;
            _context.SoapApiServerUrl = _apiServiceUrl;

            // we will use the NumberOfDays filter for retrieving the order
            //var numberOfDays = (int)(DateTime.UtcNow - createdAfter).TotalDays;

            // init the GetOrdersCall
            var apiCall = new GetOrdersCall(_context);
            apiCall.DetailLevelList = new DetailLevelCodeTypeCollection();
            apiCall.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);
            apiCall.NumberOfDays = 30; // just make it 30 days
            apiCall.OrderRole = TradingRoleCodeType.Seller;            
            apiCall.Site = SiteCodeType.US;

            return apiCall;
        }

        private OrderStatus parseOrderStatus(OrderStatusCodeType orderStatus)
        {
            switch (orderStatus)
            {
                case OrderStatusCodeType.Active: return OrderStatus.Pending;
                case OrderStatusCodeType.Inactive: return OrderStatus.PendingAvailability;                
                case OrderStatusCodeType.Completed: return OrderStatus.Unshipped; // need to verify
                case OrderStatusCodeType.Cancelled: return OrderStatus.Canceled;
                case OrderStatusCodeType.Shipped: return OrderStatus.Shipped;
                case OrderStatusCodeType.InProcess: return OrderStatus.PendingAvailability;
                case OrderStatusCodeType.CancelPending: return OrderStatus.Canceled;
                default: return OrderStatus.None;
            }
        }
    }
}
