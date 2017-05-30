using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using EIS.SystemJobApp.AmazonService;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Marketplaces
{
    public class AmazonProductAdvertisingRequestor
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _associateKey;
        private readonly AWSECommerceServicePortTypeClient _client;

        public AmazonProductAdvertisingRequestor(string accessKey, string secretKey, string assocaiteKey)
        {
            _accessKey = accessKey;
            _secretKey = secretKey;
            _associateKey = assocaiteKey;

            // create a WCF Amazon ECS client
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.MaxReceivedMessageSize = int.MaxValue;
            _client = new AWSECommerceServicePortTypeClient(binding,
                new EndpointAddress("https://webservices.amazon.com/onca/soap?Service=AWSECommerceService"));

            // add authentication to the ECS client
            _client.ChannelFactory.Endpoint.Behaviors.Add(new AmazonSigningEndpointBehavior(_accessKey, _secretKey));
        }

        public ItemLookupResponse GetProductItemResponse(AmazonInfoFeed infoFeed)
        {
            return getItemLookupResponse(new List<AmazonInfoFeed> { infoFeed });
        }

        private ItemLookupResponse getItemLookupResponse(List<AmazonInfoFeed> infoFeeds)
        {
            // prepare the ItemLookup request
            var requests = new List<ItemLookupRequest>();

            // this should be 5 items only
            foreach(var infoFeed in infoFeeds)
            {
                // create request item type for asin - WE ONLY FIND PRODUCT INFO VIA ASIN ONLY!
                if (!string.IsNullOrEmpty(infoFeed.ASIN))
                    requests.Add(createItemRequestLookup(infoFeed.ASIN, ItemLookupRequestIdType.ASIN));
            }

            // return null if there is no request available
            if (!requests.Any()) return null;

            // create an ItemSearch wrapper
            var itemLookUp = new ItemLookup();
            itemLookUp.AssociateTag = _associateKey;
            itemLookUp.AWSAccessKeyId = _accessKey;
            itemLookUp.Request = requests.ToArray();

            // send the ItemLookup request
            return _client.ItemLookup(itemLookUp);
        }

        private ItemLookupRequest createItemRequestLookup(string itemId, ItemLookupRequestIdType requestIdType)
        {
            // prepare the ItemLookup request
            var request = new ItemLookupRequest
            {
                Condition = Condition.All,
                ConditionSpecified = true,
                IdType = requestIdType,
                IdTypeSpecified = true,
                ItemId = new string[] { itemId },
                ResponseGroup = new string[] { "Images", "ItemAttributes", "Offers" },
            };

            // don't add search index if request type is ASIN
            if (requestIdType != ItemLookupRequestIdType.ASIN)
                request.SearchIndex = "All";

            return request;
        }
    }
}
