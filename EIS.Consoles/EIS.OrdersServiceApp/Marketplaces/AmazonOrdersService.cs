using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AmazonWebServiceModels;
using EIS.OrdersServiceApp.Helpers;
using EIS.OrdersServiceApp.Models;
using EIS.OrdersServiceApp.Properties;
using EIS.OrdersServiceApp.Repositories;
using MarketplaceWebService;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.Model;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Core.Services;

namespace EIS.OrdersServiceApp.Marketplaces
{
    [Export(typeof(IMarketplaceOrdersService))]
    public class AmazonOrdersService : IMarketplaceOrdersService
    {
        private AmazonCredential _credential;
        private LoggerRepository _logger;
        private OrderRepository _orderRepository;
        private MarketplaceWebServiceOrdersClient _ordersClient;
        private EmailNotificationService _emailService;

        public AmazonOrdersService()
        {
            _logger = new LoggerRepository();
            _orderRepository = new OrderRepository();
            _emailService = new EmailNotificationService();
        }

        #region helper properties call to Amazon Settings value
        private string _ApplicationName
        {
            get { return Settings.Default.ApplicationName; }
        }
        private string _Version
        {
            get { return Settings.Default.Version; }
        }
        #endregion

        public string ChannelName
        {
            get { return "Amazon"; }
        }

        public Models.Credential Credential
        {
            get { return _credential; }
            set { _credential = value as Models.AmazonCredential; }
        }

        public IEnumerable<MarketplaceOrder> GetMarketplaceOrders(DateTime createdAfter)
        {
            var marketplaceOrders = new List<MarketplaceOrder>();

            try
            {
                // init the Amazon web service
                var config = new MarketplaceWebServiceOrdersConfig { ServiceURL = "https://mws.amazonservices.com" };
                config.SetUserAgent(_ApplicationName, _Version);
                _ordersClient = new MarketplaceWebServiceOrdersClient(_credential.AccessKeyId, _credential.SecretKey, config);

                Console.WriteLine("{0} fetching orders for {1}...", ChannelName, createdAfter);

                // create ListOrdersRequest object
                var listOrdersRequest = new ListOrdersRequest
                {
                    SellerId = _credential.MerchantId,
                    MarketplaceId = new List<string> { { _credential.MarketplaceId } },
                    LastUpdatedAfter = createdAfter.Date
                };
                var listOrdersResponse = _ordersClient.ListOrders(listOrdersRequest);
                var ordersResult = listOrdersResponse.ListOrdersResult.Orders;
                var nextToken = listOrdersResponse.ListOrdersResult.NextToken;
                ListOrdersByNextTokenResponse nextOrderResponse = null;

                Console.WriteLine("{0} retrieved {1} orders and {2} next results page.", ChannelName, listOrdersResponse.ListOrdersResult.Orders.Count, !string.IsNullOrWhiteSpace(nextToken) ? "HAS" : "NO");

                do
                {
                    if (nextOrderResponse != null)
                    {
                        ordersResult = nextOrderResponse.ListOrdersByNextTokenResult.Orders;
                        nextToken = nextOrderResponse.ListOrdersByNextTokenResult.NextToken;
                        Console.WriteLine("{0} retrieved {1} next orders and {2} next results page.", ChannelName, ordersResult.Count, !string.IsNullOrWhiteSpace(nextToken) ? "HAS" : "NO");
                    }

                    // Convert the orders to the marketplace contracts and get the items.
                    for (int i = 0; i < ordersResult.Count; ++i)
                    {
                        // The maximum request quota for ListOrderItems is 30 after that it restores at 2 per second. 
                        // So if the order's that are being pulled exceed 30, then sleep for 2 seconds each one.
                        if (i > 30)
                            Thread.Sleep(2000);

                        marketplaceOrders.Add(convertOrderResponseToMarketplaceOrder(ordersResult[i]));
                    }

                    // do a rquest for the next page result of orders if next token is not null
                    if (!string.IsNullOrWhiteSpace(nextToken))
                    {
                        // pause at least 1 minute, this is the restore rate for ListOrdersByNextToken for every 6 quota
                        Thread.Sleep(61000);

                        var nextTokenRequest = new ListOrdersByNextTokenRequest { SellerId = _credential.MerchantId, NextToken = nextToken };
                        nextOrderResponse = _ordersClient.ListOrdersByNextToken(nextTokenRequest);
                    }

                } while (!string.IsNullOrWhiteSpace(nextToken));

                Console.WriteLine("{0} done fetching orders: {1} items", ChannelName, marketplaceOrders.Count);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in getting orders for {0}! Error message: {1}", ChannelName, ex.Message);
                _logger.LogError(LogEntryType.AmazonOrdersProvider,
                    string.Format("Error in retrieving orders for Amazon. Error message: {0}", ex.Message),
                    ex.StackTrace);

                _emailService.SendEmailAdminException(subject: "Amazon - Get Marketplace Orders Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "GetMarketplaceOrders Method",
                                                        userName: "OrdersService");
            }

            return marketplaceOrders;
        }

