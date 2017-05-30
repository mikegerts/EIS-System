using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BigCommerce4Net.Domain.Entities.Orders
{
    public enum ShippingProviderEnum
    {
        auspost = 0,
        canadapost = 1,
        endicia = 2,
        usps = 3,
        fedex = 4,
        royalmail = 5,
        ups = 6,
        upsready = 7,
        upsonline = 8,
        shipperhq = 9
    }
}
