using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class OrderListViewModel
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
        /// Gets or sets the name of the buyer
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// Gets or sets the shipping Name
        /// </summary>
        public string ShippingAddressName { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping first part of the address
        /// </summary>
        public string ShippingAddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the unique id of the order
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was created
        /// </summary>
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the order
        /// </summary>
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the flag whether this order was exported by the Scheduler
        /// </summary>
        public bool IsExported { get; set; }
    }
}
