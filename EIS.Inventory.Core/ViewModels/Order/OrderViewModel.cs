using EIS.Inventory.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EIS.Inventory.Core.ViewModels
{
    public class OrderViewModel
    {
        /// <summary>
        /// Identifies what marketplace it came from
        /// </summary>
        public string Marketplace { get; set; }

        /// <summary>
        /// Gets or sets the EIS Order Id
        /// </summary>
        public int EisOrderId { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the order
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the number of item shipped
        /// </summary>
        public decimal NumOfItemsShipped { get; set; }

        /// <summary>
        /// Gets or sets the number of item unshipped
        /// </summary>
        public decimal NumOfItemsUnshipped { get; set; }

        /// <summary>
        /// Gets or sets the name of the buyer
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// Gets or sets the buyer's  email address
        /// </summary>
        public string BuyerEmail { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shiping phone number
        /// </summary>
        public string ShippingAddressPhone { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping address name
        /// </summary>
        public string ShippingAddressName { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping first part of the address
        /// </summary>
        public string ShippingAddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping second part of the address
        /// </summary>
        public string ShippingAddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping third part of the address
        /// </summary>
        public string ShippingAddressLine3 { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping city
        /// </summary>
        public string ShippingCity { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping state
        /// </summary>
        public string ShippingStateOrRegion { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping postal code
        /// </summary>
        public string ShippingPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was created
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets the last update date of the order
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the payment method for the order
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the level of service for the ship (ground, expediated, next day).
        /// </summary>
        public string ShipServiceLevel { get; set; }

        /// <summary>
        /// Gets or sets the category of service for the shipment (ground, expediated, next day).
        /// </summary>
        public string ShipmentServiceCategory { get; set; }

        /// <summary>
        /// Gets or sets the status of the order
        /// </summary>
        public OrderStatus OrderStatus { get; set; }
        /// <summary>
        /// Gets or sets the Payment Status of the order
        /// </summary>
        public int PaymentStatus { get; set; }

        public string OrderStatusStr { get { return OrderStatus.ToString(); } }
        public string PaymentStatusStr
        {
            get
            {
                OrderPaymentStatus orderPaymentStatus = (OrderPaymentStatus)PaymentStatus;
                return orderPaymentStatus.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the list of order items
        /// </summary>
        public List<OrderItemViewModel> OrderItems { get; set; }

        /// <summary>
        /// Get the total items ordered
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the flag whether this order was exported by the Scheduler
        /// </summary>
        public bool IsExported { get; set; }

        [DataType(DataType.MultilineText)]
        public string OrderNote { get; set; }

        public DateTime ShipmentDate { get; set; }

        public string CarrierCode { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public decimal ShipmentCost { get; set; }
        public bool HasOrderProducts { get; set; }
        public string MarketPlaceUrl {
            get
            {
                var marketPlaceUrl = "";
                if (Marketplace == MarketPlaceTypes.Values.Amazon)
                {
                    marketPlaceUrl = "https://sellercentral.amazon.com/hz/orders/details?_encoding=UTF8&orderId=" + OrderId;
                }
                else if(Marketplace == MarketPlaceTypes.Values.BigCommerce)
                {
                    marketPlaceUrl = string.Format("https://store-mxi5a3.mybigcommerce.com/manage/orders/{0}", OrderId);
                }
                return marketPlaceUrl;
            }
        }
        public double OrderTax
        {
            get
            {
                double taxtTotal = 0;
                if(OrderItems != null)
                    foreach (var orderitem in OrderItems)
                    {
                        taxtTotal += (double)(orderitem.ItemTax + orderitem.ShippingTax + orderitem.GiftWrapTax);
                    }

                return taxtTotal;
            }
        }

        public double OrderShippingPrice
        {
            get
            {
                double shippingTotal = 0;

                if (OrderItems != null)
                    foreach (var orderitem in OrderItems)
                    {
                        shippingTotal += (double)orderitem.ShippingPrice;
                    }

                return shippingTotal;
            }
        }

        public decimal OrderDiscount
        {
            get
            {
                decimal orderDiscount = 0;

                if (OrderItems != null)
                    foreach (var orderitem in OrderItems)
                    {
                        orderDiscount += (decimal)orderitem.ShippingDiscount + orderitem.PromotionDiscount;
                    }

                return orderDiscount;
            }
        }

        public string CompanyName { get; set; }
    }
}
