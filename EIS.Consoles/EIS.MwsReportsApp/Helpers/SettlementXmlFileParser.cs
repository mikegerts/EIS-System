using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp.Helpers
{
    public class SettlementXmlFileParser
    {
        public SettlementReportDto ParseXmlFiles(List<string> xmlFilePaths)
        {
            var settlementOrders = new List<SettlementOrderDto>();
            var settlementRefunds = new List<SettlementRefundDto>();
            var xmlDoc = new XmlDocument();

            Console.WriteLine("Parsing the Reports Info XML file: {0}", xmlFilePaths.Count);

            // iterate to each file and extract the data
            foreach (var filePath in xmlFilePaths)
            {
                xmlDoc.Load(filePath);
                XmlNode row = xmlDoc.ChildNodes[1];

                var settlementId = row.SelectSingleNode("./Message/SettlementReport/SettlementData/AmazonSettlementID").InnerText;
                var orderNodes = row.SelectNodes("./Message/SettlementReport/Order");
                var refundNodes = row.SelectNodes("./Message/SettlementReport/Refund");

                // parsed first the Orders
                parsedOderNodes(orderNodes, settlementOrders, settlementId);
                parsedRefundNodes(refundNodes, settlementRefunds, settlementId);
            }

            Console.WriteLine("Done parsing reports info XML file and return {0} settlement data.", settlementOrders.Count);

            return new SettlementReportDto
            {
                Orders = settlementOrders,
                Refunds = settlementRefunds
            };
        }

        public void DeleteXmlFiles(List<string> xmlFilePaths)
        {
            try
            {
                xmlFilePaths.ForEach(file => { File.Delete(file); });
            }
            catch { }
        }

        private void parsedOderNodes(XmlNodeList orderNodes, List<SettlementOrderDto> ordersList, string settlementId)
        {
            foreach (XmlNode orderNode in orderNodes)
            {
                var extractor = new NodeValueExtractor(orderNode);
                var order = new SettlementOrderDto
                {
                    SettlementId = settlementId,
                    OrderId = extractor.GetValue<string>("AmazonOrderID"),
                    ShipmentId = extractor.GetValue<string>("ShipmentID"),
                    Marketplace = extractor.GetValue<string>("MarketplaceName"),
                };

                order.MerchantFulfillmentId = orderNode.SelectSingleNode("./Fulfillment/MerchantFulfillmentID").InnerText;
                order.PostedDate = Convert.ToDateTime(orderNode.SelectSingleNode("./Fulfillment/PostedDate").InnerText);

                // get the item nodes and add it to the list
                var itemNodes = orderNode.SelectNodes("./Fulfillment/Item");
                foreach (XmlNode itemNode in itemNodes)
                {
                    extractor = new NodeValueExtractor(itemNode);
                    var item = new SettlementItemDto
                    {
                        OrderId = order.OrderId,
                        OrderItemCode = extractor.GetValue<string>("AmazonOrderItemCode"),
                        SKU = extractor.GetValue<string>("SKU"),
                        Quantity = extractor.GetValue<int>("Quantity")
                    };

                    // get item fees and price nodes
                    var itemPriceNodes = itemNode.SelectNodes("./ItemPrice");
                    var itemFeeNodes = itemNode.SelectNodes("./ItemFees");

                    // parse the item prices and fees
                    item.ItemPrices = getParsedItemPrices(itemPriceNodes, item.OrderItemCode);
                    item.ItemFees = getParsedItemFees(itemFeeNodes, item.OrderItemCode);

                    // and, add it to the list
                    order.Items.Add(item);
                }

                // add it to the list
                ordersList.Add(order);
            }
        }

        private void parsedRefundNodes(XmlNodeList refundNodes, List<SettlementRefundDto> refundList, string settlementId)
        {
            foreach (XmlNode refundNode in refundNodes)
            {
                var extractor = new NodeValueExtractor(refundNode);

                var refund = new SettlementRefundDto
                {
                    SettlementId = settlementId,
                    OrderId = extractor.GetValue<string>("AmazonOrderID"),
                    AdjustmentId = extractor.GetValue<string>("AdjustmentID"),
                    Marketplace = extractor.GetValue<string>("MarketplaceName"),
                };

                refund.MerchantFulfillmentId = refundNode.SelectSingleNode("./Fulfillment/MerchantFulfillmentID").InnerText;
                refund.PostedDate = Convert.ToDateTime(refundNode.SelectSingleNode("./Fulfillment/PostedDate").InnerText);

                // get the item nodes and add it to the list
                var itemNodes = refundNode.SelectNodes("./Fulfillment/AdjustedItem");
                foreach (XmlNode itemNode in itemNodes)
                {
                    extractor = new NodeValueExtractor(itemNode);
                    var item = new SettlementItemDto
                    {
                        OrderId = refund.OrderId,
                        OrderItemCode = extractor.GetValue<string>("AmazonOrderItemCode"),
                        SKU = extractor.GetValue<string>("SKU"),
                        MerchantAdjustmentItemId = extractor.GetValue<string>("MerchantAdjustmentItemID"),
                    };

                    // get item fees and price nodes
                    var itemPriceNodes = itemNode.SelectNodes("./ItemPriceAdjustments");
                    var itemFeeNodes = itemNode.SelectNodes("./ItemFeeAdjustments");

                    // parse the item prices and fees
                    item.ItemPrices = getParsedItemPrices(itemPriceNodes, item.OrderItemCode);
                    item.ItemFees = getParsedItemFees(itemFeeNodes, item.OrderItemCode);

                    // and, add it to the list
                    refund.Items.Add(item);
                }

                // add it to the list
                refundList.Add(refund);
            }
        }

        private List<ItemChargeDto> getParsedItemPrices(XmlNodeList priceNodes, string orderItemCode)
        {
            var itemPrices = new List<ItemChargeDto>();
            foreach (XmlNode node in priceNodes)
            {
                var type = node.SelectSingleNode("./Component/Type").InnerText;
                var amount = node.SelectSingleNode("./Component/Amount").InnerText;
                var currency = node.SelectSingleNode("./Component/Amount").Attributes["currency"].Value;

                // add it to the list
                itemPrices.Add(new ItemChargeDto
                {
                    OrderItemCode = orderItemCode,
                    Type = type,
                    Amount = Convert.ToDecimal(amount),
                    CurrencyCode = currency
                });
            }

            return itemPrices;
        }

        private List<ItemChargeDto> getParsedItemFees(XmlNodeList feeNodes, string orderItemCode)
        {
            var itemFees = new List<ItemChargeDto>();
            foreach (XmlNode node in feeNodes)
            {
                var type = node.SelectSingleNode("./Fee/Type").InnerText;
                var amount = node.SelectSingleNode("./Fee/Amount").InnerText;
                var currency = node.SelectSingleNode("./Fee/Amount").Attributes["currency"].Value;

                // add it to the list
                itemFees.Add(new ItemChargeDto
                {
                    OrderItemCode = orderItemCode,
                    Type = type,
                    Amount = Convert.ToDecimal(amount),
                    CurrencyCode = currency
                });
            }

            return itemFees;
        }
    }
}