        public bool ConfirmOrdersShipment()
        {
            // init the Amazon order API
            var config = new MarketplaceWebServiceConfig { ServiceURL = "https://mws.amazonservices.com" };
            config.SetUserAgentHeader(_ApplicationName, _Version, "C#");
            var serviceClient = new MarketplaceWebServiceClient(_credential.AccessKeyId, _credential.SecretKey, config);
            var orderFulfillmentList = new List<OrderFulfillment>();

            // get the unshipped orders with tracking number for confirming its shipment
            var unshippedOrderFeeds = _orderRepository.GetUnshippedOrdersForShipment(ChannelName);
            if(!unshippedOrderFeeds.Any())
            {
                Console.WriteLine("No unshipped orders found from {0} for shipment confirmation.", ChannelName);
                return true;
            }

            try
            {
                Console.WriteLine("Sending {0} orders for shipment confirmation...", unshippedOrderFeeds.Count);

                // create the order fulfillment
                unshippedOrderFeeds.ForEach(order =>
                {
                    // create fulfillment item list from the order items
                    var fulfillmentItems = new List<OrderFulfillmentItem>();
                    foreach (var item in order.OrderItems)
                    {
                        fulfillmentItems.Add(new OrderFulfillmentItem
                        {
                            Item = item.OrderItemId,
                            Quantity = item.Quantity.ToString()
                        });
                    }

                    // then, the order fulfillment information
                    orderFulfillmentList.Add(new OrderFulfillment
                    {
                        Item = order.OrderId,
                        FulfillmentDate = order.FulfillmentDate,
                        FulfillmentData = new OrderFulfillmentFulfillmentData
                        {
                            Item = order.Carrier.Code,
                            ShippingMethod = "Ground", // order.ShippingMethod,
                            ShipperTrackingNumber = order.ShipperTrackingNumber
                        },
                        Item1 = fulfillmentItems.ToArray()
                    });
                });

                // iterate to the order fulfillment and add it into envelope message
                var envelopeMessages = new List<AmazonEnvelopeMessage>();
                for (var i = 0; i < orderFulfillmentList.Count; i++)
                {
                    var message = new AmazonEnvelopeMessage
                    {
                        MessageID = string.Format("{0}", i + 1),
                        Item = orderFulfillmentList[i]
                    };
                    envelopeMessages.Add(message);
                }

                // create Amazon envelope object
                var amazonEnvelope = new AmazonEnvelope
                {
                    Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = _credential.MerchantId },
                    MessageType = AmazonEnvelopeMessageType.OrderFulfillment,
                    Message = envelopeMessages.ToArray()
                };

                // let's add these orders to the shipment history
                addOrderShipmentHistoryAsync(unshippedOrderFeeds);

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(amazonEnvelope, "AmazonOrderFulfillment");

                // create feed controller and send the confirmation shipment feed
                var submitController = new SubmitFeedController(serviceClient, _logger, _credential.MarketplaceId, _credential.MerchantId, _ApplicationName);
                var streamResponse = submitController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_ORDER_FULFILLMENT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.OrderFulfillment, _ApplicationName);

