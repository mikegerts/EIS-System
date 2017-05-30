using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using System.Linq;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class CsvFileDataParser
    {
        public static Message ParseOrderFile(StreamReader reader, List<Order> orders,List<OrderItem> orderItemModel, bool hasHeader, string customerOrderHeaderFieldNames, string orderColumnName)
        {
            var message = new Message();
            try
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        var order = new Order();

                        string[] orderColumnNamesArray = orderColumnName.Split(',');
                        string[] customerOrderHeaderFieldNameArray = customerOrderHeaderFieldNames.Split(',');

                        bool isAlreadyAdded = false;

                        for (int i = 0; i < orderColumnNamesArray.Length; i++)
                        {
                            string strCellValue = null;
                            int intCellvalue = 0;
                            decimal decimalCellValue = 0;
                            DateTime? datetimeCellValue = null;

                            if ("OrderId" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.OrderId = strCellValue;
                                isAlreadyAdded = orders.Any(x => x.OrderId == order.OrderId);

                                if (isAlreadyAdded)
                                    break;
                            }
                            else if ("EisOrderId" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                                order.EisOrderId = intCellvalue;
                            }
                            else if ("Marketplace" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.Marketplace = strCellValue;
                            }
                            else if ("OrderTotal" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                                order.OrderTotal = decimalCellValue;
                            }
                            else if ("OrderStatus" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                                order.OrderStatus = intCellvalue;
                            }
                            else if ("PaymentStatus" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                                order.PaymentStatus = intCellvalue;
                            }
                            else if ("NumOfItemsShipped" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                                order.NumOfItemsShipped = decimalCellValue;
                            }
                            else if ("NumOfItemsUnshipped" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                                order.NumOfItemsUnshipped = decimalCellValue;
                            }
                            else if ("PurchaseDate" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<DateTime?>(customerOrderHeaderFieldNameArray[i], out datetimeCellValue);
                                order.PurchaseDate = datetimeCellValue;
                            }
                            else if ("LastUpdateDate" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<DateTime?>(customerOrderHeaderFieldNameArray[i], out datetimeCellValue);
                                order.LastUpdateDate = datetimeCellValue;
                            }
                            else if ("PaymentMethod" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.PaymentMethod = strCellValue;
                            }
                            else if ("CompanyName" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.CompanyName = strCellValue;
                            }
                            else if ("BuyerName" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.BuyerName = strCellValue;
                            }
                            else if ("BuyerEmail" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.BuyerEmail = strCellValue;
                            }
                            else if ("ShippingAddressPhone" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingAddressPhone = strCellValue;
                            }
                            else if ("ShippingAddressName" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingAddressName = strCellValue;
                            }
                            else if ("ShippingAddressLine1" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingAddressLine1 = strCellValue;
                            }
                            else if ("ShippingAddressLine2" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingAddressLine2 = strCellValue;
                            }
                            else if ("ShippingAddressLine3" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingAddressLine3 = strCellValue;
                            }
                            else if ("ShippingCity" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingCity = strCellValue;
                            }
                            else if ("ShippingStateOrRegion" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingStateOrRegion = strCellValue;
                            }
                            else if ("ShippingPostalCode" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingPostalCode = strCellValue;
                            }
                            else if ("ShipServiceLevel" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShipServiceLevel = strCellValue;
                            }
                            else if ("ShipmentServiceCategory" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShipmentServiceCategory = strCellValue;
                            }

                            else if ("ShipmentDate" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<DateTime?>(customerOrderHeaderFieldNameArray[i], out datetimeCellValue);
                                order.ShipmentDate = datetimeCellValue;
                            }
                            else if ("CarrierCode" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.CarrierCode = strCellValue;
                            }
                            else if ("ShippingMethod" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.ShippingMethod = strCellValue;
                            }
                            else if ("TrackingNumber" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.TrackingNumber = strCellValue;
                            }
                            else if ("ShipmentCost" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                                order.ShipmentCost = decimalCellValue;
                            }
                            else if ("OrderNote" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                order.OrderNote = strCellValue;
                            }

                        }
                        if (!isAlreadyAdded)
                        {
                            var currentOrder_ItemList = orderItemModel.Where(x => x.OrderId == order.OrderId).ToList();

                            if (currentOrder_ItemList.Count > 0)
                            {
                                order.OrderItems = new List<OrderItem>();
                                order.OrderItems.AddRange(currentOrder_ItemList);
                                orders.Add(order);
                            }
                        }

                        message.IsSucess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Vendor Product file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }

        public static List<OrderItem> GetOrderItems(StreamReader reader, bool hasHeader, string customerOrderHeaderFieldNames, string orderColumnName)
        {
            var orderItemModel = new List<OrderItem>();
            using (var csvReader = new CsvReader(reader))
            {
                csvReader.Configuration.HasHeaderRecord = hasHeader;
                int iterator = 0;
                string orderId = "";
                while (csvReader.Read())
                {
                    var orderItem = new OrderItem();

                    string[] orderColumnNamesArray = orderColumnName.Split(',');
                    string[] customerOrderHeaderFieldNameArray = customerOrderHeaderFieldNames.Split(',');
                    

                    for (int i = 0; i < orderColumnNamesArray.Length; i++)
                    {
                        string strCellValue = null;
                        int intCellvalue = 0;
                        decimal decimalCellValue = 0;
                        DateTime? datetimeCellValue = null;

                        if ("OrderId" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                            orderItem.OrderId = strCellValue;
                        }
                        else if ("SKU" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                            orderItem.SKU = strCellValue;
                        }
                        else if ("Title" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                            orderItem.Title = strCellValue;
                        }
                        else if ("QtyOrdered" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                            orderItem.QtyOrdered = intCellvalue;
                        }
                        else if ("QtyShipped" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                            orderItem.QtyShipped = intCellvalue;
                        }
                        else if ("Price" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<int>(customerOrderHeaderFieldNameArray[i], out intCellvalue);
                            orderItem.Price = intCellvalue;
                        }
                        else if ("ShippingPrice" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.ShippingPrice = decimalCellValue;
                        }
                        else if ("GiftWrapPrice" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.GiftWrapPrice = decimalCellValue;
                        }
                        else if ("ItemTax" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.ItemTax = decimalCellValue;
                        }
                        else if ("ShippingTax" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.ShippingTax = decimalCellValue;
                        }
                        else if ("GiftWrapTax" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.GiftWrapTax = decimalCellValue;
                        }
                        else if ("ShippingDiscount" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.ShippingDiscount = decimalCellValue;
                        }
                        else if ("PromotionDiscount" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<decimal>(customerOrderHeaderFieldNameArray[i], out decimalCellValue);
                            orderItem.PromotionDiscount = decimalCellValue;
                        }
                        else if ("ConditionNote" == orderColumnNamesArray[i])
                        {
                            csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                            orderItem.ConditionNote = strCellValue;
                        }
                    }

                    if (string.IsNullOrEmpty(orderId))
                        orderId = orderItem.OrderId;
                    else if (orderId != orderItem.OrderId)
                    {
                        orderId = orderItem.OrderId;
                        iterator = 0;
                    }
                    orderItem.OrderItemId = orderItem.OrderId + "-" + (iterator + 1);
                    orderItemModel.Add(orderItem);
                    iterator++;
                }
            }

            return orderItemModel;
        }

        public static Message ParseCustomerWholeSalePriceSkuFile(StreamReader reader, List<Product> skuList, bool hasHeader, string FieldNames, string ColumnName)
        {
            var message = new Message();
            try
            {
                using (var csvReader = new CsvReader(reader))
                {
                    csvReader.Configuration.HasHeaderRecord = hasHeader;
                    while (csvReader.Read())
                    {
                        var product = new Product();

                        string[] orderColumnNamesArray = ColumnName.Split(',');
                        string[] customerOrderHeaderFieldNameArray = FieldNames.Split(',');

                        bool isAlreadyAdded = false;

                        for (int i = 0; i < orderColumnNamesArray.Length; i++)
                        {
                            string strCellValue = null;
                            isAlreadyAdded = false;

                            if ("EisSKU" == orderColumnNamesArray[i])
                            {
                                csvReader.TryGetField<string>(customerOrderHeaderFieldNameArray[i], out strCellValue);
                                product.EisSKU = strCellValue;
                                isAlreadyAdded = skuList.Any(x => x.EisSKU == product.EisSKU);

                                if (isAlreadyAdded)
                                    break;
                            }
                            if (!isAlreadyAdded)
                                skuList.Add(product);

                        }

                        message.IsSucess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                message.SetMessage(false, string.Format("Vendor Product file upload failed: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
            }
            return message;
        }
    }
}