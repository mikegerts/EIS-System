using System.Collections.Generic;
using EIS.SchedulerTaskApp.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class RequestHelper
    {
        public static string MerchantId { get; set; } // or seller id
        public static List<string> MarketplaceIdList { get; set; }
        public static string AccessKeyId { get; set; }
        public static string SecretKey { get; set; }
        public static string ServiceUrl { get { return "https://mws.amazonservices.com"; } }


        /// <summary>
        /// Create inventory feed for item's quantity
        /// </summary>
        /// <param name="inventoryItems">The list of items that are to have their quantities updated</param>
        /// <returns></returns>
        public static AmazonEnvelope CreateInventoryFeedEnvelope(List<MarketplaceInventoryUpdateItem> inventoryItems)
        {
            // iterate to the EIS products and parsed it into Invetory Feed
            var inventoryFeeds = new List<Models.Inventory>();
            inventoryItems.ForEach(x =>
            {
                inventoryFeeds.Add(new Models.Inventory
                {
                    SKU = x.SKU,
                    Item = x.InventoryQuantity.ToString(),
                    // set the default leadtime shipment to 3 days
                    FulfillmentLatency = x.LeadtimeShip == 0 ? "3" : x.LeadtimeShip.ToString(),
                });
            });

            // create Amazon envelope object
            var amazonEnvelope = new AmazonEnvelope
            {
                Header = new Header { DocumentVersion = "1.01", MerchantIdentifier = MerchantId },
                MessageType = AmazonEnvelopeMessageType.Inventory
            };

            // add all Inventory feeds as messages to the envelope
            var envelopeMessages = new List<AmazonEnvelopeMessage>();
            for (var i = 0; i < inventoryFeeds.Count; i++)
            {
                var message = new AmazonEnvelopeMessage
                {
                    MessageID = string.Format("{0}", i + 1),
                    OperationType = AmazonEnvelopeMessageOperationType.Update,
                    Item = inventoryFeeds[i]
                };
                envelopeMessages.Add(message);
            }

            // convert to array and set its message
            amazonEnvelope.Message = envelopeMessages.ToArray();

            return amazonEnvelope;
        }

        /// <summary>
        /// Set the Amazon client credentials for posting feeds
        /// </summary>
        /// <param name="credential">An object contains the Amazon credentials</param>
        public static void SetCredentials(AmazonCredentialDto credential)
        {
            MerchantId = credential.MerchantId;
            MarketplaceIdList = new List<string> { credential.MarketplaceId };
            AccessKeyId = credential.AccessKeyId;
            SecretKey = credential.SecretKey;
        }
    }
}