                _logger.LogInfo(LogEntryType.AmazonOrdersProvider,
                    string.Format("{1}:{2} - Successfully sent confirming order shipment for Order IDs: \"{0}\"", string.Join("\", \"", unshippedOrderFeeds.Select(x => x.OrderId)), ChannelName, _ApplicationName, Constants.APP_NAME));
                Console.WriteLine("Successfully sent confirming order shipment for Order IDs: \"{0}\"", string.Join("\", \"", unshippedOrderFeeds.Select(x => x.OrderId)));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonOrdersProvider,
                    string.Format("{0}:{4} Error in confirming order shipment for for Order IDs: \"{3}\". <br/>Error Message: {1} <br/> Access Key: {2}", ChannelName,
                    (ex.InnerException != null ? string.Format("{0} <br/>Inner Message: {1}",
                        ex.Message, ex.InnerException.Message) : ex.Message),
                        _credential.AccessKeyId,
                        string.Join("\", \"", unshippedOrderFeeds.Select(x => x.OrderId)),
                        _ApplicationName),
                    ex.StackTrace);
                Console.WriteLine("Error in sending shipment confirmation! Error Message: {0} \nStackTrace: {1} ", ex.Message, ex.StackTrace);

                // let's delete the order confirmation
                deleteOrderShipmentHistoryAsync(unshippedOrderFeeds);

