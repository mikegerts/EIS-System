using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using MySql.Data.MySqlClient;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly string _exportFolder;
        private readonly string _connectionString;
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;
        private readonly EmailNotificationService _emailService;

        public OrderService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
            _emailService = new EmailNotificationService();
            _exportFolder = ConfigurationManager.AppSettings["ExportedFilesRoot"].ToString();
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        public IPagedList<OrderListViewModel> GetPagedOrders(int page,
            int pageSize,
            string searchString,
            string shippingAddress,
            string shippingCity,
            string shippingCountry,
            DateTime? orderDateFrom,
            DateTime? orderDateTo,
            DateTime? shipmentDateFrom,
            DateTime? shipmentDateTo,
            OrderStatus orderStatus,
            int isExported,
            string marketPlace,
            int paymentStatus,
            int orderGroupId)
        {
            // we know that the orderDateFrom and shipmentDateFrom will be initialized when filter was triggerred
            if (orderDateFrom != null || shipmentDateFrom != null)
            {
                orderDateFrom = orderDateFrom.Value.Date;
                orderDateTo = orderDateTo.Value.Date.AddHours(23.9999);
                shipmentDateFrom = shipmentDateFrom.Value.Date;
                shipmentDateTo = shipmentDateTo.Value.Date.AddHours(23.9999);
            }

            var isExportOrder = isExported == 1;
            var isShippedStatus = orderStatus == OrderStatus.Shipped;
            long orderId = 0;
            long.TryParse(searchString, out orderId);

            if (marketPlace == "Manual")
                marketPlace = "Eshopo";

            IEnumerable<order> orders = null;
            if (orderGroupId != -1)
            {
                orders = GetGroupedOrders(searchString,
                                            shippingAddress,
                                            shippingCity,
                                            shippingCountry,
                                           orderDateFrom,
                                           orderDateTo,
                                           shipmentDateFrom,
                                           shipmentDateTo,
                                           orderStatus,
                                           isExported,
                                           marketPlace,
                                           paymentStatus,
                                           isExportOrder,
                                           isShippedStatus,
                                           orderId,
                                           orderGroupId);
            }
            else
            { 
                orders = GetFilteredOrders(searchString,
                                            shippingAddress,
                                            shippingCity,
                                            shippingCountry,
                                            orderDateFrom,
                                            orderDateTo,
                                            shipmentDateFrom,
                                            shipmentDateTo,
                                            orderStatus,
                                            isExported, 
                                            marketPlace,
                                            paymentStatus, 
                                            isExportOrder, 
                                            isShippedStatus, 
                                            orderId);

            }
           

            return orders
                .OrderByDescending(x => x.PurchaseDate)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<order, OrderListViewModel>();
        }

        private IEnumerable<order> GetFilteredOrders(string searchString,
                                                    string shippingAddress,
                                                    string shippingCity,
                                                    string shippingCountry,
                                                    DateTime? orderDateFrom,
                                                    DateTime? orderDateTo, 
                                                    DateTime? shipmentDateFrom,
                                                    DateTime? shipmentDateTo,
                                                    OrderStatus orderStatus,
                                                    int isExported,
                                                    string marketPlace, 
                                                    int paymentStatus, 
                                                    bool isExportOrder, 
                                                    bool isShippedStatus, 
                                                    long orderId)
        {
            var orders = _context.orders
                .GroupJoin(_context.orderitems, o => o.OrderId, oi => oi.OrderId, (o, oi) => new { Order = o, OrderItem = oi.FirstOrDefault() })
                .GroupJoin(_context.orderproducts, ooi => ooi.OrderItem.OrderItemId, op => op.OrderItemId, (ooi, op) => new { Order = ooi.Order, OrderProduct = op.FirstOrDefault() })
                //.GroupJoin(_context.products, o => o.item_.SKU, p => p.EisSKU, (o, p) => new { o.Order, p })
                .Where(m => (string.IsNullOrEmpty(searchString) || (m.Order.OrderId.Contains(searchString)
                                || m.Order.EisOrderId == orderId
                                || m.Order.ShippingAddressName.Contains(searchString)
                                || m.Order.ShippingAddressLine1.Contains(searchString)
                                || m.Order.ShippingCity.Contains(searchString)
                                || m.Order.ShippingStateOrRegion.Contains(searchString)
                                || m.Order.CompanyName.Contains(searchString)
                                //|| m.p.EisSKU.Contains(searchString)
                                //|| m.p.ASIN.Contains(searchString)
                                //|| m.p.Name.Contains(searchString)
                                ))
                    && (orderDateFrom == null || (m.Order.PurchaseDate >= orderDateFrom && m.Order.PurchaseDate <= orderDateTo))
                    && ((!isShippedStatus || shipmentDateFrom == null) || (m.Order.LatestShipDate >= shipmentDateFrom && m.Order.LatestShipDate <= shipmentDateTo))
                    && (orderStatus == OrderStatus.None || m.Order.OrderStatus == orderStatus)
                    && (isExported == -1 || (m.OrderProduct.IsExported == isExportOrder))
                    && (marketPlace == "" || (m.Order.Marketplace == marketPlace))
                    && (paymentStatus == -1 || (m.Order.PaymentStatus == paymentStatus)))
                .GroupBy(x => x.Order)
                .OrderByDescending(x => x.Key.PurchaseDate)
                .Select(m => m.Key);

            if (!String.IsNullOrEmpty(shippingAddress)) { orders = orders.Where(o => o.ShippingAddressLine1.Equals(shippingAddress)); }
            if (!String.IsNullOrEmpty(shippingCity)) { orders = orders.Where(o => o.ShippingCity.Equals(shippingCity)); }
            if (!String.IsNullOrEmpty(shippingCountry)) { orders = orders.Where(o => o.ShippingStateOrRegion.Equals(shippingCountry)); }

            return orders;

        }

        private IEnumerable<order> GetGroupedOrders(string searchString,
                                                    string shippingAddress,
                                                    string shippingCity,
                                                    string shippingCountry,
                                                    DateTime? orderDateFrom, 
                                                    DateTime? orderDateTo, 
                                                    DateTime? shipmentDateFrom,
                                                    DateTime? shipmentDateTo,
                                                    OrderStatus orderStatus, 
                                                    int isExported,
                                                    string marketPlace, 
                                                    int paymentStatus, 
                                                    bool isExportOrder, 
                                                    bool isShippedStatus, 
                                                    long orderId, 
                                                    int ordergroupid)
        {
            var orderGroup = _context.ordergroupdetails.FirstOrDefault(x => x.Id == ordergroupid);
            if (orderGroup == null)
                return null;

            var orders = orderGroup.orders
                .GroupJoin(_context.orderitems, o => o.OrderId, oi => oi.OrderId, (o, oi) => new { Order = o, OrderItem = oi.FirstOrDefault() })
                .GroupJoin(_context.orderproducts, ooi => ooi.OrderItem.OrderItemId, op => op.OrderItemId, (ooi, op) => new { Order = ooi.Order, OrderProduct = op.FirstOrDefault() })
                .Where(m => (string.IsNullOrEmpty(searchString) || (m.Order.OrderId.Contains(searchString)
                                || m.Order.EisOrderId == orderId
                                || m.Order.ShippingAddressName.Contains(searchString)
                                || m.Order.ShippingAddressLine1.Contains(searchString)
                                || m.Order.ShippingCity.Contains(searchString)
                                || m.Order.ShippingStateOrRegion.Contains(searchString)
                                || m.Order.CompanyName.Contains(searchString)

                                ))
                    && (orderDateFrom == null || (m.Order.PurchaseDate >= orderDateFrom && m.Order.PurchaseDate <= orderDateTo))
                    && ((!isShippedStatus || shipmentDateFrom == null) || (m.Order.LatestShipDate >= shipmentDateFrom && m.Order.LatestShipDate <= shipmentDateTo))
                    && (orderStatus == OrderStatus.None || m.Order.OrderStatus == orderStatus)
                    && (isExported == -1 || (m.OrderProduct.IsExported == isExportOrder))
                    && (marketPlace == "" || (m.Order.Marketplace == marketPlace))
                    && (paymentStatus == -1 || (m.Order.PaymentStatus == paymentStatus)))
                .GroupBy(x => x.Order)
                .OrderByDescending(x => x.Key.PurchaseDate)
                .Select(m => m.Key);

            if (!String.IsNullOrEmpty(shippingAddress)) { orders = orders.Where(o => o.ShippingAddressLine1.Equals(shippingAddress)); }
            if (!String.IsNullOrEmpty(shippingCity)) { orders = orders.Where(o => o.ShippingCity.Equals(shippingCity)); }
            if (!String.IsNullOrEmpty(shippingCountry)) { orders = orders.Where(o => o.ShippingStateOrRegion.Equals(shippingCountry)); }

            return orders;
        }

        public OrderViewModel GetOrderById(string orderId)
        {
            var order = _context.orders.FirstOrDefault(x => x.OrderId == orderId);

            return Mapper.Map<order, OrderViewModel>(order);
        }

        public IEnumerable<OrderItemViewModel> GetOrderUnshippedItems(string orderId)
        {
            var orderItems = _context.orderitems
                .Where(x => x.OrderId == orderId && (x.QtyOrdered - x.QtyShipped ?? 0) > 0)
                .ToList();

            return Mapper.Map<IEnumerable<OrderItemViewModel>>(orderItems);
        }

        public void LogOrderShipment(OrderShipmentViewModel shipmentModel)
        {
            // let's check first if it's already in the database
            var oldShipment = _context.ordershipments.FirstOrDefault(x => x.OrderId == shipmentModel.OrderId);
            if (oldShipment == null)
            {
                var shipment = Mapper.Map<OrderShipmentViewModel, ordershipment>(shipmentModel);
                shipment.Created = DateTime.Now;
                _context.ordershipments.Add(shipment);
            }
            else
            {
                // update the existing data
                shipmentModel.Modified = DateTime.Now;
                _context.Entry(oldShipment).CurrentValues.SetValues(shipmentModel);
            }

            _context.SaveChanges();
        }

        public void UpdateOrderShipment(OrderShipmentViewModel shipmentModel)
        {
            shipmentModel.Modified = DateTime.Now;
            var oldShipment = _context.ordershipments.FirstOrDefault(x => x.OrderId == shipmentModel.OrderId);
            _context.Entry(oldShipment).CurrentValues.SetValues(shipmentModel);

            _context.SaveChanges();
        }

        public void UpdateOrder(OrderViewModel orderModel)
        {
            var oldOrder =  _context.orders.FirstOrDefault(x => x.OrderId == orderModel.OrderId);

            if (oldOrder.Marketplace == "Eshopo")
            {
                oldOrder.PaymentStatus = orderModel.PaymentStatus;
                oldOrder.ShippingAddressLine1 = orderModel.ShippingAddressLine1;
                oldOrder.ShippingStateOrRegion = orderModel.ShippingStateOrRegion;
                oldOrder.ShippingPostalCode = orderModel.ShippingPostalCode;
                oldOrder.ShippingCity = orderModel.ShippingCity;
                oldOrder.CompanyName = orderModel.CompanyName;
            }
            oldOrder.OrderNote = orderModel.OrderNote;
  

            _context.SaveChanges();
        }
        
        public int GetNextEisOrderId()
        {
            var maxEisOrderId = 1000000;
            try
            {
                var existingMaxEisOrderId = _context.orders.Max(x => x.EisOrderId);
                maxEisOrderId = existingMaxEisOrderId > 0 ? existingMaxEisOrderId : maxEisOrderId;
            }
            catch { }
            return maxEisOrderId + 1;
        }

        public OrderViewModel SaveManualOrder(OrderViewModel orderModel)
        {
            var order = Mapper.Map<OrderViewModel, order>(orderModel);
            order.OrderStatus = OrderStatus.Unshipped;
            order.EisOrderId = GetNextEisOrderId();
            order.ShippingAddressName = order.BuyerName;
            order.Created = DateTime.UtcNow;

            _context.orders.Add(order);
            _context.SaveChanges();

            // determine and add the order products for this order
            var results = new List<OrderProductResult>();
            foreach(var orderItem in order.orderitems)
            {
                // convert the order item into marketplace order - THIS IS BAD THOUGH
                var marketplaceOrderItem = Mapper.Map<MarketplaceOrderItem>(orderItem);
                var result = ManageOrderVendorProduct(marketplaceOrderItem);
                results.Add(result);
            }  
            
            // notify Admin if there are insufficient products for the orders
            EvaluateForInsufficientVendorProducts(results);

            return Mapper.Map<OrderViewModel>(order);
        }

        public int SaveMarketplaceOrders(List<MarketplaceOrder> orderResults)
        {
            try
            {
                var results = new List<OrderProductResult>();

                // iterate and convert the order results into order domain object
                foreach (var orderResult in orderResults)
                {
                    // let's check first if this marketplace order already exist to the db
                    var existingOrder = _context.orders.FirstOrDefault(x => x.OrderId == orderResult.OrderId);
                    if (existingOrder != null)
                        continue;

                    // map the order result into order domain and set the its EIS order Id
                    var order = Mapper.Map<order>(orderResult);
                    order.EisOrderId = GetNextEisOrderId();
                    _context.orders.Add(order);

                    foreach(var orderItem in orderResult.OrderItems)
                    {
                        // create also the order update history for orderitems
                        _context.orderupdatehistories.Add(new orderupdatehistory
                        {
                            OrderItemId = orderItem.OrderItemId,
                            QtyOrdered = orderItem.QtyOrdered,
                            OrderStatus = orderResult.OrderStatus,
                            PurchaseDate = orderResult.PurchaseDate,
                            ResultDate = DateTime.UtcNow
                        });

                        // create order products and update the product inventory
                        var result = ManageOrderVendorProduct(orderItem);
                        results.Add(result);
                    }

                    // let's save the changes 1by1 so we can get its EIS OrderID
                    _context.SaveChanges();
                }
                
                // notify Admin if there are insufficient products for the orders
                EvaluateForInsufficientVendorProducts(results);
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.OrderService, errorMsg, ex.StackTrace);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.OrderService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                return 0;
            }

            return orderResults.Count;
        }

        public OrderViewModel UpdateManualOrder(string orderId, OrderViewModel orderModel)
        {
            var existingOrder = _context.orders.FirstOrDefault(x => x.OrderId == orderId);
            if (existingOrder == null)
                return null;

            var updatedOrder = Mapper.Map<OrderViewModel, order>(orderModel);
            var updatedOrderItems = updatedOrder.orderitems;
            var existingOrderItems = existingOrder.orderitems;

            // let's retain the date when the order was created
            updatedOrder.Created = existingOrder.Created;

            // find the newly added order items (order items came from client - existing order items = new added orderitems)
            var addedOrderItems = updatedOrderItems.Except(existingOrderItems, (o1, o2) => (o1.OrderItemId == o2.OrderItemId && o1.SKU == o2.SKU));

            // find deleted order items by exising orderitems - updated orderitems = deleted orderitems
            var deletedOrderItems = existingOrderItems.Except(updatedOrderItems, (o1, o2) => (o1.OrderItemId == o2.OrderItemId && o1.SKU == o2.SKU));

            // find modified orderitems by updated orderitems - added orderitems = modified orderitems
            var modifiedOrderItems = updatedOrderItems.Except(addedOrderItems, (o1, o2) => (o1.OrderItemId == o2.OrderItemId && o1.SKU == o2.SKU)).ToList();
            // apply the modified orderitems to the current property value 
            foreach (var item in modifiedOrderItems)
            {
                var existingOrderItem = _context.orderitems.FirstOrDefault(x => x.OrderItemId == item.OrderItemId && x.SKU == item.SKU);

                // get DBEntity object for the existing orderitem entity
                var orderItemEntry = _context.Entry(existingOrderItem);
                orderItemEntry.CurrentValues.SetValues(item);
            }

            // mark all added orderitemss entity state to Added
            addedOrderItems.ToList().ForEach(item => _context.Entry(item).State = EntityState.Added);

            // mark all deleted order items entity state to Deleted
            deletedOrderItems.ToList().ForEach(item => _context.Entry(item).State = EntityState.Deleted);            

            // get DBEnity object for the existing order entity
            var orderEntry = _context.Entry(existingOrder);
            orderEntry.CurrentValues.SetValues(updatedOrder);

            // let's save the changes
            _context.SaveChanges();

            return orderModel;
        }

        public void UpdateOrderProducts(string orderId)
        {
            var order = _context.orders.FirstOrDefault(x => x.OrderId == orderId);
            if (order == null)
                return;

            // determine and add the order products for this order
            var results = new List<OrderProductResult>();
            foreach (var orderItem in order.orderitems)
            {
                // get all the order products for this order and delete and return the quantity
                var orderProducts = orderItem.orderproducts.ToList();
                if (orderProducts.Any())
                {
                    UpdateVendorProductInventory(orderProducts, true);

                    // delete the order products for this item
                    _context.orderproducts.RemoveRange(orderProducts);
                    _context.SaveChanges();
                }

                // convert the order item into marketplace order - THIS IS BAD THOUGH
                var marketplaceOrderItem = Mapper.Map<MarketplaceOrderItem>(orderItem);
                var result = ManageOrderVendorProduct(marketplaceOrderItem);
                results.Add(result);
            }

            // notify Admin if there are insufficient products for the orders
            EvaluateForInsufficientVendorProducts(results);
        }

        public OrderListViewModel UpdateMarketplaceOrder(string marketplaceOrderId, MarketplaceOrder order)
        {
            // let's get first the order data
            var existingOrder = _context.orders.FirstOrDefault(x => x.OrderId == marketplaceOrderId);
            if (existingOrder == null)
                return null;

            // update each properties of the existing order one by one
            existingOrder.Marketplace = order.Marketplace;
            existingOrder.OrderTotal = order.OrderTotal;
            existingOrder.NumOfItemsShipped = (int)order.NumOfItemsShipped;
            existingOrder.NumOfItemsUnshipped = (int)order.NumOfItemsUnshipped;
            existingOrder.OrderStatus = order.OrderStatus;
            existingOrder.PurchaseDate = order.PurchaseDate;
            existingOrder.LastUpdateDate = order.LastUpdateDate;
            existingOrder.PaymentMethod = order.PaymentMethod;
            existingOrder.BuyerName = order.BuyerName;
            existingOrder.BuyerEmail = order.BuyerEmail;
            existingOrder.ShippingAddressPhone = order.ShippingAddressPhone;
            existingOrder.ShippingAddressName = order.ShippingAddressName;
            existingOrder.ShippingAddressLine1 = order.ShippingAddressLine1;
            existingOrder.ShippingAddressLine2 = order.ShippingAddressLine2;
            existingOrder.ShippingAddressLine3 = order.ShippingAddressLine3;
            existingOrder.ShippingCity = order.ShippingCity;
            existingOrder.ShippingStateOrRegion = order.ShippingStateOrRegion;
            existingOrder.ShippingPostalCode = order.ShippingPostalCode;
            existingOrder.ShipServiceLevel = order.ShipServiceLevel;
            existingOrder.ShipmentServiceCategory = order.ShipmentServiceCategory;
            existingOrder.EarliestShipDate = order.EarliestShipDate;
            existingOrder.LatestShipDate = order.LatestShipDate;
            existingOrder.EarliestDeliveryDate = order.EarliestDeliveryDate;
            existingOrder.LatestDeliveryDate = order.LatestDeliveryDate;
            existingOrder.OrderType = order.OrderType;
            existingOrder.MarketplaceId = order.MarketplaceId;
            existingOrder.PurchaseOrderNumber = order.PurchaseOrderNumber;
            existingOrder.SalesChannel = order.SalesChannel;
            existingOrder.SellerOrderId = order.SellerOrderId;
            existingOrder.AdjustmentAmount = order.AdjustmentAmount;
            existingOrder.AmountPaid = order.AmountPaid;
            existingOrder.PaymentOrRefundAmount = order.PaymentOrRefundAmount;
            existingOrder.CompanyName = order.CompanyName;

            // save the changes first
            _context.SaveChanges();

            // then, its order items
            foreach (var item in order.OrderItems)
            {
                // check if has an existing order item
                var existingOrderItem = _context.orderitems
                    .FirstOrDefault(x => x.OrderId == item.OrderId && x.OrderItemId == item.OrderItemId);
                if(existingOrderItem == null)
                {
                    existingOrderItem = new orderitem
                    {
                        OrderId = item.OrderId,
                        OrderItemId = item.OrderItemId
                    };
                    _context.orderitems.Add(existingOrderItem);
                }

                // set it's values
                existingOrderItem.ItemId = item.MarketplaceItemId;
                existingOrderItem.SKU = item.SKU;
                existingOrderItem.Title = item.Title;
                existingOrderItem.QtyOrdered = item.QtyOrdered;
                existingOrderItem.QtyShipped = item.QtyShipped;
                existingOrderItem.Price = item.Price;
                existingOrderItem.ShippingPrice = item.ShippingPrice;
                existingOrderItem.GiftWrapPrice = item.GiftWrapPrice;
                existingOrderItem.ItemTax = item.Tax;
                existingOrderItem.ShippingTax = item.ShippingTax;
                existingOrderItem.GiftWrapTax = item.GiftWrapTax;
                existingOrderItem.ShippingDiscount = item.ShippingDiscount;
                existingOrderItem.PromotionDiscount = item.PromotionDiscount;
                existingOrderItem.ConditionNote = item.ConditionNote;
            }

            // save the changes
            _context.SaveChanges();

            return Mapper.Map<OrderListViewModel>(existingOrder);
        }

        public void ToggleOrderExportValue(bool isExported, bool isSelectAllPages, List<int> eisOrderIds)
        {
            if (!isSelectAllPages && !eisOrderIds.Any())
                return;

            // if null, create an empty list
            if (eisOrderIds == null)
                eisOrderIds = new List<int>();

            // get the order items 
            var orderProducts = _context.orderproducts
                    .Join(_context.orderitems,
                            op => op.OrderItemId,
                            oi => oi.OrderItemId,
                            (op, oi) => new { OrderProduct = op, OrderItem = oi })
                   .Join(_context.orders.Where(x => (isSelectAllPages && !eisOrderIds.Contains(x.EisOrderId)) || eisOrderIds.Contains(x.EisOrderId)),
                            ooi => ooi.OrderItem.OrderId,
                            o => o.OrderId,
                            (ooi, o) => new { ooi.OrderProduct })
                   .Select(x => x.OrderProduct)
                   .ToList();

            // toggle the IsExported
            orderProducts.ForEach(o =>
            {
                o.IsExported = isExported;
                o.ExportedDate = isExported ? (DateTime?)DateTime.UtcNow : null;
            });

            // save the changes
            _context.SaveChanges();
        }

        public List<PendingOrderViewModel> GetPendingOrders(string eisSupplierSKU)
        {
            // get the number of order items which are Unshipped/Pending for this item
            var pendingOrders = _context.orderproducts.Where(x => x.EisSupplierSKU == eisSupplierSKU)
                .Join(_context.orderitems,
                        op => op.OrderItemId,
                        oi => oi.OrderItemId,
                        (op, oi) => new { OrderProduct = op, OrderItem = oi })
               .Join(_context.orders.Where(x => x.OrderStatus == OrderStatus.Unshipped || x.OrderStatus == OrderStatus.Pending),
                        ooi => ooi.OrderItem.OrderId,
                        o => o.OrderId,
                        (ooi, o) => new { ooi.OrderProduct, Order = o })
               .Select(x => new PendingOrderViewModel
               {
                   EisOrderId = x.Order.EisOrderId,
                   OrderId = x.Order.OrderId,
                   Marketplace = x.Order.Marketplace,
                   Quantity = x.OrderProduct.Quantity
               })
               .ToList();

            return pendingOrders;
        }


        // TODO: NEED TO ADD IMPLEMENTATION CODE
        public string CustomExportOrderAsync(ExportOrder model)
        {
            var sqlQuery = string.Empty;
            var filePath = string.Format("{1}\\{0:MM}{0:dd}{0:yyyy}-{0:HH}{0:mm}{0:ss}_CustomExportOrders.csv", model.RequestedDate, _exportFolder);
            try
            {
                using (var streamWriter = new StreamWriter(filePath))
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, createCustomExportSqlQuery(model),null);
                        var config = new CsvConfiguration();
                        config.Delimiter = model.Delimiter;

                        var csvWriter = new CsvWriter(streamWriter, config);

                        // write the header text for the CSV files
                        foreach (var field in model.OrderFieldsArr)
                        {
                            csvWriter.WriteField(removePrefixTable(field));
                        }
                           
                        csvWriter.NextRecord();

                        while (reader.Read())
                        {
                            foreach (var field in model.OrderFieldsArr)
                            {
                                var fieldName = removePrefixTable(field);
                                if(fieldName=="TrackingNumber")
                                {
                                    var trackingNumber = reader[fieldName].ToString();
                                    if (!String.IsNullOrEmpty(trackingNumber))
                                    {
                                        if(trackingNumber.Length == 22)
                                        {
                                            //Format USPS Tracking No.
                                            trackingNumber = Regex.Replace(trackingNumber, @"^(..)(....)(....)(....)(....)(....)$", "$1-$2-$3-$4-$5-$6");
                                            csvWriter.WriteField(trackingNumber);
                                        }
                                        else
                                        {
                                            //Format FedEx Tracking Number
                                            trackingNumber = Regex.Replace(trackingNumber, @"^(..)(....)(....)(....)(....)(..)$", "$1-$2-$3-$4-$5-$6");
                                            csvWriter.WriteField(trackingNumber);
                                        }
                                    }
                                    else
                                    {
                                        csvWriter.WriteField(trackingNumber);
                                    }

                                }
                                else
                                {
                                    csvWriter.WriteField(reader[fieldName]);
                                }
                            }

                            csvWriter.NextRecord();
                        }

                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            
            return filePath;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                    // Dispose other managed resources.
                }
                //release unmanaged resources.
            }
            _disposed = true;
        }
        #endregion


        #region Shared methods with the console applications
        public OrderProductResult ManageOrderVendorProduct(MarketplaceOrderItem orderItem)
        {
            var orderProducts = new List<orderproduct>();
            var vendorProducts = new List<vendorproduct>();
            var shadowProduct = new shadow();
            OrderProductResult result;

            using (var context = new EisInventoryContext())
            {
                // let's get all its vendor products via its links
                vendorProducts = context.vendorproductlinks
                    .Where(x => x.EisSKU == orderItem.SKU && x.IsActive)
                    .Select(x => x.vendorproduct)
                    .ToList();

                // let's determine if this product is a shadow sku; WHY NOT USE THE SKUType of product? O.o
                shadowProduct = context.shadows.FirstOrDefault(x => x.ShadowSKU == orderItem.SKU && x.IsConnected);
            }

            if (shadowProduct != null)
            {
                // let's take out the compatible vendor products that has available stock and sort by price asc
                var compatibleVendorProducts = vendorProducts
                    .Where(x => x.Quantity > 0 && (shadowProduct.FactorQuantity % x.MinPack) == 0)
                    .OrderBy(x => x.SupplierPrice)
                    .ToList();

                // iterate and determine the qualified vendor products
                orderProducts = createOrderProducts(compatibleVendorProducts, orderItem.OrderItemId, orderItem.QtyOrdered, shadowProduct.FactorQuantity);
                result = new OrderProductResult { ItemPack = shadowProduct.FactorQuantity };
            }
            else
            {
                // let's take out the compatible vendor products that has available stock and sort by price asc
                var compatibleVendorProducts = vendorProducts
                    .Where(x => x.Quantity > 0 && x.MinPack == 1)
                    .OrderBy(x => x.SupplierPrice)
                    .ToList();

                // iterate and determine the qualified vendor products
                orderProducts = createOrderProducts(compatibleVendorProducts, orderItem.OrderItemId, orderItem.QtyOrdered, 1);
                result = new OrderProductResult { ItemPack = 1 };
            }

            // save the order products to the database
            addOrderProducts(orderProducts);

            // update the vendor product's quantity
            UpdateVendorProductInventory(orderProducts, false);

            result.OrderProducts = Mapper.Map<List<OrderProduct>>(orderProducts);
            result.OrderItem = orderItem;
            return result;
        }

        public bool UpdateVendorProductInventory(List<orderproduct> orderProducts, bool isOrderCanceled)
        {
            if (!orderProducts.Any())
                return false;

            using (var context = new EisInventoryContext())
            {
                // iterate and return the quantity for the vendor product
                foreach (var item in orderProducts)
                {
                    // get the vendor product
                    var vendorProduct = context.vendorproducts
                        .FirstOrDefault(x => x.EisSupplierSKU == item.EisSupplierSKU);
                    if (vendorProduct == null || vendorProduct.vendor.IsAlwaysInStock)
                        continue;

                    if (isOrderCanceled)
                        vendorProduct.Quantity += item.Quantity;
                    else
                        vendorProduct.Quantity -= item.Quantity;
                }

                // save the changes
                context.SaveChanges();
            }

            return true;
        }

        public void EvaluateForInsufficientVendorProducts(List<OrderProductResult> orderResults)
        {
            foreach (var result in orderResults)
            {
                // send email to the Admin if it doesn't match
                if(result.TotalAvailableItems != result.TotalOrderedItems)
                    _emailService.SendEmailForInsufficientProducts(result);
            }
        }

        // IF YOU HAVE CHANGES ON THIS CODE: PLEASE CHECK ALSO THE CODE IN EIS.Inventory.Core.Services.OrderService - SUCKS!
        private void addOrderProducts(List<orderproduct> orderProducts)
        {
            if (!orderProducts.Any())
                return;

            using (var context = new EisInventoryContext())
            {
                // TODO: NEED TO CHECK IF THE ORDER ITEM ID AND ORDER PRODUCT EXISTENCE
                context.orderproducts.AddRange(orderProducts);
                context.SaveChanges();
            }
        }

        // IF YOU HAVE CHANGES ON THIS CODE: PLEASE CHECK ALSO THE CODE IN EIS.Inventory.Core.Services.OrderService - SUCKS!
        private List<orderproduct> createOrderProducts(List<vendorproduct> compatibleVendorProducts, string orderItemId, int qytOrdered, int factorQty)
        {
            var orderProducts = new List<orderproduct>();
            var totalOrderedItems = qytOrdered * factorQty;

            // let's get first the vendor products who can fully supplied for this ordered item, the list is already sorted by its price
            var qualifiedVendorProduct = compatibleVendorProducts.FirstOrDefault(x => x.TotalItems >= totalOrderedItems);
            if (qualifiedVendorProduct != null)
            {
                // determine how many items/packs required for this vendor product
                var itemsRequired = totalOrderedItems / qualifiedVendorProduct.MinPack;

                // otherwise, create order product for this item
                orderProducts.Add(new orderproduct
                {
                    OrderItemId = orderItemId,
                    EisSupplierSKU = qualifiedVendorProduct.EisSupplierSKU,
                    Quantity = itemsRequired,
                    Pack = qualifiedVendorProduct.MinPack,
                    IsExported = false,
                    IsPoGenerated = false,
                    Created = DateTime.UtcNow
                });
            }
            else
            {
                // iterate and let's take out the remaining available stock for each compatible vendor products
                foreach (var vendorProduct in compatibleVendorProducts)
                {
                    // exit if there are no remaining items left
                    if (totalOrderedItems <= 0)
                        break;

                    // continue if this vendorproduct has no available stock, this should not be happenned
                    if (vendorProduct.Quantity < 1)
                        continue;

                    // determine how many items/packs required for this vendor product
                    var itemsRequired = totalOrderedItems / vendorProduct.MinPack;

                    // if the vendor product can't supply all for this ordered items, then let's use its remaining stock
                    if (itemsRequired > vendorProduct.Quantity)
                    {
                        itemsRequired = getAllowedQuantity(vendorProduct.Quantity, vendorProduct.MinPack, factorQty);
                        if (itemsRequired == 0) continue;
                    }

                    // otherwise, create order product for this item
                    orderProducts.Add(new orderproduct
                    {
                        OrderItemId = orderItemId,
                        EisSupplierSKU = vendorProduct.EisSupplierSKU,
                        Quantity = itemsRequired,
                        Pack = vendorProduct.MinPack,
                        IsExported = false,
                        IsPoGenerated = false,
                        Created = DateTime.UtcNow
                    });

                    // update the remaining item
                    totalOrderedItems -= (itemsRequired * vendorProduct.MinPack);
                }
            }

            return orderProducts;
        }

        private int getAllowedQuantity(int itemsRequired, int minPack, int factorQty)
        {
            var result = itemsRequired * minPack;
            if (result % factorQty == 0)
                return itemsRequired;

            return getAllowedQuantity(itemsRequired - 1, minPack, factorQty);
        }


        #endregion

        private string createCustomExportSqlQuery(ExportOrder model)
        {
            var sqlQuery = string.Empty;
            var sqlPredicate = new StringBuilder();

            if (model.IsAllOderItems)
            {

                var orderQuery = @"
                                    SELECT {0} FROM orders
                                    LEFT JOIN orderitems ON orderitems.OrderId = orders.OrderId 
                                    {1} {2};";

                if (model.ExcludedEisOrderIdArr.Any())
                {
                    sqlPredicate.Append(" AND orders.EisOrderId NOT IN(");
                    sqlPredicate.AppendFormat("'{0}'", string.Join("','", model.ExcludedEisOrderIdArr));
                    sqlPredicate.Append(") ");

                }

                sqlQuery = string.Format(orderQuery, string.Join(",", model.OrderFields)
                                                                    , sqlPredicate.ToString()
                                                                    , (string.IsNullOrEmpty(model.SortBy) ? string.Empty : (" ORDER BY " + model.SortBy)));
            }
            else
            {
                sqlQuery = @"
                                    SELECT {0} FROM orders
                                    LEFT JOIN orderitems ON orderitems.OrderId = orders.OrderId
                                    WHERE orders.EisOrderId IN ('{1}') {2};";

                sqlQuery = string.Format(sqlQuery, string.Join(",", model.OrderFields)
                                                                 , string.Join("','", model.SelectedEisOrderIdArr)
                                                                 , (string.IsNullOrEmpty(model.SortBy) ? string.Empty : (" ORDER BY " + model.SortBy)));
            }


            return sqlQuery;
        }

        private string removePrefixTable(string columnName)
        {
             return columnName.Split('.')[1];
        }
    }
}
