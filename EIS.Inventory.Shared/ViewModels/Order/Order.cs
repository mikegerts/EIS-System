using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class Order
    {/// <summary>
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
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets the last update date of the order
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

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
        public int OrderStatus { get; set; }
        /// <summary>
        /// Gets or sets the Payment Status of the order
        /// </summary>
        public int PaymentStatus { get; set; }

        /// <summary>
        /// Get the total items ordered
        /// </summary>
        public int TotalItems { get; set; }

      

        public string OrderNote { get; set; }

        public DateTime? ShipmentDate { get; set; }

        public string CarrierCode { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public decimal ShipmentCost { get; set; }
        public bool HasOrderProducts { get; set; }
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the list of order items
        /// </summary>
        public List<OrderItem> OrderItems { get; set; }
    }
}