                return false;
            }
        }

        #region helpers

        private Task addOrderShipmentHistoryAsync(List<MarketplaceOrderFulfillment> orderFeeds)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Inserting order shipment histories...");
                var records = _orderRepository.InsertOrderShipmentHistory(orderFeeds);
                Console.WriteLine("Done inserting order shipment histories: {0} items", orderFeeds.Count);
            });
            return Task.FromResult(true);
        }

        private Task deleteOrderShipmentHistoryAsync(List<MarketplaceOrderFulfillment> orderFeeds)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Deleting order shipment histories...");
                var records = _orderRepository.DeleteOrderShipmentHistory(orderFeeds);
                Console.WriteLine("Done deleting order shipment histories: {0} items", orderFeeds.Count);
            });
            return Task.FromResult(true);
        }

        /// <summary>
        /// Convert the SubmitFeedResponse stream for the post feed folow into a AmazonExeption if needed
        /// </summary>
        /// <param name="responseStream">The response stream object</param>
        /// <param name="messageType">The type of the message</param>
        /// <returns></returns>
        private void parsedResultStreamAndLogReport(Stream responseStream, AmazonEnvelopeMessageType messageType, string submittedBy)
        {
            try
            {
                using (var stream = responseStream)
                {
                    // the result may not be an XML document. This will be reveled with testing.
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    var report = doc.SelectSingleNode("/AmazonEnvelope/Message/ProcessingReport");
                    var processingSummary = report.SelectSingleNode("ProcessingSummary");
                    var processingReport = new MarketplaceProcessingReport
                    {
                        MerchantId = _credential.MerchantId,
                        MessageType = messageType.ToString(),
                        TransactionId = report.SelectSingleNode("DocumentTransactionID").InnerText,
                        MessagesProcessed = int.Parse(processingSummary.SelectSingleNode("MessagesProcessed").InnerText),
                        MessagesSuccessful = int.Parse(processingSummary.SelectSingleNode("MessagesSuccessful").InnerText),
                        MessagesWithError = int.Parse(processingSummary.SelectSingleNode("MessagesWithError").InnerText),
                        MessagesWithWarning = int.Parse(processingSummary.SelectSingleNode("MessagesWithWarning").InnerText),
                        StatusCode = report.SelectSingleNode("StatusCode").InnerText,
                        SubmittedBy = submittedBy
                    };

                    // parsed the any processing report results
                    var results = report.SelectNodes("Result");
                    var reportResults = new List<MarketplaceProcessingReportResult>();
                    foreach (XmlNode result in results)
                    {
                        reportResults.Add(new MarketplaceProcessingReportResult
                        {
                            TransactionId = processingReport.TransactionId,
                            MessageId = int.Parse(result.SelectSingleNode("MessageID").InnerText),
                            Code = result.SelectSingleNode("ResultCode").InnerText,
                            MessageCode = result.SelectSingleNode("ResultMessageCode").InnerText,
                            Description = result.SelectSingleNode("ResultDescription").InnerText,
                            AdditionalInfo = result.SelectSingleNode("AdditionalInfo/AmazonOrderID") == null ? "" : result.SelectSingleNode("AdditionalInfo/AmazonOrderID").InnerText
                        });
                    }

                    // add it to the report summary
                    processingReport.ReportResults = reportResults;

                    // save it to the database
                    _logger.AddProcessingReport(processingReport);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonOrdersProvider,
                    string.Format("Error in parsing {0} result response stream. <br/> Error Message: {1}", messageType.ToString(),
                    ex.InnerException != null ? string.Format("{0} <br/>Inner Message: {1}", ex.Message, ex.InnerException.Message) : ex.Message),
                    ex.StackTrace);
            }
        }

        /// <summary>
        /// Converts the response object into MarketplaceOrder object.
        /// Also makes the call to get the items for the order.
        /// </summary>
        /// <param name="order">The given Amazon order objecdt.</param>
        /// <returns>A MarketplaceOrder object</returns>
        private MarketplaceOrder convertOrderResponseToMarketplaceOrder(MarketplaceWebServiceOrders.Model.Order order)
        {
            var orderStatus = (Inventory.Shared.Models.OrderStatus)Enum.Parse(typeof(Inventory.Shared.Models.OrderStatus), order.OrderStatus, true);

            var marketplaceOrder = new MarketplaceOrder()
            {
                BuyerEmail = order.BuyerEmail,
                BuyerName = order.BuyerName,
                OrderTotal = order.IsSetOrderTotal() ? decimal.Parse(order.OrderTotal.Amount) : 0,
                OrderId = order.AmazonOrderId,
                Marketplace = ChannelName,
                ShipServiceLevel = order.ShipServiceLevel,                
                ShipmentServiceCategory = order.ShipmentServiceLevelCategory,
                NumOfItemsShipped = order.NumberOfItemsShipped,
                NumOfItemsUnshipped = order.NumberOfItemsUnshipped,
                PurchaseDate = order.PurchaseDate,
                LastUpdateDate = order.LastUpdateDate,
                PaymentMethod = order.PaymentMethod,
                OrderStatus = orderStatus,
                OrderType = order.OrderType,
                EarliestShipDate = order.EarliestShipDate,
                LatestShipDate = order.LatestShipDate,
                EarliestDeliveryDate = order.EarliestDeliveryDate,
                LatestDeliveryDate = order.LatestDeliveryDate,
                MarketplaceId = order.MarketplaceId,
                PurchaseOrderNumber = order.PurchaseOrderNumber,
                SalesChannel = order.SalesChannel,
                SellerOrderId = order.SellerOrderId,
            };

            if (order.IsSetShippingAddress())
            {
                marketplaceOrder.ShippingAddressLine1 = order.ShippingAddress.AddressLine1;
                marketplaceOrder.ShippingAddressLine2 = order.ShippingAddress.AddressLine2;
                marketplaceOrder.ShippingAddressLine3 = order.ShippingAddress.AddressLine3;
                marketplaceOrder.ShippingCity = order.ShippingAddress.City;
                marketplaceOrder.ShippingStateOrRegion = order.ShippingAddress.StateOrRegion;
                marketplaceOrder.ShippingPostalCode = order.ShippingAddress.PostalCode;
                marketplaceOrder.ShippingAddressName = order.ShippingAddress.Name;
                marketplaceOrder.ShippingAddressPhone = order.ShippingAddress.Phone;
            }

            // do the request for the list of items for this order
            var listOrderRequest = new ListOrderItemsRequest { SellerId = _credential.MerchantId, AmazonOrderId = order.AmazonOrderId };
            var listItemsResponse = _ordersClient.ListOrderItems(listOrderRequest);
            var nextToken = listItemsResponse.ListOrderItemsResult.NextToken;
            var items = new List<MarketplaceOrderItem>();
            ListOrderItemsByNextTokenResponse nextTokenResponse = null;

            // convert the item responses to marketplace order item objects.
            var orderItems = listItemsResponse.ListOrderItemsResult.OrderItems;

            do
            {
                if (nextTokenResponse != null)
                {
                    orderItems = nextTokenResponse.ListOrderItemsByNextTokenResult.OrderItems;
                    nextToken = nextTokenResponse.ListOrderItemsByNextTokenResult.NextToken;
                }

                // iterate and convert the marketplace order item into local object
                for (var i = 0; i < orderItems.Count; i++)
                {
                    var orderItem = orderItems[i];
                    var item = new MarketplaceOrderItem
                    {

                        OrderId = order.AmazonOrderId,
                        MarketplaceItemId = orderItem.ASIN,
                        OrderItemId = orderItem.OrderItemId,
                        SKU = orderItem.SellerSKU,
                        Title = orderItem.Title,
                        QtyOrdered = Convert.ToInt32(orderItem.QuantityOrdered),
                        QtyShipped = Convert.ToInt32(orderItem.QuantityShipped),
                        ConditionNote = orderItem.ConditionNote
                    };
                    
                    item.Price = orderItem.IsSetItemPrice() ? decimal.Parse(orderItem.ItemPrice.Amount) : 0;
                    item.ShippingPrice = orderItem.IsSetShippingPrice() ? decimal.Parse(orderItem.ShippingPrice.Amount) : 0;
                    item.GiftWrapPrice = orderItem.IsSetGiftWrapPrice() ? decimal.Parse(orderItem.GiftWrapPrice.Amount) : 0;
                    item.Tax = orderItem.IsSetItemTax() ? decimal.Parse(orderItem.ItemTax.Amount) : 0;
                    item.ShippingTax = orderItem.IsSetShippingTax() ? decimal.Parse(orderItem.ShippingTax.Amount) : 0;
                    item.GiftWrapTax = orderItem.IsSetGiftWrapTax() ? decimal.Parse(orderItem.GiftWrapTax.Amount) : 0;
                    item.ShippingDiscount = orderItem.IsSetShippingDiscount() ? decimal.Parse(orderItem.ShippingDiscount.Amount) : 0;
                    item.PromotionDiscount = orderItem.IsSetPromotionDiscount() ? decimal.Parse(orderItem.PromotionDiscount.Amount) : 0;
                    
                    items.Add(item);
                }

                // get the page order items if the next token not null
                if (!string.IsNullOrWhiteSpace(nextToken))
                {
                    // pause for 2 seconds, this is the restore reate for the ListOrderItemsByNextToken for every 30 quota
                    Thread.Sleep(2000);

                    var nextRequest = new ListOrderItemsByNextTokenRequest { SellerId = _credential.MerchantId, NextToken = nextToken };
                    nextTokenResponse = _ordersClient.ListOrderItemsByNextToken(nextRequest);
                }

            } while (!string.IsNullOrWhiteSpace(nextToken));

            // set the order items to the local object
            marketplaceOrder.OrderItems = items;

            return marketplaceOrder;
        }
        #endregion
    }
}
