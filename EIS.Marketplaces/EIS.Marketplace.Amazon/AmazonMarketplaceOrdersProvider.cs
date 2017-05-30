using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using AmazonWebServiceModels;
using EIS.Inventory.Core;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.Amazon.Helpers;
using EIS.Marketplace.Amazon.Properties;
using MarketplaceWebService;
using MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.Model;

namespace EIS.Marketplace.Amazon
{
    [Export(typeof(IMarketplaceOrdersProvider))]
    public class AmazonMarketplaceOrdersProvider : IMarketplaceOrdersProvider
    {
        private MarketplaceWebServiceClient _amazonClient;
        private MarketplaceWebServiceOrdersClient _ordersClient;
        private AmazonCredentialDto _credential;
        private ILogService _logger;

        public AmazonMarketplaceOrdersProvider()
        {
            // Verify that the settings in the config file are setup correctly.
            if (string.IsNullOrWhiteSpace(_ApplicationName))
                throw new InvalidOperationException("ApplicationName setting in the config file can't be whitespace, blank or null");
            if (string.IsNullOrWhiteSpace(_Version))
                throw new InvalidOperationException("Version setting in the config file can't be whitespace, blank or null");
            _logger = Core.Get<ILogService>();
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

        public CredentialDto MarketplaceCredential
        {
            get { return _credential; }
            set
            {
                _credential = value as AmazonCredentialDto;
                RequestHelper.SetCredentials(_credential);
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

        public bool ConfirmOrderShimpmentDetails(MarketplaceOrderFulfillment marketplaceOrder, string submittedBy)
        {
            if (!marketplaceOrder.OrderItems.Any())
                return false;

            // create configuratin to use US marketplace
            var config = new MarketplaceWebServiceConfig { ServiceURL = RequestHelper.ServiceUrl };
            config.SetUserAgentHeader(_ApplicationName, _Version, "C#");
            _amazonClient = new MarketplaceWebServiceClient(_credential.AccessKeyId, _credential.SecretKey, config);

            try
            {
                // create fulfillment item list from the order items
                var fulfillmentItems = new List<OrderFulfillmentItem>();
                foreach (var item in marketplaceOrder.OrderItems)
                {
                    fulfillmentItems.Add(new OrderFulfillmentItem
                    {
                        Item = item.OrderItemId,
                        Quantity = item.Quantity.ToString()
                    });
                }

                // create the order fulfillment information
                var fulfillment = new OrderFulfillment
                {
                    Item = marketplaceOrder.OrderId,
                    FulfillmentDate = marketplaceOrder.FulfillmentDate,
                    FulfillmentData = new OrderFulfillmentFulfillmentData
                    {
                        Item = marketplaceOrder.Carrier.Code,
                        ShippingMethod = marketplaceOrder.ShippingMethod,
                        ShipperTrackingNumber = marketplaceOrder.ShipperTrackingNumber
                    },
                    Item1 = fulfillmentItems.ToArray()
                };

                // create Amazon envelope object
                var amazonEnvelope = new AmazonEnvelope
                {
                    Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = _credential.MerchantId },
                    MessageType = AmazonEnvelopeMessageType.OrderFulfillment,
                    Message = new AmazonEnvelopeMessage[] { 
                    new AmazonEnvelopeMessage {   MessageID = "1", Item = fulfillment } 
                }
                };

                // parse the envelope into file
                var xmlFullName = XmlParser.WriteXmlToFile(amazonEnvelope, "OrderFulfillment");

                var submitController = new SubmitFeedController(_amazonClient, _logger, _credential.MarketplaceId, _credential.MerchantId, submittedBy);
                var streamResponse = submitController.SubmitFeedAndGetResponse(xmlFullName, AmazonFeedType._POST_ORDER_FULFILLMENT_DATA_);
                parsedResultStreamAndLogReport(streamResponse, AmazonEnvelopeMessageType.OrderFulfillment, submittedBy);

                _logger.Add(LogEntrySeverity.Information, LogEntryType.AmazonOrdersProvider, string.Format("{0} - Successfully sent confirming order shipment for Order ID \'{1}\'", ChannelName, marketplaceOrder.OrderId));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Add(LogEntrySeverity.Error, LogEntryType.AmazonOrdersProvider,
                    string.Format("{0} - Error in confirming order shipment for OrderId: {3}. <br/>Error Message: {1} <br/> Access Key: {2}", ChannelName,
                    EisHelper.GetExceptionMessage(ex),
                        _credential.AccessKeyId,
                        marketplaceOrder.OrderId),
                    ex.StackTrace);

                return false;
            }
        }

        public List<Carrier> GetShippingCarriers()
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

        private List<MarketplaceOrder> getMarketplaceOrdersData(List<string> orderIds)
        {
            // init the Amazon web service
            var config = new MarketplaceWebServiceOrdersConfig { ServiceURL = "https://mws.amazonservices.com" };
            config.SetUserAgent(_ApplicationName, _Version);
            _ordersClient = new MarketplaceWebServiceOrdersClient(_credential.AccessKeyId, _credential.SecretKey, config);
            var results = new List<MarketplaceOrder>();

            try
            {
                // create the request object
                var request = new GetOrderRequest
                {
                    AmazonOrderId = orderIds,
                    SellerId = _credential.MerchantId
                };

                // send the request
                var orderResponse = _ordersClient.GetOrder(request);
                var ordersResult = orderResponse.GetOrderResult.Orders;
                if (!ordersResult.Any())
                    return null;

                foreach(var item in ordersResult)
                    results.Add(convertOrderResponseToMarketplaceOrder(item));

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


        /// <summary>
        /// Converts the response object into MarketplaceOrder object.
        /// Also makes the call to get the items for the order.
        /// </summary>
        /// <param name="order">The given Amazon order objecdt.</param>
        /// <returns>A MarketplaceOrder object</returns>
        private MarketplaceOrder convertOrderResponseToMarketplaceOrder(MarketplaceWebServiceOrders.Model.Order order)
        {
            var orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), order.OrderStatus, true);

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
                _logger.Add(LogEntrySeverity.Error,
                    LogEntryType.AmazonOrdersProvider,
                    string.Format("{2} - Error in parsing {0} result response stream. <br/> Error Message: {1}", messageType.ToString(),
                    EisHelper.GetExceptionMessage(ex), ChannelName),
                    ex.StackTrace);
            }
        }

        public bool CancelOrder(string orderId)
        {
            return true;
        }

        public bool UnshippedOrder(string orderId)
        {
            return true;
        }
    }
}
