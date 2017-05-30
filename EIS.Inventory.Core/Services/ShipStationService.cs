using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EIS.Inventory.Core.Services
{
    public class ShipStationService : IDisposable
    {
        #region Properties
        private string _authorizationCode = "";
        private Uri _baseAddress;
        private readonly IImageHelper _imageHelper;
        private EisInventoryContext _context;
        private readonly string _marketplaceMode;
        private readonly bool _hasValidCredential = false;
        private LogService _logger;
        private EmailNotificationService _emailService;
        
        #endregion


        public ShipStationService ()
        {
            _logger = new LogService();
            _imageHelper = new ImageHelper(new PersistenceHelper());
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
            _context = new EisInventoryContext();
            _emailService = new EmailNotificationService();

            var shipCredential = GetShipStationCredentials();
            if (shipCredential != null)
            {
                var combinedKey = string.Format("{0}:{1}", shipCredential.ApiKey, shipCredential.ApiSecretKey);
                _authorizationCode = string.Format("Basic {0}", StringUtility.Base64Encode(combinedKey));
                _baseAddress = new Uri(shipCredential.ServiceEndPoint);
                _hasValidCredential = true;
            }           
        }

        #region Public Methods

        public List<order> GetUnshippedOrders ()
        {
            var unshippedOrderList = new List<order>();
            unshippedOrderList = _context.orders
                    .Where(o => o.OrderStatus == OrderStatus.Unshipped)
                    .ToList();

            var existingshippingorders = _context.shippingstationtrackings
                .Select(o => o.EisOrderId)
                .ToList();

            // filter unshipped orders that were not yet posted to the shipstation system
            unshippedOrderList = unshippedOrderList
                .Where(o => !existingshippingorders.Contains(o.EisOrderId))
                .ToList();

            return unshippedOrderList;
        }

        public order GetEISOrderByOrderNumber (string orderNumber)
        {            
            var orderObject = _context.orders.FirstOrDefault(o => o.OrderId == orderNumber);

            return orderObject;
        }

        public async Task<int> PostOrderToShipStation()
        {
            int returnCount = 0;

            try
            {
                var unshippedOrders = GetUnshippedOrders();
                var qualifiedUnshippedOrders = new List<order>();

                // Set Special Quantity for Shadow SKU
                foreach (var order in unshippedOrders)
                {
                    var totalUnitsOrdered = 0;
                    var orderItems = order.orderitems.ToList();
                    foreach (var item in orderItems)
                    {
                        // get the product to determin its FactoryQty
                        var product = _context.products.FirstOrDefault(x => x.EisSKU == item.SKU);
                        Console.WriteLine(">> PRODUCT IS NULL : {0} - EisSKU:{1} \t  FactorQty:{2}", product == null, (product == null ? "" : product.EisSKU), (product == null ? 0 : product.FactorQuantity));
                        if (product == null)
                            continue;

                        totalUnitsOrdered = totalUnitsOrdered + (product.FactorQuantity * item.QtyOrdered);

                        // FOR DEBUGGING ONLY, WILL REMOVE THE FOLLOWING LINES AFTER THE TEST
                        Console.WriteLine("IDENTIFYING - {0} \t {1} \t {2} \t {3}", order.EisOrderId, item.OrderItemId, product.EisSupplierSKU, product.Quantity);
                    }

                    // get the total order products consumed
                    var totalOrderProductsAvailable = orderItems.SelectMany(x => x.orderproducts)
                                    .Sum(o => o.Quantity * o.Pack);

                    // don't add the order if it doesn't meet with the total order items required vs the total order products availability
                    if (totalUnitsOrdered == totalOrderProductsAvailable)
                        qualifiedUnshippedOrders.Add(order);
                }

                // FOR DEBUGGING ONLY, WILL REMOVE THE FOLLOWING LINES AFTER THE TEST
                Console.WriteLine("<< THE LIST OF QUALIFIED ORDERS FOR POSTING SHIPSTATIOIN >>");
                foreach (var order in qualifiedUnshippedOrders)
                {
                    var orderProducts = order.orderitems.SelectMany(x => x.orderproducts).ToList();
                    foreach (var item in orderProducts)
                        Console.WriteLine("{0} \t {1} \t {2} \t {3}", order.EisOrderId, item.OrderItemId, item.EisSupplierSKU, item.Quantity);
                }
                Console.WriteLine("<< END THE LIST OF QUALIFIED ORDERS FOR POSTING SHIPSTATIOIN >>");

                if (qualifiedUnshippedOrders.Count > 0)
                {
                    var ordersShipStation = CreateShippingOrders(qualifiedUnshippedOrders);
                    string responseData = string.Empty;

                    // Shipping Station API
                    using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
                    {

                        //authorization is the base 64 encoded string from API Key and API Secret from the ShipStation account api
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                        var json_content = JsonConvert.SerializeObject(ordersShipStation);

                        //using (var content = new StringContent("{  \"orderNumber\": \"TEST-ORDER-API-DOCS\",  \"orderKey\": \"0f6bec18-3e89-4881-83aa-f392d84f4c74\",  \"orderDate\": \"2015-06-29T08:46:27.0000000\",  \"paymentDate\": \"2015-06-29T08:46:27.0000000\",  \"shipByDate\": \"2015-07-05T00:00:00.0000000\",  \"orderStatus\": \"awaiting_shipment\",  \"customerId\": 37701499,  \"customerUsername\": \"headhoncho@whitehouse.gov\",  \"customerEmail\": \"headhoncho@whitehouse.gov\",  \"billTo\": {    \"name\": \"The President\",    \"company\": null,    \"street1\": null,    \"street2\": null,    \"street3\": null,    \"city\": null,    \"state\": null,    \"postalCode\": null,    \"country\": null,    \"phone\": null,    \"residential\": null  },  \"shipTo\": {    \"name\": \"The President\",    \"company\": \"US Govt\",    \"street1\": \"1600 Pennsylvania Ave\",    \"street2\": \"Oval Office\",    \"street3\": null,    \"city\": \"Washington\",    \"state\": \"DC\",    \"postalCode\": \"20500\",    \"country\": \"US\",    \"phone\": \"555-555-5555\",    \"residential\": true  },  \"items\": [    {      \"lineItemKey\": \"vd08-MSLbtx\",      \"sku\": \"ABC123\",      \"name\": \"Test item #1\",      \"imageUrl\": null,      \"weight\": {        \"value\": 24,        \"units\": \"ounces\"      },      \"quantity\": 2,      \"unitPrice\": 99.99,      \"taxAmount\": 2.5,      \"shippingAmount\": 5,      \"warehouseLocation\": \"Aisle 1, Bin 7\",      \"options\": [        {          \"name\": \"Size\",          \"value\": \"Large\"        }      ],      \"productId\": 123456,      \"fulfillmentSku\": null,      \"adjustment\": false,      \"upc\": \"32-65-98\"    },    {      \"lineItemKey\": null,      \"sku\": \"DISCOUNT CODE\",      \"name\": \"10% OFF\",      \"imageUrl\": null,      \"weight\": {        \"value\": 0,        \"units\": \"ounces\"      },      \"quantity\": 1,      \"unitPrice\": -20.55,      \"taxAmount\": null,      \"shippingAmount\": null,      \"warehouseLocation\": null,      \"options\": [],      \"productId\": 123456,      \"fulfillmentSku\": \"SKU-Discount\",      \"adjustment\": true,      \"upc\": null    }  ],  \"amountPaid\": 218.73,  \"taxAmount\": 5,  \"shippingAmount\": 10,  \"customerNotes\": \"Thanks for ordering!\",  \"internalNotes\": \"Customer called and would like to upgrade shipping\",  \"gift\": true,  \"giftMessage\": \"Thank you!\",  \"paymentMethod\": \"Credit Card\",  \"requestedShippingService\": \"Priority Mail\",  \"carrierCode\": \"fedex\",  \"serviceCode\": \"fedex_2day\",  \"packageCode\": \"package\",  \"confirmation\": \"delivery\",  \"shipDate\": \"2015-07-02\",  \"weight\": {    \"value\": 25,    \"units\": \"ounces\"  },  \"dimensions\": {    \"units\": \"inches\",    \"length\": 7,    \"width\": 5,    \"height\": 6  },  \"insuranceOptions\": {    \"provider\": \"carrier\",    \"insureShipment\": true,    \"insuredValue\": 200  },  \"internationalOptions\": {    \"contents\": null,    \"customsItems\": null  },  \"advancedOptions\": {    \"warehouseId\": 98765,    \"nonMachinable\": false,    \"saturdayDelivery\": false,    \"containsAlcohol\": false,    \"mergedOrSplit\": false,    \"mergedIds\": [],    \"parentId\": null,    \"storeId\": 12345,    \"customField1\": \"Custom data that you can add to an order. See Custom Field #2 & #3 for more info!\",    \"customField2\": \"Per UI settings, this information can appear on some carrier's shipping labels. See link below\",    \"customField3\": \"https://help.shipstation.com/hc/en-us/articles/206639957\",    \"source\": \"Webstore\",    \"billToParty\": null,    \"billToAccount\": null,    \"billToPostalCode\": null,    \"billToCountryCode\": null  }}", System.Text.Encoding.Default, "application/json")) {
                        using (var content = new StringContent(json_content))
                        {
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                            using (var response = await httpClient.PostAsync("orders/createorders", content).ConfigureAwait(false))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    responseData = await response.Content.ReadAsStringAsync();

                                    saveToTracking(ordersShipStation);

                                    CreateLogFile(responseData, "PostOrderToShipStation");

                                    // Check if there are erroneous data and send email notification if found
                                    if (!string.IsNullOrEmpty(responseData))
                                    {
                                        JObject json = JObject.Parse(responseData);
                                        FilterErrorDataAndLogEmail(json, "PostOrderToShipStation Erroneous Data");
                                    }
                                }
                                else
                                {
                                    var orderIdList = ordersShipStation.Select(o => o.OrderNumber).ToList();

                                    var messageText = string.Format("Error Message: {0}.     \n Possible Affected Order Ids: {1}", response.ReasonPhrase, string.Join(",", orderIdList));

                                    CreateLogFile(messageText, "PostOrderToShipStation");
                                    // Send email notification
                                    _emailService.SendShipstationWarningEmailAdmin(subject: "Post Order To ShipStation Error", body: messageText);
                                }

                            }
                        }

                        returnCount = ordersShipStation.Count;
                    }

                }

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}", EisHelper.GetExceptionMessage(ex)), "PostOrderToShipStation");
                Console.WriteLine("Error Message: {0}", EisHelper.GetExceptionMessage(ex));

                // Send email notification
                _emailService.SendEmailAdminException(subject: "Post Order To ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "PostOrderToShipStation Method",
                                                        userName: "OrdersService");

            }

            return returnCount;
        }

        public async Task<int> PostOrderToShipStationByOrderNumber (string orderNumber)
        {
            int returnCount = 0;

            try
            {
                var orderObject = GetEISOrderByOrderNumber(orderNumber);

                // Set Special Quantity for Shadow SKU
                var orderItems = orderObject.orderitems;

                SetSpecialQuantity(ref orderItems);

                if (orderObject != null)
                {
                    var ordersShipStation = MapEisOrderToShipStationOrder(orderObject);
                    var ordersShipStationList = new List<ShipStationOrdersViewModel>();
                    ordersShipStationList.Add(ordersShipStation);
                    string responseData = string.Empty;

                    // Shipping Station API
                    using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
                    {

                        //authorization is the base 64 encoded string from API Key and API Secret from the ShipStation account api
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                        var json_content = JsonConvert.SerializeObject(ordersShipStationList);

                        //using (var content = new StringContent("{  \"orderNumber\": \"TEST-ORDER-API-DOCS\",  \"orderKey\": \"0f6bec18-3e89-4881-83aa-f392d84f4c74\",  \"orderDate\": \"2015-06-29T08:46:27.0000000\",  \"paymentDate\": \"2015-06-29T08:46:27.0000000\",  \"shipByDate\": \"2015-07-05T00:00:00.0000000\",  \"orderStatus\": \"awaiting_shipment\",  \"customerId\": 37701499,  \"customerUsername\": \"headhoncho@whitehouse.gov\",  \"customerEmail\": \"headhoncho@whitehouse.gov\",  \"billTo\": {    \"name\": \"The President\",    \"company\": null,    \"street1\": null,    \"street2\": null,    \"street3\": null,    \"city\": null,    \"state\": null,    \"postalCode\": null,    \"country\": null,    \"phone\": null,    \"residential\": null  },  \"shipTo\": {    \"name\": \"The President\",    \"company\": \"US Govt\",    \"street1\": \"1600 Pennsylvania Ave\",    \"street2\": \"Oval Office\",    \"street3\": null,    \"city\": \"Washington\",    \"state\": \"DC\",    \"postalCode\": \"20500\",    \"country\": \"US\",    \"phone\": \"555-555-5555\",    \"residential\": true  },  \"items\": [    {      \"lineItemKey\": \"vd08-MSLbtx\",      \"sku\": \"ABC123\",      \"name\": \"Test item #1\",      \"imageUrl\": null,      \"weight\": {        \"value\": 24,        \"units\": \"ounces\"      },      \"quantity\": 2,      \"unitPrice\": 99.99,      \"taxAmount\": 2.5,      \"shippingAmount\": 5,      \"warehouseLocation\": \"Aisle 1, Bin 7\",      \"options\": [        {          \"name\": \"Size\",          \"value\": \"Large\"        }      ],      \"productId\": 123456,      \"fulfillmentSku\": null,      \"adjustment\": false,      \"upc\": \"32-65-98\"    },    {      \"lineItemKey\": null,      \"sku\": \"DISCOUNT CODE\",      \"name\": \"10% OFF\",      \"imageUrl\": null,      \"weight\": {        \"value\": 0,        \"units\": \"ounces\"      },      \"quantity\": 1,      \"unitPrice\": -20.55,      \"taxAmount\": null,      \"shippingAmount\": null,      \"warehouseLocation\": null,      \"options\": [],      \"productId\": 123456,      \"fulfillmentSku\": \"SKU-Discount\",      \"adjustment\": true,      \"upc\": null    }  ],  \"amountPaid\": 218.73,  \"taxAmount\": 5,  \"shippingAmount\": 10,  \"customerNotes\": \"Thanks for ordering!\",  \"internalNotes\": \"Customer called and would like to upgrade shipping\",  \"gift\": true,  \"giftMessage\": \"Thank you!\",  \"paymentMethod\": \"Credit Card\",  \"requestedShippingService\": \"Priority Mail\",  \"carrierCode\": \"fedex\",  \"serviceCode\": \"fedex_2day\",  \"packageCode\": \"package\",  \"confirmation\": \"delivery\",  \"shipDate\": \"2015-07-02\",  \"weight\": {    \"value\": 25,    \"units\": \"ounces\"  },  \"dimensions\": {    \"units\": \"inches\",    \"length\": 7,    \"width\": 5,    \"height\": 6  },  \"insuranceOptions\": {    \"provider\": \"carrier\",    \"insureShipment\": true,    \"insuredValue\": 200  },  \"internationalOptions\": {    \"contents\": null,    \"customsItems\": null  },  \"advancedOptions\": {    \"warehouseId\": 98765,    \"nonMachinable\": false,    \"saturdayDelivery\": false,    \"containsAlcohol\": false,    \"mergedOrSplit\": false,    \"mergedIds\": [],    \"parentId\": null,    \"storeId\": 12345,    \"customField1\": \"Custom data that you can add to an order. See Custom Field #2 & #3 for more info!\",    \"customField2\": \"Per UI settings, this information can appear on some carrier's shipping labels. See link below\",    \"customField3\": \"https://help.shipstation.com/hc/en-us/articles/206639957\",    \"source\": \"Webstore\",    \"billToParty\": null,    \"billToAccount\": null,    \"billToPostalCode\": null,    \"billToCountryCode\": null  }}", System.Text.Encoding.Default, "application/json")) {
                        using (var content = new StringContent(json_content))
                        {
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                            using (var response = await httpClient.PostAsync("orders/createorders", content).ConfigureAwait(false))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    responseData = await response.Content.ReadAsStringAsync();

                                    var list = new List<ShipStationOrdersViewModel>();
                                    list.Add(ordersShipStation);
                                    saveToTracking(list);
                                    
                                    CreateLogFile(responseData, "PostOrderToShipStation");

                                    returnCount = 1;
                                }
                                else
                                {
                                    CreateLogFile(response.ReasonPhrase, "PostOrderToShipStation");

                                    // Send email notification
                                    _emailService.SendShipstationWarningEmailAdmin(subject: "Post Order To ShipStation Error", body: response.ReasonPhrase);
                                }

                            }
                        }
                                           
                    }

                }

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}",
                    EisHelper.GetExceptionMessage(ex))
                    , "PostOrderToShipStation");
                Console.WriteLine("Error Message: {0}", EisHelper.GetExceptionMessage(ex));

                // Send email notification
                _emailService.SendEmailAdminException(subject: "Post Order To ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "PostOrderToShipStationByOrderNumber Method",
                                                        userName: "OrdersService");
            }

            return returnCount;
        }

        public async Task<List<ShipStationOrdersViewModel>> GetOrders ()
        {
            List<ShipStationOrdersViewModel> orderList = new List<ShipStationOrdersViewModel>();

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                using (var response = await httpClient.GetAsync("orders").ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseData))
                        {
                            JObject json = JObject.Parse(responseData);

                            //Convert JSON response data to OrderViewModel
                            orderList = JsonConvert.DeserializeObject<List<ShipStationOrdersViewModel>>(json["orders"].ToString());
                        }
                    }
                    else
                    {
                        CreateLogFile(response.ReasonPhrase, "GetOrders");

                        // Send email notification
                        _emailService.SendShipstationWarningEmailAdmin(subject: "Getting Orders From ShipStation Error", body: response.ReasonPhrase);
                    }
                }
            }

            return orderList;

        }


        public async Task<ShipStationOrdersViewModel> GetOrderById ( string ID )
        {
            ShipStationOrdersViewModel orderItem = new ShipStationOrdersViewModel();

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                using (var response = await httpClient.GetAsync("orders/" + ID).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseData))
                        {
                            JObject json = JObject.Parse(responseData);

                            //Convert JSON response data to OrderViewModel
                            orderItem = JsonConvert.DeserializeObject<ShipStationOrdersViewModel>(json["orders"][0].ToString());
                        }
                    }
                    else
                    {
                        CreateLogFile(response.ReasonPhrase, "GetOrderById");
                    }
                }
            }

            return orderItem;

        }


        public async Task<ShipStationOrdersViewModel> GetOrderByNumber ( string orderNumber )
        {
            ShipStationOrdersViewModel orderItem = new ShipStationOrdersViewModel();

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                using (var response = await httpClient.GetAsync("orders?orderNumber=" + orderNumber).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseData))
                        {
                            JObject json = JObject.Parse(responseData);
                                                        
                            if((int)json["total"] > 0)
                            {
                                //Convert JSON response data to OrderViewModel
                                orderItem = JsonConvert.DeserializeObject<ShipStationOrdersViewModel>(json["orders"][0].ToString());
                            }
                            else
                            {
                                orderItem = null;
                            }
                        }
                    }
                    else
                    {
                        CreateLogFile(response.ReasonPhrase, "GetOrderByNumber");
                    }
                }
            }

            return orderItem;

        }

        public async Task<List<ShipStationShipmentViewModel>> GetShipments ()
        {
            var shipmentList = new List<ShipStationShipmentViewModel>();
            var dateFilter = string.Format("{0}-01-01 00:00:00", DateTime.Now.Year.ToString());

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                using (var response = await httpClient.GetAsync(string.Format("shipments?pageSize=500&sortDir=DESC&createDateStart={0}", dateFilter)).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseData))
                        {
                            JObject json = JObject.Parse(responseData);
                            shipmentList = JsonConvert.DeserializeObject<List<ShipStationShipmentViewModel>>(json["shipments"].ToString());
                        }
                    }
                    else
                    {
                        CreateLogFile(response.ReasonPhrase, "GetShipments");
                    }
                }
            }

            return shipmentList;
        }

        public async Task<List<ShipStationShipmentViewModel>> GetShipmentsByDate ( DateTime LastDate )
        {

            var shipmentList = new List<ShipStationShipmentViewModel>();
            var dateFilter = string.Format("{0} 00:00:00", LastDate.ToString("yyyy-MM-dd"));

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                var pageNumber = 0;
                var totalPage = 0;

                do
                {
                    pageNumber++;
                    using (var response = await httpClient.GetAsync(string.Format("shipments?pageSize=500&sortDir=DESC&createDateEnd={0}&page={1}", dateFilter, pageNumber)).ConfigureAwait(false))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();

                            if (!string.IsNullOrEmpty(responseData))
                            {
                                JObject json = JObject.Parse(responseData);
                                shipmentList.AddRange(JsonConvert.DeserializeObject<List<ShipStationShipmentViewModel>>(json["shipments"].ToString()));

                                totalPage = (int)Math.Ceiling(Convert.ToDouble(JsonConvert.DeserializeObject<int>(json["total"].ToString()) / 500.00));

                            }
                        }
                        else
                        {
                            CreateLogFile(response.ReasonPhrase, "GetShipmentsByDate");
                        }

                    }

                } while (pageNumber < totalPage);
            }

            return shipmentList;
        }
        
        public async Task<ShipStationShipmentViewModel> GetShipmentsByOrderNumber ( string orderNumber )
        {

            var shipment = new ShipStationShipmentViewModel();

            using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                using (var response = await httpClient.GetAsync("shipments?orderNumber=" + orderNumber).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(responseData))
                        {
                            JObject json = JObject.Parse(responseData);

                            shipment = JsonConvert.DeserializeObject<ShipStationShipmentViewModel>(json["shipments"][0].ToString());
                        }
                    }
                    else
                    {
                        CreateLogFile(response.ReasonPhrase, "GetShipmentsByOrderNumber");
                    }

                }
            }

            return shipment;

        }
        
        public async Task<int> PostTrackingNumberAndCost ()
        {
            int returnCount = 0;

            try
            {
                int postedShipments = 0;
                var shipList = new List<ShipStationShipmentViewModel>();
                shipList = await GetShipments().ConfigureAwait(false);

                // Filter shipments only with Tracking Numbers
                shipList = shipList.Where(o => o.TrackingNumber != null && o.TrackingNumber != "").ToList();
                shippingstationtracking tracking = null;

                foreach (var shipment in shipList)
                {
                    if (!string.IsNullOrEmpty(shipment.OrderNumber))
                    {
                        var order_item = _context.orders
                            .FirstOrDefault(o => o.OrderId == shipment.OrderNumber && (o.TrackingNumber == null || o.TrackingNumber == ""));

                        if (order_item != null)
                        {
                            // update order shipment details if not yet updated
                            order_item.ShipmentCost = shipment.ShipmentCost;
                            order_item.ShipmentDate = shipment.ShipDate;
                            order_item.CarrierCode = MapCarrierCode(shipment.CarrierCode);
                            order_item.TrackingNumber = shipment.TrackingNumber;
                            order_item.ShippingMethod = shipment.ServiceCode.Replace("_", "");

                            // confirm shipment details only for manual orders
                            if (order_item.Marketplace == MarketPlaceTypes.Values.Eshopo)
                            {
                                ConfirmShipmentDetailsManualOrders(order_item);
                            }

                            // update product weight 
                            UpdateProductWeight(order_item, shipment);

                            tracking = _context.shippingstationtrackings
                                .FirstOrDefault(o => o.EisOrderId == order_item.EisOrderId);

                            if (tracking != null)
                            {
                                tracking.TrackingNumber = shipment.TrackingNumber;
                                tracking.Cost = shipment.ShipmentCost;
                                tracking.ShippingStationOrderId = shipment.OrderId;
                            }

                            postedShipments++;
                        }
                    }
                }

                int cnt = _context.SaveChanges();

                Console.WriteLine("Done updating {0} orders from database.", cnt);

                returnCount = postedShipments;

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}",
                    EisHelper.GetExceptionMessage(ex))
                    , "PostTrackingNumberandCost");
                Console.WriteLine("Error Message: {0}" , EisHelper.GetExceptionMessage(ex));

                // Send email notification
                _emailService.SendEmailAdminException(subject: "Get Shipment Details from ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "PostTrackingNumberAndCost Method",
                                                        userName: "OrdersService");
            }

            return returnCount;
        }
        
        public async Task<int> PostTrackingNumberAndCostByOrderNumber (string orderNumber)
        {
            int returnCount = 0;

            try
            {
                int postedShipments = 0;
                var shipment = new ShipStationShipmentViewModel();
                shipment = await GetShipmentsByOrderNumber(orderNumber).ConfigureAwait(false);
                
                shippingstationtracking tracking = null;
                
                if (!string.IsNullOrEmpty(shipment.OrderNumber))
                {
                    var order_item = _context.orders
                        .FirstOrDefault(o => o.OrderId == shipment.OrderNumber && (o.TrackingNumber == null || o.TrackingNumber == ""));

                    if (order_item != null)
                    {
                        // update order shipment details if not yet updated
                        order_item.ShipmentCost = shipment.ShipmentCost;
                        order_item.ShipmentDate = shipment.ShipDate;
                        order_item.CarrierCode = MapCarrierCode(shipment.CarrierCode);
                        order_item.TrackingNumber = shipment.TrackingNumber;
                        order_item.ShippingMethod = shipment.ServiceCode.Replace("_", "");

                        // confirm shipment details only for manual orders
                        if (order_item.Marketplace == MarketPlaceTypes.Values.Eshopo)
                        {
                            ConfirmShipmentDetailsManualOrders(order_item);
                        }

                        // update product weight 
                        UpdateProductWeight(order_item, shipment);

                        tracking = _context.shippingstationtrackings
                            .FirstOrDefault(o => o.EisOrderId == order_item.EisOrderId);

                        if (tracking != null)
                        {
                            tracking.TrackingNumber = shipment.TrackingNumber;
                            tracking.Cost = shipment.ShipmentCost;
                            tracking.ShippingStationOrderId = shipment.OrderId;
                        }

                        postedShipments++;
                    }
                }

                int cnt = _context.SaveChanges();

                Console.WriteLine("Done updating {0} orders from database.", cnt);

                returnCount = postedShipments;

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}", EisHelper.GetExceptionMessage(ex))
                    , "PostTrackingNumberandCost");
                Console.WriteLine("Error Message: {0}", EisHelper.GetExceptionMessage(ex));

                // Send email notification
                _emailService.SendEmailAdminException(subject: "Get Shipment Details from ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "PostTrackingNumberAndCostByOrderNumber Method",
                                                        userName: "OrdersService");
            }

            return returnCount;
        }                         

        public bool CanDeleteOrder(string orderNumber)
        {
            var canDelete = false;

            using (var context = new EisInventoryContext())
            {
                var orderObject = context.orders.FirstOrDefault(o => o.OrderId == orderNumber);

                if(orderObject != null)
                {
                    var shippingtracking = context.shippingstationtrackings.FirstOrDefault(o => o.EisOrderId == orderObject.EisOrderId);

                    if (shippingtracking != null && shippingtracking.IsActive.Value)
                        canDelete = true;
                }
            }

            return canDelete;
        }

        public async Task<bool> DeleteOrderByOrderNumber (string orderNumber)
        {
            bool isSuccess = false;

            if (CanDeleteOrder(orderNumber))
            {
                var shipStationOrder = await GetOrderByNumber(orderNumber).ConfigureAwait(false);

                if (shipStationOrder != null)
                {
                    using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                        using (var response = await httpClient.DeleteAsync(string.Format("orders/{0}", shipStationOrder.OrderId)).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(responseData))
                                {
                                    JObject json = JObject.Parse(responseData);

                                    isSuccess = Convert.ToBoolean(json["success"].ToString());

                                    if (isSuccess)
                                    {
                                        RemoveTrackingByOrderNumber(orderNumber);
                                    }

                                    var message = json["message"].ToString();

                                    CreateLogFile(string.Format("Order {0}: {1}.", orderNumber, message), "DeleteOrderByOrderNumber");
                                }
                            }
                            else
                            {
                                CreateLogFile(response.ReasonPhrase, "DeleteOrderByOrderNumber");

                                // Send email notification
                                _emailService.SendShipstationWarningEmailAdmin(subject: "Delete Orders from ShipStation Error",
                                    body: string.Format("Failed to delete Order: {0}, Reason: {1}", orderNumber, response.ReasonPhrase));
                            }

                        }
                    }
                }

            }
            
            return isSuccess;

        }

        // use for one time only when getting weights based on end date (LastDate)
        public async Task<int> GetAccurateWeight_PreviousDates ( DateTime LastDate )
        {
            int returnCount = 0;

            try
            {
                int postedShipments = 0;

                Console.WriteLine("Fetching accurate weight data in shipstation...");
                var shipList = new List<ShipStationShipmentViewModel>();
                shipList = await GetShipmentsByDate(LastDate).ConfigureAwait(false);

                Console.WriteLine("There are {0} data acquired from ShipStation.", shipList.Count);
                shipList = shipList.Where(o => o.Weight != null).ToList();

                Console.WriteLine("Updating accurate weight in products...");
                foreach (var shipment in shipList)
                {
                    if (!string.IsNullOrEmpty(shipment.OrderNumber))
                    {
                        var order_item = _context.orders.FirstOrDefault(o => o.OrderId == shipment.OrderNumber);

                        if (order_item != null)
                        {
                            // update product weight 
                            UpdateProductWeight(order_item, shipment);

                            postedShipments++;
                        }
                    }
                }

                int cnt = _context.SaveChanges();

                Console.WriteLine("Done updating {0} orders weights from database.", cnt);

                returnCount = postedShipments;

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}", EisHelper.GetExceptionMessage(ex))
                    , "GetAccurateWeight_PreviousDates");
                Console.WriteLine("Error Message: {0}", EisHelper.GetExceptionMessage(ex));

                // Send Error Email Notification
                _emailService.SendEmailAdminException(subject: "Get Accurate Weight of Previous Dates from ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "GetAccurateWeight_PreviousDates Method",
                                                        userName: "OrdersService");
            }

            return returnCount;
        }

        /// <summary>
        /// Create Shipment Label.
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <returns>Base64 Decoded PDF Value in string format</returns>
        public async Task<string> CreateShipmentLabel ( string orderNumber )
        {
            var decodedbase64string = "";

            try
            {
                var shipStationShipment = await GetOrderByNumber(orderNumber).ConfigureAwait(false);
                var shipStationLabel = CreateShipmentLabelObject(shipStationShipment);

                if (shipStationShipment != null)
                {
                    using (var httpClient = new HttpClient { BaseAddress = _baseAddress })
                    {
                        var json_content = JsonConvert.SerializeObject(shipStationLabel);

                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", _authorizationCode);

                        using (var content = new StringContent(json_content))
                        {
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                            using (var response = await httpClient.PostAsync("orders/createlabelfororder", content).ConfigureAwait(false))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    string responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                                    if (!string.IsNullOrEmpty(responseData))
                                    {
                                        JObject json = JObject.Parse(responseData);

                                        //Convert JSON response data to string
                                        decodedbase64string = json["labelData"].ToString();
                                    }
                                }
                                else
                                {
                                    CreateLogFile(response.ReasonPhrase, "CreateShipmentLabel");

                                    // Send email notification
                                    _emailService.SendShipstationWarningEmailAdmin(subject: "Create Shipment Label from ShipStation Error", 
                                        body: string.Format("Order Number: {0} , Error Message: {1}", orderNumber, response.ReasonPhrase));
                                }

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                CreateLogFile(string.Format("Error Message: {0}", EisHelper.GetExceptionMessage(ex))
                    , "CreateShipmentLabel");
                Console.WriteLine("Error Message: {0}", EisHelper.GetExceptionMessage(ex));

                // Send email notification
                _emailService.SendEmailAdminException(subject: "Create Shipment Label from ShipStation Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "CreateShipmentLabel Method",
                                                        userName: "OrdersService");
            }

            return decodedbase64string;

        }

        public bool HasValidCredentials
        {
            get { return _hasValidCredential; }
        }

        public void Dispose ()
        {
            _context.Dispose();
        }
        
        public bool IsOrderExistingInShipStation(string orderNumber)
        {
            var isExist = false;

            var resultTask = GetOrderByNumber(orderNumber);
            resultTask.Wait();
            var result = resultTask.Result;

            if (result != null)
                isExist = true;

            return isExist;
        }

        public bool IsTrackingExistOrderNumber(string orderNumber)
        {
            var isExist = false;
            var orderItem = _context.orders.FirstOrDefault(o => o.OrderId == orderNumber);

            if (orderItem != null)
            {
                var tracking = _context.shippingstationtrackings.Where(o => o.EisOrderId == orderItem.EisOrderId).FirstOrDefault();

                isExist = tracking != null;
            }

            return isExist;
        }
        
        public string GetProductImageUrl(string eisSKU)
        {
            var productImage = _context.productimages
                    .FirstOrDefault(x => x.EisSKU == eisSKU && (x.ImageType == "CUSTOM" || x.ImageType == "LARGE"));

            var url = string.Empty;
            if (productImage != null)
                url = _imageHelper.GetProductImageUri(eisSKU, productImage.FileName);

            return url;
        }

        #endregion


        #region Private Methods

        private shipstationcredential GetShipStationCredentials ()
        {
            shipstationcredential shipStationCredential = null;

            // get the default ShipStation credential
            var credential = _context.credentials
                .OrderByDescending(x => x.IsDefault)
                .FirstOrDefault(x => x.MarketplaceType.Equals("ShipStation") && x.Mode == _marketplaceMode);

            // convert the credential into shipsation
            if (credential is shipstationcredential)
                shipStationCredential = credential as shipstationcredential;
            else
                Console.WriteLine("Unknown credential type or no ShipStation credential found from db!");

            return shipStationCredential;
        }

        private List<ShipStationOrdersViewModel> CreateShippingOrders ( List<order> unshippedOrderList )
        {

            var ShipStationOrders = new List<ShipStationOrdersViewModel>();

            foreach (var order in unshippedOrderList)
            {

                var mappedOrder = MapEisOrderToShipStationOrder(order);

                ShipStationOrders.Add(mappedOrder);

            }

            return ShipStationOrders;
        }

        private ShipStationOrdersViewModel MapEisOrderToShipStationOrder ( order unshippedOrder )
        {

            var shipStationOrder = new ShipStationOrdersViewModel();

            shipStationOrder.EisOrderId = unshippedOrder.EisOrderId;
            shipStationOrder.OrderKey = unshippedOrder.EisOrderId.ToString();
            shipStationOrder.OrderNumber = unshippedOrder.OrderId;
            shipStationOrder.CustomerName = unshippedOrder.BuyerName;
            shipStationOrder.CustomerEmail = unshippedOrder.BuyerEmail;
            shipStationOrder.OrderDate = unshippedOrder.PurchaseDate;
            shipStationOrder.ShipByDate = unshippedOrder.LatestShipDate;
            shipStationOrder.PaymentDate = unshippedOrder.PurchaseDate;
            shipStationOrder.OrderStatus = "awaiting_shipment";
            shipStationOrder.BillTo = MapAddress(unshippedOrder);
            shipStationOrder.ShipTo = MapAddress(unshippedOrder);
            shipStationOrder.AmountPaid = 0;
            shipStationOrder.TaxAmount = 0;
            shipStationOrder.ShippingAmount = 0;


            var orderItems = new List<ShipStationOrderItemsViewModel>();

            // Use OrderProducts
            foreach (var item in unshippedOrder.orderitems)
            {
                foreach(var oproduct in item.orderproducts)
                {
                    orderItems.Add(MapOrderProducts(oproduct));
                }
            }

            shipStationOrder.Items = orderItems;
            shipStationOrder.PaymentMethod = unshippedOrder.PaymentMethod;

            return shipStationOrder;
        }

        private ShipStationAddressViewModel MapAddress ( order eisOrder )
        {

            var shipAddress = new ShipStationAddressViewModel();
            shipAddress.Name = eisOrder.ShippingAddressName;

            if (!string.IsNullOrEmpty(eisOrder.ShippingAddressLine1))
            {
                shipAddress.Street1 = eisOrder.ShippingAddressLine1;
            }
            else
            {
                shipAddress.Street1 = eisOrder.ShippingAddressLine2;
            }

            shipAddress.Street2 = eisOrder.ShippingAddressLine2;
            shipAddress.Street3 = eisOrder.ShippingAddressLine3;
            shipAddress.PostalCode = eisOrder.ShippingPostalCode;
            shipAddress.Phone = eisOrder.ShippingAddressPhone;
            shipAddress.State = eisOrder.ShippingStateOrRegion;
            shipAddress.City = eisOrder.ShippingCity;
            shipAddress.Company = eisOrder.CompanyName;

            return shipAddress;
        }

        private ShipStationOrderItemsViewModel MapOrderItems ( orderitem eisOrderItem )
        {

            var shippingOrderItem = new ShipStationOrderItemsViewModel();
            var productObject = _context.products
                    .FirstOrDefault(o => o.EisSKU == eisOrderItem.SKU);

            shippingOrderItem.OrderId = eisOrderItem.order.EisOrderId;
            shippingOrderItem.LineItemKey = eisOrderItem.OrderItemId;
            shippingOrderItem.SKU = eisOrderItem.SKU;
            shippingOrderItem.Name = eisOrderItem.Title;
            shippingOrderItem.Quantity = eisOrderItem.QtyOrdered;
            shippingOrderItem.UnitPrice = 0; // eisOrderItem.Price;
            shippingOrderItem.TaxAmount = eisOrderItem.ItemTax;
            shippingOrderItem.ShippingAmount = eisOrderItem.ShippingPrice;
            shippingOrderItem.Adjustment = false;
            shippingOrderItem.CreateDate = eisOrderItem.order.Created;
            shippingOrderItem.ModifyDate = eisOrderItem.order.LastUpdateDate;

            if (productObject != null)
                shippingOrderItem.ImageUrl = GetProductImageUrl(eisOrderItem.SKU);

            return shippingOrderItem;

        }

        private ShipStationOrderItemsViewModel MapOrderProducts(orderproduct eisOrderProduct)
        {

            var shippingOrderItem = new ShipStationOrderItemsViewModel();
            var productObject = _context.products
                    .FirstOrDefault(o => o.EisSKU == eisOrderProduct.orderitem.SKU);

            shippingOrderItem.OrderId = eisOrderProduct.orderitem.order.EisOrderId;
            shippingOrderItem.LineItemKey = eisOrderProduct.OrderItemId;
            shippingOrderItem.SKU = eisOrderProduct.EisSupplierSKU;
            shippingOrderItem.Name = eisOrderProduct.orderitem.Title;
            shippingOrderItem.Quantity = eisOrderProduct.Quantity;
            shippingOrderItem.UnitPrice = 0; // eisOrderItem.Price;
            shippingOrderItem.TaxAmount = eisOrderProduct.orderitem.ItemTax;
            shippingOrderItem.ShippingAmount = eisOrderProduct.orderitem.ShippingPrice;
            shippingOrderItem.Adjustment = false;
            shippingOrderItem.CreateDate = eisOrderProduct.orderitem.order.Created;
            shippingOrderItem.ModifyDate = eisOrderProduct.orderitem.order.LastUpdateDate;

            if (productObject != null)
                shippingOrderItem.ImageUrl = GetProductImageUrl(productObject.EisSKU);

            return shippingOrderItem;

        }

        private void saveToTracking ( List<ShipStationOrdersViewModel> orders )
        {
            Console.WriteLine("Inserting {0} orders into database...", orders.Count);
            var counter = 0;
            foreach (var order in orders)
            {
                var tracking = new shippingstationtracking();

                tracking.EisOrderId = order.EisOrderId;
                tracking.DateCreated = DateTime.Now;

                if (!isTrackingExist(order.EisOrderId))
                {
                    tracking.IsActive = true;
                    _context.shippingstationtrackings.Add(tracking);
                }
            }

            counter = _context.SaveChanges();
            Console.WriteLine("Done inserting {0} orders into database.", counter);
        }

        private bool isTrackingExist(int EisOrderId )
        {
            var tracking = _context.shippingstationtrackings.Where(o => o.EisOrderId == EisOrderId).FirstOrDefault();

            return tracking != null;
        }
        
        private void SetSpecialQuantity ( ref ICollection<orderitem> orderItems )
        {
            // let's override the orderitem's SKU and QtyOrdered by Shadow's ParentSKU and the product of its Factor Qty
            foreach(var orderItem in orderItems)
            {
                // get the product details for this order item
                var product = _context.products.FirstOrDefault(x => x.EisSKU == orderItem.SKU);
                if(product != null && product.SkuType == SkuType.Shadow)
                {
                    // get the shadow product information
                    var shadow = _context.shadows.FirstOrDefault(x => x.ShadowSKU == product.EisSKU);
                    if (shadow == null)
                        continue;

                    orderItem.SKU = shadow.ParentSKU;
                    orderItem.QtyOrdered *= shadow.FactorQuantity;
                }
            }
        }

        private string MapCarrierCode ( string ShipStationCarrierCode )
        {
            string eisCarrierCode = "";

            switch (ShipStationCarrierCode)
            {
                case "stamps_com":
                    eisCarrierCode = CarrierTypes.Values.USPS;
                    break;
                case "ups":
                    eisCarrierCode = CarrierTypes.Values.UPS;
                    break;
                case "fedex":
                    eisCarrierCode = CarrierTypes.Values.FedEx;
                    break;
                default: break;
            }

            return eisCarrierCode;
        }

        private void UpdateProductWeight ( order order_item, ShipStationShipmentViewModel shipment )
        {
            if (order_item.orderitems.Count == 1 && shipment.Weight != null) // exclude orders with 2 or more skus
            {
                var orderItemProduct = order_item.orderitems.First();
                var productObject = _context.products.FirstOrDefault(o => o.EisSKU == orderItemProduct.SKU);
                decimal converted_weight = 0.0m;

                if (productObject != null)
                {
                    var computed_weight = shipment.Weight.value;

                    if (orderItemProduct.QtyOrdered > 1) // divide weight by quantity for multiple quantities
                    {
                        converted_weight = computed_weight / Convert.ToDecimal(orderItemProduct.QtyOrdered);
                    }
                    else
                    {
                        converted_weight = computed_weight;
                    }

                    var accurateWeight = Math.Round(converted_weight, MidpointRounding.AwayFromZero);

                    var isParent = IsParentProduct(productObject);

                    if(isParent)
                    {
                        // Update Parent Product based on Mean Weight
                        var parentMeanWeight = UpdateParentProductWeight(productObject, accurateWeight, shipment.Weight.units);

                        // update all shadows sku under the parent sku
                        UpdateShadowProductWeightsFromParent(productObject, parentMeanWeight, shipment.Weight.units);
                        
                    }
                    else if(productObject.SkuType == SkuType.Shadow)
                    {
                        // Update Shadows SKU weight
                        UpdateShadowProductsWeight(productObject, accurateWeight, shipment.Weight.units);
                    }
                    else
                    {
                        productObject.AccurateWeight = (decimal)accurateWeight;
                        productObject.AccurateWeightUnit = shipment.Weight.units;
                        productObject.GuessedWeight = (decimal)accurateWeight;
                        productObject.GuessedWeightUnit = shipment.Weight.units;
                    }
                                      
                    UpdateShippingRates(accurateWeight, productObject);
                }

            }
        }

        private decimal UpdateParentProductWeight(product productObject, decimal accurateWeight, string unit)
        {
            decimal meanWeight = 0;

            if (IsParentProduct(productObject))
            {
                var parentWeight = Math.Round(accurateWeight, MidpointRounding.AwayFromZero);

                productObject.AppendWeights(Convert.ToDecimal(parentWeight));
                meanWeight = Math.Round(productObject.GetMeanWeight(), MidpointRounding.AwayFromZero);

                productObject.AccurateWeight = meanWeight;
                productObject.GuessedWeight = meanWeight;

                productObject.AccurateWeightUnit = unit;
                productObject.GuessedWeightUnit = unit;
            }

            return meanWeight;
        }

        private bool IsParentProduct(product productObject)
        {
            var isParentShadow = false;
            var shadowProduct = _context.shadows.FirstOrDefault(o => o.ParentSKU == productObject.EisSKU);

            if (shadowProduct != null && productObject.SkuType == SkuType.Normal)
            {
                isParentShadow = true;
            }            

            return isParentShadow;
        }

        private void UpdateShadowProductsWeight(product productObject, decimal accurateWeight, string unit)
        {
            try
            {                
                // update all shadows sku that are under the same parent sku
                var shadowProduct = _context.shadows.FirstOrDefault(o => o.ShadowSKU == productObject.EisSKU);

                if (shadowProduct != null)
                {
                    var parentProduct = _context.products.FirstOrDefault(o => o.EisSKU == shadowProduct.ParentSKU);

                    var parentComputedWeight = accurateWeight / shadowProduct.FactorQuantity;

                    // Update Parent Product based on Mean Weight
                    var parentMeanWeight = UpdateParentProductWeight(parentProduct, parentComputedWeight, unit);

                    UpdateShadowProductWeightsFromParent(parentProduct, parentMeanWeight, unit);
                }

            } catch(Exception ex)
            {
                // Send email notification
                _emailService.SendEmailAdminException(subject: "Update Shadows Weight Error",
                                                        exParam: ex,
                                                        useDefaultTemplate: true,
                                                        url: "UpdateShadowProductsWeight Method",
                                                        userName: "OrdersService");
            }
        }

        private void UpdateShadowProductWeightsFromParent(product parentProduct, decimal parentAccurateWeight, string unit)
        {
            // update all shadows sku under the parent sku
            if (parentProduct.shadows.Count > 0)
            {
                foreach (var shadow in parentProduct.shadows)
                {
                    var shadowWeight = parentAccurateWeight * shadow.FactorQuantity;
                    shadowWeight = Math.Round(shadowWeight, MidpointRounding.AwayFromZero);

                    var productShadow = _context.products.FirstOrDefault(o => o.EisSKU == shadow.ShadowSKU);

                    if (productShadow != null)
                    {
                        productShadow.AccurateWeight = shadowWeight;
                        productShadow.AccurateWeightUnit = unit;
                        productShadow.GuessedWeight = shadowWeight;
                        productShadow.GuessedWeightUnit = unit;
                    }
                }
            }
        }

        private void UpdateShippingRates ( decimal accurateWeight, product productObject )
        {
            var shippingRate = _context.shippingrates.Where(o => o.WeightFrom == accurateWeight ||
                                                            (o.WeightTo.HasValue && o.WeightTo != 0
                                                            && o.WeightFrom <= accurateWeight
                                                            && accurateWeight <= o.WeightTo))
                                                    .DefaultIfEmpty(null).FirstOrDefault();

            if (shippingRate != null)
            {
                productObject.AccurateShipping = shippingRate.Rate.ToString();
                productObject.GuessedShipping = shippingRate.Rate.ToString();
            }

        }

        private void CreateLogFile ( string logText, string serviceMethod )
        {
            var filePath = ConfigurationManager.AppSettings["ServiceLogPath"];
            var fileName = "ShipStation_Logs";

            LogToFile.CreateLog(serviceMethod, logText, filePath, fileName);
        }

        private void ConfirmShipmentDetailsManualOrders(order orderObject)
        {
            orderObject.NumOfItemsShipped = orderObject.NumOfItemsUnshipped;
            orderObject.NumOfItemsUnshipped = 0;
            orderObject.ShipServiceLevel = "Standard";
            orderObject.ShipmentServiceCategory = "Standard";
            orderObject.OrderStatus = OrderStatus.Shipped;

            foreach(var item in orderObject.orderitems)
            {
                item.QtyShipped = item.QtyOrdered;
            }
        }

        private ShipStationShipmentLabel CreateShipmentLabelObject(ShipStationOrdersViewModel shipmentObject)
        {
            var shipmentLabel = new ShipStationShipmentLabel();

            shipmentLabel.orderId = shipmentObject.OrderId;
            shipmentLabel.carrierCode = shipmentObject.CarrierCode;
            shipmentLabel.serviceCode = shipmentObject.ServiceCode;
            shipmentLabel.packageCode = shipmentObject.PackageCode;
            shipmentLabel.confirmation = shipmentObject.Confirmation;
            shipmentLabel.shipDate = shipmentObject.ShipDate;
            shipmentLabel.weight = shipmentObject.Weight;
            shipmentLabel.testLabel = true;
            shipmentLabel.dimensions = null;
            shipmentLabel.insuranceOptions = null;
            shipmentLabel.internationalOptions = null;
            shipmentLabel.advancedOptions = null;


            return shipmentLabel;
        }

        private bool FilterErrorDataAndLogEmail(JObject jsonData, string subject)
        {
            var hasErrors = false; 

            try
            {
                if(jsonData["hasErrors"] != null 
                    && !string.IsNullOrEmpty(jsonData["hasErrors"].ToString())
                    && jsonData["results"] != null)
                {
                    hasErrors = Convert.ToBoolean(jsonData["hasErrors"].ToString());
                }
                
                if (hasErrors)
                {
                    var orderReturnList = new List<ShipStationReturnOrder>();

                    // Filter erroneous data
                    orderReturnList = JsonConvert.DeserializeObject<List<ShipStationReturnOrder>>(jsonData["results"].ToString());

                    orderReturnList = orderReturnList.Where(o => o.Success == false).ToList();

                    if(orderReturnList.Count > 0)
                    {
                        var bodyMessage = new StringBuilder();

                        foreach (var order in orderReturnList)
                        {
                            bodyMessage.AppendFormat("Order ID: {0} , Error Message: {1}", order.OrderNumber, order.ErrorMessage);
                            bodyMessage.Append("");
                        }

                        // Create log file and Send Email
                        CreateLogFile(logText: bodyMessage.ToString(), serviceMethod: subject);

                        _emailService.SendShipstationWarningEmailAdmin(subject: subject, body: bodyMessage.ToString());
                    }
                }

            } catch(Exception ex)
            {
                CreateLogFile(logText: EisHelper.GetExceptionMessage(ex), serviceMethod:  subject + " - Error Filtering Data");
            }

            return hasErrors;
        }

        private bool RemoveTrackingByOrderNumber(string orderNumber)
        {
            var output = false;

            using (var context = new EisInventoryContext())
            {
                var orderObject = context.orders.FirstOrDefault(o => o.OrderId == orderNumber);

                if (orderObject != null)
                {
                    var shippingtracking = context.shippingstationtrackings.FirstOrDefault(o => o.EisOrderId == orderObject.EisOrderId);

                    if (shippingtracking != null)
                    {
                        shippingtracking.IsActive = false;

                        output = true;

                        context.SaveChanges();
                    }
                }
            }

            return output;
        }

        #endregion

    }
}
