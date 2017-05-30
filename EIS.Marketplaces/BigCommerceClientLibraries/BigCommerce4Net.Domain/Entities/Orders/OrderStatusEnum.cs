using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BigCommerce4Net.Domain.Entities.Orders
{
    public enum OrderStatusEnum
    {
        Incomplete = 0,
        Pending = 1,
        Shipped = 2,
        Partially_Shipped = 3,
        Refunded = 4,
        Cancelled = 5,
        Declined = 6,
        Awaiting_Payment = 7,
        Awaiting_Pickup = 8,
        Awaiting_Shipment = 9,
        Completed = 10,
        Awaiting_Fulfillment = 11,
        Manual_Verification_Required = 12,
        Disputed = 13
    }
}
